using System.Collections.Generic;
using Game.Boost.Managers;
using Game.Cars.Managers;
using Game.Cars.Structure;
using Game.Configuration.Structure;
using Game.Input.Managers;
using Game.Race.Structure;
using Game.Simulation.Managers;
using Game.Simulation.Structure;
using Game.Standings.Managers;
using UnityEngine;
using static Game.Boost.Constants.BoostConstants;
using static Game.Simulation.Constants.ProbeConstants;
using static Game.Simulation.Constants.SimulationConstants;

namespace Game.Simulation.Logics
{
    public static class LifecycleProbe
    {
        public static IReadOnlyList<RuleCheck> Verify(RaceConfig config, int seed)
        {
            return new[]
            {
                BoostSurvivesTheFinishLine(config),
                SameStepCrossingsOrderByTime(config),
                RestartsAreRepeatable(config, seed)
            };
        }

        private static RuleCheck BoostSurvivesTheFinishLine(RaceConfig config)
        {
            var speed = config.race.baseSpeed;
            var step = config.race.logicStep;
            var finish = speed * CrossingSeconds + speed * (MaxLevel - NeutralLevel) * HalfWindow;
            var boost = new BoostController();
            boost.Configure(config.boost);
            var motion = new CarMotion(boost, boost);
            motion.Configure(speed, finish);
            boost.Open();
            var elapsed = 0f;
            var offset = NoCrossingTime;
            var crossings = 0;
            var boostedAtCrossing = false;
            while (elapsed < CrossingSeconds * CrossingOverrun)
            {
                if (Mathf.Abs(elapsed - CrossingSeconds + HalfWindow) < step * HalfWindow) boost.Request(MaxLevel);
                var wasActive = boost.IsActive;
                var advance = motion.Advance(step);
                if (advance.HasCrossed)
                {
                    offset = elapsed + advance.CrossOffset;
                    boostedAtCrossing = wasActive;
                    crossings++;
                }
                elapsed += step;
            }
            return new RuleCheck("a boost running into the finish line crosses once and keeps the contract",
                crossings == 1 && boostedAtCrossing && motion.Distance > finish,
                $"crossings={crossings} boosting={boostedAtCrossing} at={offset:0.####}s extra={motion.BoostDistance:0.##}m");
        }

        private static RuleCheck SameStepCrossingsOrderByTime(RaceConfig config)
        {
            var standings = new StandingsManager();
            var early = new SimulatedCar();
            var late = new SimulatedCar();
            var cars = new List<Game.Cars.Interfaces.ICar> { late, early };
            standings.Register(cars);
            late.Initialize(Setup(0, config, config.race.baseSpeed));
            early.Initialize(Setup(1, config, config.race.baseSpeed * SameStepSpeedRatio));
            standings.Reset();

            var step = config.race.logicStep;
            var stepStart = config.race.baseSpeed;
            var crossings = new List<FinishCrossing>
            {
                new(late, step * LateCrossFraction),
                new(early, step * EarlyCrossFraction)
            };
            standings.ReportCrossings(crossings, stepStart);

            var byTime = early.FinishOrder == 1 && late.FinishOrder == 2;
            var separated = late.FinishTime - early.FinishTime > 0f;
            return new RuleCheck("two cars crossing in one sub step are ordered by crossing time",
                byTime && separated,
                $"reportedFirst=car{late.Index} winner=car{(byTime ? early.Index : late.Index)} " +
                $"early={early.FinishTime:0.######}s late={late.FinishTime:0.######}s gap={(late.FinishTime - early.FinishTime) * 1000f:0.###}ms");
        }

        private static RuleCheck RestartsAreRepeatable(RaceConfig config, int seed)
        {
            var simulation = new RaceSimulation(config);
            var first = simulation.Run(seed, BalancedScenario,
                new ScriptedBoostInput(ScenarioBuilder.Build(BalancedScenario, config, seed)), config.race.logicStep);
            var firstTime = first.playerFinishTime;
            var firstRank = first.playerFinishRank;
            var firstBoost = first.playerBoostDistance;
            var second = simulation.Run(seed, BalancedScenario,
                new ScriptedBoostInput(ScenarioBuilder.Build(BalancedScenario, config, seed)), config.race.logicStep);
            var identical = Mathf.Approximately(firstTime, second.playerFinishTime)
                            && firstRank == second.playerFinishRank
                            && Mathf.Approximately(firstBoost, second.playerBoostDistance);
            return new RuleCheck("back to back restarts reproduce the race exactly", identical,
                $"first={firstTime:0.####}s rank {firstRank} boost {firstBoost:0.##}m, " +
                $"second={second.playerFinishTime:0.####}s rank {second.playerFinishRank} " +
                $"boost {second.playerBoostDistance:0.##}m");
        }

        private static CarSetup Setup(int index, RaceConfig config, float speed) =>
            new(index, $"probe{index}", false, Color.white, 0f, speed, config.track.raceDistance, config.boost);
    }
}
