using System.Collections.Generic;
using Core.Utilities;
using Game.Configuration.Structure;
using Game.Input.Structure;
using UnityEngine;
using static Game.Boost.Constants.BoostConstants;
using static Game.Simulation.Constants.SimulationConstants;

namespace Game.Simulation.Logics
{
    public static class ScenarioBuilder
    {
        public static IReadOnlyList<ScriptedBoostEvent> Build(string scenario, RaceConfig config, int seed) =>
            scenario switch
            {
                NoBoostScenario => new ScriptedBoostEvent[0],
                MaxSpamScenario => Spam(config, MaxLevel),
                EarlyThenPassiveScenario => EarlyBurst(config),
                _ => Balanced(config, seed)
            };

        private static IReadOnlyList<ScriptedBoostEvent> Spam(RaceConfig config, int level)
        {
            var events = new List<ScriptedBoostEvent>();
            for (var time = 0f; time < MaxRaceSeconds; time += SpamInterval)
                events.Add(new ScriptedBoostEvent(time, level));
            return events;
        }

        private static IReadOnlyList<ScriptedBoostEvent> EarlyBurst(RaceConfig config)
        {
            var events = new List<ScriptedBoostEvent>();
            var window = config.boost.windowDuration + config.boost.cooldown + CycleMargin;
            var until = config.track.raceDistance * EarlyAttackProgress / config.race.baseSpeed;
            for (var time = 0f; time < until; time += window)
                events.Add(new ScriptedBoostEvent(time, MaxLevel));
            return events;
        }

        private static IReadOnlyList<ScriptedBoostEvent> Balanced(RaceConfig config, int seed)
        {
            var events = new List<ScriptedBoostEvent>();
            var random = DeterministicRandom.Stream(seed, MaxLevel);
            var settings = config.boost;
            var cycle = settings.windowDuration + settings.cooldown + CycleMargin;
            var energy = settings.energyOnStart;
            var clock = 0f;
            while (clock < MaxRaceSeconds)
            {
                var level = PickLevel(random);
                var cost = settings.levelCosts[level - MinLevel];
                var wait = cost > energy ? (cost - energy) / settings.energyRegenPerSecond : 0f;
                var press = clock + wait;
                energy = Mathf.Min(settings.energyCapacity, energy + wait * settings.energyRegenPerSecond) - cost;
                events.Add(new ScriptedBoostEvent(press, level));
                energy = Mathf.Min(settings.energyCapacity, energy + cycle * settings.energyRegenPerSecond);
                clock = press + cycle;
            }
            return events;
        }

        private static int PickLevel(DeterministicRandom random)
        {
            var roll = random.NextFloat();
            if (roll < EfficientShare) return MinLevel + 1;
            if (roll < MidShare) return MinLevel + 2;
            if (roll < HeavyShare) return MinLevel + 3;
            return MaxLevel;
        }
    }
}
