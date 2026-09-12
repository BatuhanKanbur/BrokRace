using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Game.Configuration.Structure;
using Game.Input.Managers;
using Game.Simulation.Logics;
using Game.Simulation.Managers;
using Game.Telemetry.Logics;
using Game.Telemetry.Structure;
using UnityEditor;
using UnityEngine;
using static Game.Boost.Constants.BoostConstants;
using static Game.Simulation.Constants.SimulationConstants;
using static Editor.Constants.ValidationConstants;

namespace Editor
{
    public static class RaceValidationRunner
    {
        private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

        public static void Run()
        {
            CultureInfo.DefaultThreadCurrentCulture = Culture;
            CultureInfo.CurrentCulture = Culture;
            var config = AssetDatabase.LoadAssetAtPath<RaceConfig>(ConfigPath);
            var output = Path.Combine(Directory.GetParent(Application.dataPath).FullName, ValidationFolder);
            Directory.CreateDirectory(output);
            config.telemetry.outputDirectory = output;

            WriteBoostContract(config, output);
            WriteRaceMatrix(config, output);
            WriteFrameRateTable(config, output);
            WriteLifecycle(config, output);
            Debug.Log($"[RaceValidationRunner] written to {output} DONE");
        }

        private static void WriteBoostContract(RaceConfig config, string output)
        {
            var builder = new StringBuilder();
            builder.AppendLine(ContractHeader);
            foreach (var step in ProbeSteps)
            {
                for (var level = MinLevel; level <= MaxLevel; level++)
                {
                    var check = BoostContractProbe.Measure(config.boost, config.race.baseSpeed, step, level);
                    builder.Append(check.Level).Append(',')
                        .Append(Number(1f / step)).Append(',')
                        .Append(Number(check.BaseSpeed)).Append(',')
                        .Append(Number(check.ExpectedWindowDistance)).Append(',')
                        .Append(Number(check.MeasuredWindowDistance)).Append(',')
                        .Append(Number(check.ExpectedExtra)).Append(',')
                        .Append(Number(check.MeasuredExtra)).Append(',')
                        .Append(Number(check.BoostedSeconds)).Append(',')
                        .AppendLine(check.DeviationPercent.ToString(PercentFormat, Culture));
                }
            }
            File.WriteAllText(Path.Combine(output, ContractFile), builder.ToString());

            var rules = new StringBuilder();
            rules.AppendLine(RuleHeader);
            foreach (var check in BoostContractProbe.VerifyRules(config.boost, config.race.baseSpeed, config.race.logicStep))
                rules.Append(check.Name).Append(',').Append(check.Passed ? PassLabel : FailLabel).Append(',')
                    .AppendLine(check.Detail);
            File.WriteAllText(Path.Combine(output, RuleFile), rules.ToString());
        }

        private static void WriteRaceMatrix(RaceConfig config, string output)
        {
            var writer = new TelemetryFileWriter(config.telemetry);
            var summary = new StringBuilder();
            summary.AppendLine(MatrixHeader);
            var rivals = new StringBuilder();
            rivals.AppendLine(RivalHeader);
            foreach (var scenario in Scenarios)
            {
                foreach (var seed in Seeds)
                {
                    var simulation = new RaceSimulation(config);
                    var input = new ScriptedBoostInput(ScenarioBuilder.Build(scenario, config, seed));
                    var report = simulation.Run(seed, scenario, input, config.race.logicStep);
                    writer.Write(simulation.Log, report, $"{scenario}_{seed}");
                    AppendSummary(summary, report);
                    AppendRivals(rivals, report);
                }
            }
            File.WriteAllText(Path.Combine(output, MatrixFile), summary.ToString());
            File.WriteAllText(Path.Combine(output, RivalFile), rivals.ToString());
        }

