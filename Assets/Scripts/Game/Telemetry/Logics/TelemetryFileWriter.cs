using System.Globalization;
using System.IO;
using System.Text;
using Game.Configuration.Structure;
using Game.Telemetry.Interfaces;
using Game.Telemetry.Structure;
using UnityEngine;
using static Game.Telemetry.Constants.TelemetryConstants;

namespace Game.Telemetry.Logics
{
    public class TelemetryFileWriter : ITelemetryWriter
    {
        private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

        private readonly TelemetrySettings _settings;

        public TelemetryFileWriter(TelemetrySettings settings) => _settings = settings;

        public string Write(ITelemetryLog log, RaceReport report, string runName)
        {
            var directory = Path.IsPathRooted(_settings.outputDirectory)
                ? _settings.outputDirectory
                : Path.Combine(Application.persistentDataPath, _settings.outputDirectory);
            Directory.CreateDirectory(directory);
            if (_settings.writeCsv)
            {
                File.WriteAllText(Path.Combine(directory, runName + SampleFileSuffix), BuildSamples(log));
                File.WriteAllText(Path.Combine(directory, runName + EventFileSuffix), BuildEvents(log));
            }
            if (_settings.writeJson)
                File.WriteAllText(Path.Combine(directory, runName + ReportFileSuffix), JsonUtility.ToJson(report, true));
            return directory;
        }

        private static string BuildSamples(ITelemetryLog log)
        {
            var builder = new StringBuilder(log.Samples.Count * SampleLineBudget);
            builder.AppendLine(SampleHeader);
            foreach (var sample in log.Samples)
            {
                builder.Append(sample.time.ToString(TimeFormat, Culture)).Append(',')
                    .Append(sample.carIndex).Append(',')
                    .Append(log.CarNames[sample.carIndex]).Append(',')
                    .Append(sample.isPlayer ? 1 : 0).Append(',')
                    .Append(sample.rank).Append(',')
                    .Append(sample.distance.ToString(DistanceFormat, Culture)).Append(',')
                    .Append(sample.speed.ToString(DistanceFormat, Culture)).Append(',')
                    .Append(sample.boostLevel).Append(',')
                    .Append(sample.energy.ToString(EnergyFormat, Culture)).Append(',')
                    .Append(sample.balanceScale.ToString(ScaleFormat, Culture)).Append(',')
                    .Append(sample.assistDistance.ToString(EnergyFormat, Culture)).Append(',')
                    .Append(sample.boostDistance.ToString(EnergyFormat, Culture)).Append(',')
                    .Append(sample.gapToPlayer.ToString(EnergyFormat, Culture)).Append(',')
                    .Append(sample.gapToLeader.ToString(EnergyFormat, Culture)).Append(',')
                    .AppendLine(sample.aiState);
            }
            return builder.ToString();
        }

        private static string BuildEvents(ITelemetryLog log)
        {
            var builder = new StringBuilder(log.Events.Count * EventLineBudget);
            builder.AppendLine(EventHeader);
            foreach (var entry in log.Events)
            {
                builder.Append(entry.time.ToString(TimeFormat, Culture)).Append(',')
                    .Append(entry.carIndex).Append(',')
                    .Append(log.CarNames[entry.carIndex]).Append(',')
                    .Append(entry.kind).Append(',')
                    .Append(entry.level).Append(',')
                    .AppendLine(entry.detail);
            }
            return builder.ToString();
        }
    }
}
