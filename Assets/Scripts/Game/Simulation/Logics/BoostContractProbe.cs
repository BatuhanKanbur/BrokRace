using System.Collections.Generic;
using Game.Boost.Enums;
using Game.Boost.Managers;
using Game.Cars.Managers;
using Game.Configuration.Structure;
using Game.Simulation.Structure;
using UnityEngine;
using static Game.Boost.Constants.BoostConstants;

namespace Game.Simulation.Logics
{
    public static class BoostContractProbe
    {
        private const float SettleFactor = 2.5f;
        private const float NoFinish = float.MaxValue;

        public static BoostCheck Measure(BoostSettings settings, float baseSpeed, float stepTime, int level)
        {
            var boost = new BoostController(settings);
            var motion = new CarMotion(boost, boost, baseSpeed, NoFinish);
            boost.Open();
            boost.Request(level);
            var elapsed = 0f;
            var limit = settings.windowDuration * SettleFactor;
            while (elapsed < limit)
            {
                motion.Advance(stepTime);
                elapsed += stepTime;
            }
            var window = settings.windowDuration;
            var expectedExtra = (level - NeutralLevel) * baseSpeed * window;
            return new BoostCheck(level, stepTime, baseSpeed,
                level * baseSpeed * window,
                baseSpeed * window + motion.BoostDistance,
                expectedExtra, motion.BoostDistance, boost.BoostedSeconds);
        }

        public static IReadOnlyList<RuleCheck> VerifyRules(BoostSettings settings, float baseSpeed, float stepTime)
        {
            return new[]
            {
                MidWindowRequestIgnored(settings, baseSpeed, stepTime),
                CooldownBlocksRequest(settings, baseSpeed, stepTime),
                EmptyWalletRejectsWithoutCost(settings),
                ClosedWindowRejects(settings),
                NeutralLevelGivesNoGain(settings, baseSpeed, stepTime)
            };
        }

        private static RuleCheck MidWindowRequestIgnored(BoostSettings settings, float baseSpeed, float stepTime)
        {
            var boost = new BoostController(settings);
            var motion = new CarMotion(boost, boost, baseSpeed, NoFinish);
            boost.Open();
            boost.Request(3);
            var energyAfterFirst = boost.Energy;
            var steps = (int)(settings.windowDuration * 0.5f / stepTime);
            for (var step = 0; step < steps; step++)
                motion.Advance(stepTime);
            var outcome = boost.Request(5);
            var remaining = boost.RemainingWindow;
            var expectedRemaining = settings.windowDuration - steps * stepTime;
            while (boost.IsActive)
                motion.Advance(stepTime);
            var expectedExtra = 2f * baseSpeed * settings.windowDuration;
            var held = outcome == BoostRequestOutcome.WindowActive
                       && Mathf.Approximately(boost.Energy, energyAfterFirst + settings.energyRegenPerSecond * steps * stepTime)
                       && Mathf.Abs(remaining - expectedRemaining) < stepTime
                       && Mathf.Abs(motion.BoostDistance - expectedExtra) < expectedExtra * 0.0001f;
            return new RuleCheck("request during active window is ignored", held,
                $"outcome={outcome} level stayed 3, extra={motion.BoostDistance:0.###}m expected={expectedExtra:0.###}m");
        }

        private static RuleCheck CooldownBlocksRequest(BoostSettings settings, float baseSpeed, float stepTime)
        {
            var boost = new BoostController(settings);
            var motion = new CarMotion(boost, boost, baseSpeed, NoFinish);
            boost.Open();
            boost.Request(2);
            while (boost.IsActive)
                motion.Advance(stepTime);
            var energyBefore = boost.Energy;
            var blocked = boost.Request(2);
            var unchanged = Mathf.Approximately(boost.Energy, energyBefore);
            while (boost.RemainingCooldown > 0f)
                motion.Advance(stepTime);
            var allowed = boost.Request(2);
            return new RuleCheck("cooldown rejects without cost", blocked == BoostRequestOutcome.OnCooldown
                                                                  && unchanged
                                                                  && allowed == BoostRequestOutcome.Accepted,
                $"during cooldown={blocked}, after cooldown={allowed}");
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
                levelCosts = settings.levelCosts
            };
            var boost = new BoostController(drained);
            boost.Open();
            var outcome = boost.Request(MaxLevel);
            return new RuleCheck("insufficient energy rejects without cost",
                outcome == BoostRequestOutcome.InsufficientEnergy && Mathf.Approximately(boost.Energy, 0f)
                                                                 && boost.EnergySpent == 0f,
                $"outcome={outcome} energy={boost.Energy:0.##} spent={boost.EnergySpent:0.##}");
        }

        private static RuleCheck ClosedWindowRejects(BoostSettings settings)
        {
            var boost = new BoostController(settings);
            var beforeStart = boost.Request(MaxLevel);
            boost.Open();
            boost.Close();
            var afterFinish = boost.Request(MaxLevel);
            return new RuleCheck("countdown and post finish reject",
                beforeStart == BoostRequestOutcome.NotRunning && afterFinish == BoostRequestOutcome.NotRunning,
                $"beforeStart={beforeStart} afterFinish={afterFinish}");
        }

        private static RuleCheck NeutralLevelGivesNoGain(BoostSettings settings, float baseSpeed, float stepTime)
        {
            var check = Measure(settings, baseSpeed, stepTime, NeutralLevel);
            return new RuleCheck("level 1 is the reference level",
                Mathf.Approximately(check.MeasuredExtra, 0f)
                && Mathf.Approximately(check.MeasuredWindowDistance, baseSpeed * settings.windowDuration),
                $"extra={check.MeasuredExtra:0.####}m window={check.MeasuredWindowDistance:0.###}m");
        }
    }
}