        private static void WriteFrameRateTable(RaceConfig config, string output)
        {
            var builder = new StringBuilder();
            builder.AppendLine(FrameHeader);
            foreach (var seed in Seeds)
            {
                var baseline = default(RaceReport);
                foreach (var frameRate in FrameRates)
                {
                    var simulation = new RaceSimulation(config);
                    var input = new ScriptedBoostInput(ScenarioBuilder.Build(BalancedScenario, config, seed));
                    var report = simulation.RunFramed(seed, BalancedScenario, input, 1f / frameRate);
                    baseline ??= report;
                    builder.Append(seed).Append(',')
                        .Append(frameRate).Append(',')
                        .Append(Number(report.playerFinishTime)).Append(',')
                        .Append(report.playerFinishRank).Append(',')
                        .Append(Number(report.playerBoostDistance)).Append(',')
                        .Append(Number(report.playerExpectedBoostDistance)).Append(',')
                        .Append(Deviation(report.playerBoostDistance, baseline.playerBoostDistance)).Append(',')
                        .Append(Deviation(report.playerFinishTime, baseline.playerFinishTime)).Append(',')
                        .AppendLine(Number(report.finalGapToLeader));
                }
            }
            File.WriteAllText(Path.Combine(output, FrameFile), builder.ToString());
        }

        private static void WriteLifecycle(RaceConfig config, string output)
        {
            var builder = new StringBuilder();
            builder.AppendLine(RuleHeader);
            foreach (var check in LifecycleProbe.Verify(config, Seeds[0]))
                builder.Append(check.Name).Append(',').Append(check.Passed ? PassLabel : FailLabel).Append(',')
                    .AppendLine(check.Detail);
            File.WriteAllText(Path.Combine(output, LifecycleFile), builder.ToString());
        }

        private static void AppendSummary(StringBuilder builder, RaceReport report)
        {
            var player = FindPlayer(report);
            builder.Append(report.run.scenario).Append(',')
                .Append(report.run.seed).Append(',')
                .Append(Number(report.playerFinishTime)).Append(',')
                .Append(report.playerFinishRank).Append(',')
                .Append(player.acceptedBoosts).Append(',')
                .Append(player.rejectedBoosts).Append(',')
                .Append(Number(player.energySpent)).Append(',')
                .Append(Number(player.energyWasted)).Append(',')
                .Append(Number(report.playerBoostDistance)).Append(',')
                .Append(Number(report.playerExpectedBoostDistance)).Append(',')
                .Append(Number(report.playerAssistDistance)).Append(',')
                .Append(report.playerOvertakesMade).Append(',')
                .Append(report.playerOvertakesConceded).Append(',')
                .Append(Number(report.finalGapAhead)).Append(',')
                .Append(Number(report.finalGapBehind)).Append(',')
                .Append(Number(report.finalGapToLeader)).Append(',')
                .Append(Number(report.averageGapAheadLastFifth)).Append(',')
                .Append(Number(report.averageGapBehindLastFifth)).Append(',')
                .Append(Number(report.averageGapToLeaderLastFifth)).Append(',')
                .Append(Number(report.packSpreadAtWinnerFinish)).Append(',')
                .Append(Number(report.averagePackSpread)).Append(',')
                .Append(Number(report.maxPackSpread)).Append(',')
                .AppendLine(report.leadChanges.ToString(Culture));
        }

        private static void AppendRivals(StringBuilder builder, RaceReport report)
        {
            foreach (var car in report.cars)
            {
                builder.Append(report.run.scenario).Append(',')
                    .Append(report.run.seed).Append(',')
                    .Append(car.name).Append(',')
                    .Append(car.profile).Append(',')
                    .Append(car.isPlayer ? 1 : 0).Append(',')
                    .Append(car.finishOrder).Append(',')
                    .Append(Number(car.finishTime)).Append(',')
                    .Append(Number(car.finishTime - report.playerFinishTime)).Append(',')
                    .Append(car.acceptedBoosts).Append(',')
                    .Append(car.rejectedBoosts).Append(',')
                    .Append(Number(car.energySpent)).Append(',')
                    .Append(Number(car.energyWasted)).Append(',')
                    .Append(Number(car.boostDistance)).Append(',')
                    .Append(Number(car.assistDistance)).Append(',')
                    .AppendLine(Number(car.averageSpeed));
            }
        }

        private static CarSummary FindPlayer(RaceReport report)
        {
            foreach (var car in report.cars)
                if (car.isPlayer)
                    return car;
            return report.cars[0];
        }

        private static string Number(float value) => value.ToString(NumberFormat, Culture);

        private static string Deviation(float value, float reference) => reference != 0f
            ? (Mathf.Abs(value - reference) / reference * PercentScale).ToString(PercentFormat, Culture)
            : ZeroLabel;

        private static readonly IReadOnlyList<string> Scenarios = new[]
        {
            BalancedScenario, NoBoostScenario, MaxSpamScenario, EarlyThenPassiveScenario
        };
    }
}
