using System.Collections.Generic;
using Game.Boost.Enums;
using Game.Boost.Managers;
using Game.Cars.Managers;
using Game.Configuration.Structure;
using Game.Simulation.Structure;
using UnityEngine;
using static Game.Boost.Constants.BoostConstants;
using static Game.Simulation.Constants.ProbeConstants;

namespace Game.Simulation.Logics
{
    public static class BoostContractProbe
    {
        public static BoostCheck Measure(BoostSettings settings, float baseSpeed, float stepTime, int level)
        {
            var boost = new BoostController();
            boost.Configure(settings);
            var motion = new CarMotion(boost, boost);
            motion.Configure(baseSpeed, NoFinish);
            boost.Open();
            boost.Request(level);
            var elapsed = 0f;
            var travelled = 0f;
            while (boost.IsActive)
            {
                travelled += motion.Advance(stepTime).Travelled;
                elapsed += stepTime;
            }
            var window = settings.windowDuration;
            var overrun = elapsed - boost.BoostedSeconds;
            var windowTravel = travelled - baseSpeed * overrun;
            return new BoostCheck(level, stepTime, baseSpeed,
                level * baseSpeed * window, windowTravel,
                (level - NeutralLevel) * baseSpeed * window, motion.BoostDistance,
                boost.BoostedSeconds, motion.Distance);
        }

        public static IReadOnlyList<RuleCheck> VerifyRules(BoostSettings settings, float baseSpeed, float stepTime)
        {
            return new[]
            {
                MidWindowRequestIgnored(settings, baseSpeed, stepTime),
                CooldownRejectsWithoutCost(settings, baseSpeed, stepTime),
                EmptyWalletRejectsWithoutCost(settings),
                ClosedWindowRejects(settings),
                NeutralLevelMatchesCoasting(settings, baseSpeed, stepTime),
                OutOfRangeLevelIsClamped(settings, baseSpeed, stepTime),
                FinishCrossingIsSubStepExact(settings, baseSpeed, stepTime)
            };
        }

        private static RuleCheck MidWindowRequestIgnored(BoostSettings settings, float baseSpeed, float stepTime)
        {
            var boost = new BoostController();
            boost.Configure(settings);
            var motion = new CarMotion(boost, boost);
            motion.Configure(baseSpeed, NoFinish);
            boost.Open();
            boost.Request(ProbeLevel);
            var energyAfterAccept = boost.Energy;
            var steps = Mathf.RoundToInt(settings.windowDuration * HalfWindow / stepTime);
            for (var step = 0; step < steps; step++)
                motion.Advance(stepTime);
            var outcome = boost.Request(MaxLevel);
            var levelHeld = boost.ActiveLevel == ProbeLevel;
            var regen = settings.energyRegenPerSecond * steps * stepTime;
            var energyUntouched = Mathf.Abs(boost.Energy - energyAfterAccept - regen) < EnergyTolerance;
            while (boost.IsActive)
                motion.Advance(stepTime);
            var expected = (ProbeLevel - NeutralLevel) * baseSpeed * settings.windowDuration;
            var distanceHeld = Mathf.Abs(motion.BoostDistance - expected) < expected * DistanceTolerance;
            return new RuleCheck("request during an active window is ignored",
                outcome == BoostRequestOutcome.WindowActive && levelHeld && energyUntouched && distanceHeld,
                $"outcome={outcome} level={boost.ActiveLevel} extra={motion.BoostDistance:0.###}m expected={expected:0.###}m");
        }

        private static RuleCheck CooldownRejectsWithoutCost(BoostSettings settings, float baseSpeed, float stepTime)
        {
            var boost = new BoostController();
            boost.Configure(settings);
            var motion = new CarMotion(boost, boost);
            motion.Configure(baseSpeed, NoFinish);
            boost.Open();
            boost.Request(MinLevel + 1);
            while (boost.IsActive)
                motion.Advance(stepTime);
            var energyBefore = boost.Energy;
            var blocked = boost.Request(MinLevel + 1);
            var untouched = Mathf.Abs(boost.Energy - energyBefore) < EnergyTolerance;
            while (boost.RemainingCooldown > 0f)
                motion.Advance(stepTime);
            var allowed = boost.Request(MinLevel + 1);
            return new RuleCheck("cooldown rejects without cost",
                blocked == BoostRequestOutcome.OnCooldown && untouched && allowed == BoostRequestOutcome.Accepted,
                $"duringCooldown={blocked} afterCooldown={allowed}");
        }

