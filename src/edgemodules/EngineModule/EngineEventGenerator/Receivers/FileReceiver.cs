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
            if (!File.Exists(_fileSettings.Filename))
            {
                throw new FileNotFoundException($"The engine file was not found at {Path.GetFullPath(_fileSettings.Filename)}");
            }
            var firstLine = true;
            _engineCycles = new List<EngineCycle>();
            using var reader = File.OpenText(_fileSettings.Filename);
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
    }
}