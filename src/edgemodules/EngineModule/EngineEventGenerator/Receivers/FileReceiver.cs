using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EngineEventGenerator.Configuration;
using EngineEventGenerator.Interfaces;
using EngineEventGenerator.Models;
using Microsoft.Extensions.Options;
using Flurl;
using Flurl.Http;

namespace EngineEventGenerator.Receivers
{
    public class FileReceiver : ITelemetryReceiver
    {
        // Field that contains all field names
        private static string[] header;

        // This dictionary keeps track of the cycles for every engine
        private IDictionary<string, CycleState> _cycleStates;

        // The settings for the file reading
        private FileSettings _fileSettings;

        // Private list that contains all engine cycles from the read file (in memory)
        private List<EngineCycle> _engineCycles;
        private ITelemetryTransmitter _telemetryTransmitter;

        public FileReceiver(IOptions<FileSettings> filesettings, ITelemetryTransmitter telemetryTransmitter)
        {
            _fileSettings = filesettings.Value;
            _telemetryTransmitter = telemetryTransmitter;
        }

        public async Task Initialize()
        {
            var fileName = await EnsureFile();

            var firstLine = true;
            _engineCycles = new List<EngineCycle>();
            using var reader = File.OpenText(fileName);
            // Read through the entire file and add all cycles to the collection
            while (!reader.EndOfStream)
            {
                if (firstLine)
                {
                    header = (await reader.ReadLineAsync())?.Split(';');
                    firstLine = false;
                }
                else
                {
                    var line = await reader.ReadLineAsync();
                    var item = new EngineCycle();
                    var sensorData = line.Split(',');
                    item.EngineId = "engine" + int.Parse(sensorData[0]).ToString("000");
                    item.Cycle = Convert.ToInt32(sensorData[1]);
                    item.Data = line;
                    item.SensorData = sensorData;
                    _engineCycles.Add(item);
                }
            }

            // Initialize state for every engine
            _cycleStates = new Dictionary<string, CycleState>();
            foreach (var engineId in _engineCycles.Select(cycle => cycle.EngineId).Distinct())
            {
                _cycleStates.Add(engineId,
                    new CycleState
                    {
                        Direction = Direction.Up, NextCycle = 1,
                        Cycles = _engineCycles.Where(cycle =>
                            cycle.EngineId.Equals(engineId, StringComparison.CurrentCultureIgnoreCase))
                    });
            }
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            var maxCycle = _engineCycles.Max(item => item.Cycle);

            while (!cancellationToken.IsCancellationRequested)
            {
                var cycleList = new List<EngineCycle> { };
                cycleList.AddRange(
                    _cycleStates.Select(cycleState =>
                        cycleState.Value.PopCycle()));
                await _telemetryTransmitter.Transmit(header, cycleList, cancellationToken);
                await Task.Delay(_fileSettings.TransmitIntervalInMilliseconds, cancellationToken);
            }
        }

        private async Task<string> EnsureFile()
        {
            if (string.IsNullOrEmpty(_fileSettings.Filename))
            {
                throw new ArgumentException($"The file settings were not provided and no engine was found");
            }

            if (Uri.TryCreate(_fileSettings.Filename, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                var fileName = Guid.NewGuid().ToString() + ".csv";
                // Download from url
                await _fileSettings.Filename.DownloadFileAsync(".", fileName);
                return fileName;
            }
            else
            {
                if (!File.Exists(_fileSettings.Filename))
                {
                    throw new FileNotFoundException(
                        $"The engine file was not found at {Path.GetFullPath(_fileSettings.Filename)}");
                }

                return _fileSettings.Filename;
            }
        }
    }
}