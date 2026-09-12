using System.Collections.Generic;
using Game.Ai.Interfaces;
using Game.Boost.Enums;
using Game.Cars.Interfaces;
using Game.Configuration.Structure;
using Game.Race.Interfaces;
using Game.Standings.Interfaces;
using Game.Telemetry.Interfaces;
using Game.Telemetry.Structure;
using UnityEngine;
using static Game.Telemetry.Constants.TelemetryConstants;

namespace Game.Telemetry.Managers
{
    public class TelemetryRecorder : ITelemetryRecorder, ITelemetryLog
    {
        private readonly TelemetrySettings _settings;
        private readonly IRaceState _race;
        private readonly IRaceOrder _order;
        private readonly Dictionary<int, IAiDriver> _drivers = new();
        private readonly List<CarSample> _samples = new();
        private readonly List<RaceEvent> _events = new();
        private readonly List<string> _carNames = new();
        private readonly Dictionary<int, bool> _wasAhead = new();

        private RaceRunInfo _info;
        private float _sampleTimer;
        private float _spreadSum;
        private float _spreadMax;
        private float _spreadLast;
        private int _spreadCount;
        private int _overtakesMade;
        private int _overtakesConceded;
        private int _leadChanges;
        private int _lastLeaderIndex;

        public TelemetryRecorder(TelemetrySettings settings, IRaceState race, IRaceOrder order,
            IReadOnlyList<IAiDriver> drivers)
        {
            _settings = settings;
            _race = race;
            _order = order;
            foreach (var driver in drivers)
                _drivers[driver.CarIndex] = driver;
            foreach (var car in race.Cars)
                _carNames.Add(car.DisplayName);
        }

        public IReadOnlyList<CarSample> Samples => _samples;
        public IReadOnlyList<RaceEvent> Events => _events;
        public IReadOnlyList<string> CarNames => _carNames;

        public void Begin(RaceRunInfo info)
        {
            Reset();
            _info = info;
            foreach (var car in _race.Cars)
                _wasAhead[car.Index] = false;
            Sample();
        }

        public void Step(float stepTime)
        {
            TrackOvertakes();
            _sampleTimer += stepTime;
            if (_sampleTimer < _settings.sampleInterval) return;
            _sampleTimer -= _settings.sampleInterval;
            TrackLeader();
            TrackSpread();
            Sample();
        }

        public void RecordRequest(ICarProgress car, int level, BoostRequestOutcome outcome) => _events.Add(new RaceEvent
        {
            time = _race.Time,
            carIndex = car.Index,
            kind = outcome == BoostRequestOutcome.Accepted ? BoostAccepted : BoostRejected,
            level = level,
            detail = outcome.ToString()
        });

        public void RecordFinish(ICarProgress car) => _events.Add(new RaceEvent
        {
            time = car.FinishTime,
            carIndex = car.Index,
            kind = Finish,
            level = car.FinishOrder,
            detail = car.FinishTime.ToString(TimeFormat)
        });

        public RaceReport Complete()
        {
            TrackSpread();
            Sample();
            var player = _race.Player;
            var report = new RaceReport
            {
                run = _info,
                cars = BuildSummaries(),
                playerOvertakesMade = _overtakesMade,
                playerOvertakesConceded = _overtakesConceded,
                playerFinishTime = player.FinishTime,
                playerFinishRank = player.FinishOrder,
                leadChanges = _leadChanges,
                playerBoostDistance = player.BoostDistance,
                playerExpectedBoostDistance = player.Ledger.ExtraLevelSum * player.NaturalSpeed * _info.boostWindow,
                playerAssistDistance = player.AssistDistance,
                averagePackSpread = _spreadCount > 0 ? _spreadSum / _spreadCount : 0f,
                maxPackSpread = _spreadMax,
                packSpreadAtFinish = _spreadLast
            };
            FillGapStatistics(report);
            return report;
        }

        public void Reset()
        {
            _info = null;
            _samples.Clear();
            _events.Clear();
            _wasAhead.Clear();
            _sampleTimer = 0f;
            _spreadSum = 0f;
            _spreadMax = 0f;
            _spreadLast = 0f;
            _spreadCount = 0;
            _overtakesMade = 0;
            _overtakesConceded = 0;
            _leadChanges = 0;
            _lastLeaderIndex = -1;
        }

        private void Sample()
        {
            var player = _race.Player;
            var leader = _order.Leader;
            foreach (var car in _race.Cars)
            {
                _samples.Add(new CarSample
                {
                    time = _race.Time,
                    carIndex = car.Index,
                    isPlayer = car.IsPlayer,
                    rank = _order.RankOf(car),
                    distance = car.Distance,
                    speed = car.Speed,
                    boostLevel = car.BoostState.ActiveLevel,
                    energy = car.BoostState.Energy,
                    balanceScale = car.BalanceScale,
                    assistDistance = car.AssistDistance,
                    boostDistance = car.BoostDistance,
                    gapToPlayer = car.Distance - player.Distance,
                    gapToLeader = leader.Distance - car.Distance,
                    aiState = Describe(car)
                });
            }
        }