        private static RuleCheck EmptyWalletRejectsWithoutCost(BoostSettings settings)
        {
            var drained = new BoostSettings
            {
                windowDuration = settings.windowDuration,
                cooldown = settings.cooldown,
                energyCapacity = settings.energyCapacity,
                energyOnStart = 0f,
                energyRegenPerSecond = 0f,
                levelCosts = settings.levelCosts,
                levelColors = settings.levelColors
            };
            var boost = new BoostController();
            boost.Configure(drained);
            boost.Open();
            var outcome = boost.Request(MaxLevel);
            return new RuleCheck("insufficient energy rejects without cost",
                outcome == BoostRequestOutcome.InsufficientEnergy && boost.EnergySpent == 0f && !boost.IsActive,
                $"outcome={outcome} energy={boost.Energy:0.##} spent={boost.EnergySpent:0.##}");
        }

        private static RuleCheck ClosedWindowRejects(BoostSettings settings)
        {
            var boost = new BoostController();
            boost.Configure(settings);
            var beforeStart = boost.Request(MaxLevel);
            boost.Open();
            boost.Close();
            var afterFinish = boost.Request(MaxLevel);
            return new RuleCheck("countdown and post finish reject",
                beforeStart == BoostRequestOutcome.NotRunning && afterFinish == BoostRequestOutcome.NotRunning
                                                             && boost.EnergySpent == 0f,
                $"beforeStart={beforeStart} afterFinish={afterFinish}");
        }

        private static RuleCheck NeutralLevelMatchesCoasting(BoostSettings settings, float baseSpeed, float stepTime)
        {
            var check = Measure(settings, baseSpeed, stepTime, NeutralLevel);
            var coasting = Coast(settings, baseSpeed, stepTime, check.BoostedSeconds);
            return new RuleCheck("level 1 is the reference level",
                check.BoostedSeconds > 0f
                && Mathf.Abs(check.MeasuredWindowDistance - coasting) < coasting * DistanceTolerance,
                $"window={check.MeasuredWindowDistance:0.####}m coasting={coasting:0.####}m extra={check.MeasuredExtra:0.####}m");
        }

        private static RuleCheck OutOfRangeLevelIsClamped(BoostSettings settings, float baseSpeed, float stepTime)
        {
            var boost = new BoostController();
            boost.Configure(settings);
            var motion = new CarMotion(boost, boost);
            motion.Configure(baseSpeed, NoFinish);
            boost.Open();
            boost.Request(MaxLevel + OverflowLevel);
            var clampedHigh = boost.ActiveLevel == MaxLevel;
            while (boost.IsActive)
                motion.Advance(stepTime);
            while (boost.RemainingCooldown > 0f)
                motion.Advance(stepTime);
            boost.Request(MinLevel - OverflowLevel);
            var clampedLow = boost.ActiveLevel == MinLevel;
            return new RuleCheck("out of range levels clamp to 1..5", clampedHigh && clampedLow,
                $"above={clampedHigh} below={clampedLow}");
        }

        private static RuleCheck FinishCrossingIsSubStepExact(BoostSettings settings, float baseSpeed, float stepTime)
        {
            var finish = baseSpeed * CrossingSeconds + baseSpeed * HalfWindow * stepTime;
            var boost = new BoostController();
            boost.Configure(settings);
            var motion = new CarMotion(boost, boost);
            motion.Configure(baseSpeed, finish);
            boost.Open();
            var elapsed = 0f;
            var offset = 0f;
            var crossings = 0;
            while (elapsed < CrossingSeconds * CrossingOverrun)
            {
                var step = motion.Advance(stepTime);
                if (step.HasCrossed)
                {
                    offset = elapsed + step.CrossOffset;
                    crossings++;
                }
                elapsed += stepTime;
            }
            var exact = finish / baseSpeed;
            return new RuleCheck("finish crossing time is sub step exact",
                crossings == 1 && Mathf.Abs(offset - exact) < stepTime * CrossingTolerance,
                $"crossings={crossings} solved={offset:0.#####}s exact={exact:0.#####}s step={stepTime:0.#####}s");
        }

        private static float Coast(BoostSettings settings, float baseSpeed, float stepTime, float duration)
        {
            var boost = new BoostController();
            boost.Configure(settings);
            var motion = new CarMotion(boost, boost);
            motion.Configure(baseSpeed, NoFinish);
            var elapsed = 0f;
            var travelled = 0f;
            while (elapsed < duration - stepTime * HalfWindow)
            {
                travelled += motion.Advance(stepTime).Travelled;
                elapsed += stepTime;
            }
            return travelled;
        }
    }
}
