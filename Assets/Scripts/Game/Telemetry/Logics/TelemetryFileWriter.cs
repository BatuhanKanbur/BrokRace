using System.Globalization;
using System.IO;
using System.Text;
using Game.Configuration.Structure;
using Game.Telemetry.Interfaces;
using UnityEngine;
using static Game.Telemetry.Constants.TelemetryConstants;

namespace Game.Telemetry.Logics
{
    public class TelemetryFileWriter : ITelemetryWriter
    {
        private readonly TelemetrySettings _settings;

        public TelemetryFileWriter(TelemetrySettings settings) => _settings = settings;

        public string Write(ITelemetryRecorder recorder, string runName, string[] carNames)
        {
            var directory = Path.IsPathRooted(_settings.outputDirectory)
                ? _settings.outputDirectory
                : Path.Combine(Application.persistentDataPath, _settings.outputDirectory);
            Directory.CreateDirectory(directory);

            if (_settings.writeCsv)
            {
                File.WriteAllText(Path.Combine(directory, runName + SampleFileSuffix), BuildSamples(recorder, carNames));
                File.WriteAllText(Path.Combine(directory, runName + EventFileSuffix), BuildEvents(recorder, carNames));
            }
            if (_settings.writeJson)
            {
                File.WriteAllText(Path.Combine(directory, runName + ReportFileSuffix),
                    JsonUtility.ToJson(recorder.Complete(), true));
            }
            return directory;
        }

        private static string BuildSamples(ITelemetryRecorder recorder, string[] carNames)
        {
            var culture = CultureInfo.InvariantCulture;
            var builder = new StringBuilder(recorder.Samples.Count * 96);
            builder.AppendLine(SampleHeader);
            foreach (var sample in recorder.Samples)
            {
                builder.Append(sample.time.ToString("0.###", culture)).Append(',')
                    .Append(sample.carIndex).Append(',')
                    .Append(carNames[sample.carIndex]).Append(',')
                    .Append(sample.carIndex == 0 ? 1 : 0).Append(',')
                    .Append(sample.rank).Append(',')
                    .Append(sample.distance.ToString("0.###", culture)).Append(',')
                    .Append(sample.speed.ToString("0.###", culture)).Append(',')
                    .Append(sample.boostLevel).Append(',')
                    .Append(sample.energy.ToString("0.##", culture)).Append(',')
                    .Append(sample.balanceScale.ToString("0.####", culture)).Append(',')
                    .Append(sample.gapToPlayer.ToString("0.##", culture)).Append(',')
                    .Append(sample.gapToLeader.ToString("0.##", culture)).Append(',')
                    .AppendLine(sample.aiState);
            }
            return builder.ToString();
        }

        private static string BuildEvents(ITelemetryRecorder recorder, string[] carNames)
        {
            var culture = CultureInfo.InvariantCulture;
            var builder = new StringBuilder(recorder.Events.Count * 64);
            builder.AppendLine(EventHeader);
            foreach (var entry in recorder.Events)
            {
                builder.Append(entry.time.ToString("0.###", culture)).Append(',')
                    .Append(entry.carIndex).Append(',')
                    .Append(carNames[entry.carIndex]).Append(',')
                    .Append(entry.kind).Append(',')
                    .Append(entry.level).Append(',')
                    .AppendLine(entry.detail);
            }
            return builder.ToString();
        }
    }
}
