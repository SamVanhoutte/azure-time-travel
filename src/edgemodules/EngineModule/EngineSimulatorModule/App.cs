using System;
using System.Threading;
using System.Threading.Tasks;
using EngineEventGenerator.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EngineSimulatorModule
{
    public class App
    {
        private readonly ILogger<App> _logger;
        private readonly ITelemetryReceiver _telemetryReceiver;
        private readonly ITelemetryTransmitter _telemetryTransmitter;

        public App(ILogger<App> logger, ITelemetryReceiver receiver, ITelemetryTransmitter transmitter)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _telemetryReceiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
            _telemetryTransmitter = transmitter ?? throw new ArgumentNullException(nameof(transmitter));
        }

        public async Task Run(string[] args)
        {
            _logger.LogInformation($"Starting with injected App {nameof(App)}");

            await _telemetryReceiver.Initialize();
            await _telemetryReceiver.Run(CancellationToken.None);
        }
    }
}