        private string Describe(ICarProgress car)
        {
            if (!_drivers.TryGetValue(car.Index, out var driver)) return PlayerState;
            var decision = driver.LastDecision;
            return $"{driver.ProfileName}:{decision.Level} s{decision.Strike:0.00} d{decision.Defend:0.00} " +
                   $"p{decision.Pace:0.00} c{decision.Closing:0.00} e{decision.Spill:0.00}";
        }

        private void TrackOvertakes()
        {
            var player = _race.Player;
            foreach (var car in _race.Cars)
            {
                if (car.IsPlayer) continue;
                var gap = car.Distance - player.Distance;
                if (_wasAhead[car.Index])
                {
                    if (gap > -OvertakeHysteresis) continue;
                    _wasAhead[car.Index] = false;
                    _overtakesMade++;
                    AddSwap(Overtake, car);
                    continue;
                }
                if (gap < OvertakeHysteresis) continue;
                _wasAhead[car.Index] = true;
                _overtakesConceded++;
                AddSwap(Conceded, car);
            }
        }

        private void AddSwap(string kind, ICarProgress car) => _events.Add(new RaceEvent
        {
            time = _race.Time,
            carIndex = car.Index,
            kind = kind,
            level = 0,
            detail = car.DisplayName
        });

        private void TrackLeader()
        {
            var leader = _order.Leader.Index;
            if (leader == _lastLeaderIndex) return;
            if (_lastLeaderIndex >= 0) _leadChanges++;
            _lastLeaderIndex = leader;
        }

        private void TrackSpread()
        {
            var front = float.MinValue;
            var back = float.MaxValue;
            foreach (var car in _race.Cars)
            {
                var reached = Mathf.Min(car.Distance, _race.RaceDistance);
                front = Mathf.Max(front, reached);
                back = Mathf.Min(back, reached);
            }
            _spreadLast = front - back;
            _spreadSum += _spreadLast;
            _spreadMax = Mathf.Max(_spreadMax, _spreadLast);
            _spreadCount++;
        }

        private CarSummary[] BuildSummaries()
        {
            var summaries = new CarSummary[_race.Cars.Count];
            for (var index = 0; index < summaries.Length; index++)
            {
                var car = _race.Cars[index];
                summaries[index] = new CarSummary
                {
                    carIndex = car.Index,
                    name = car.DisplayName,
                    profile = _drivers.TryGetValue(car.Index, out var driver) ? driver.ProfileName : PlayerState,
                    isPlayer = car.IsPlayer,
                    finishOrder = car.FinishOrder,
                    finishTime = car.FinishTime,
                    acceptedBoosts = car.Ledger.AcceptedCount,
                    rejectedBoosts = car.Ledger.RejectedCount,
                    energySpent = car.Ledger.EnergySpent,
                    energyWasted = car.Ledger.EnergyWasted,
                    boostedSeconds = car.Ledger.BoostedSeconds,
                    boostDistance = car.BoostDistance,
                    assistDistance = car.AssistDistance,
                    averageSpeed = car.FinishTime > 0f ? _info.raceDistance / car.FinishTime : 0f
                };
            }
            return summaries;
        }

        private void FillGapStatistics(RaceReport report)
        {
            var player = _race.Player;
            var threshold = _info.raceDistance * FinalStretchFraction;
            var carCount = _race.Cars.Count;
            var aheadSum = 0f;
            var behindSum = 0f;
            var leaderSum = 0f;
            var count = 0;
            for (var block = 0; block + carCount <= _samples.Count; block += carCount)
            {
                var playerSample = _samples[block + player.Index];
                if (playerSample.distance < threshold) continue;
                var nearestAhead = float.MaxValue;
                var nearestBehind = float.MaxValue;
                for (var offset = 0; offset < carCount; offset++)
                {
                    if (offset == player.Index) continue;
                    var gap = _samples[block + offset].distance - playerSample.distance;
                    if (gap > 0f) nearestAhead = Mathf.Min(nearestAhead, gap);
                    else nearestBehind = Mathf.Min(nearestBehind, -gap);
                }
                aheadSum += nearestAhead < float.MaxValue ? nearestAhead : 0f;
                behindSum += nearestBehind < float.MaxValue ? nearestBehind : 0f;
                leaderSum += playerSample.gapToLeader;
                count++;
            }
            report.finalGapToLeader = _order.Leader.Distance - player.Distance;
            var ahead = _order.CarAhead(player);
            var behind = _order.CarBehind(player);
            report.finalGapAhead = ahead != null ? ahead.Distance - player.Distance : 0f;
            report.finalGapBehind = behind != null ? player.Distance - behind.Distance : 0f;
            if (count == 0) return;
            report.averageGapAheadLastFifth = aheadSum / count;
            report.averageGapBehindLastFifth = behindSum / count;
            report.averageGapToLeaderLastFifth = leaderSum / count;
        }
    }
}
