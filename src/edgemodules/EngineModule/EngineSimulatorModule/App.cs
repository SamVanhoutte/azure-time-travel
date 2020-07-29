using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EngineSimulatorModule
{
    public class App
    {
        private readonly ILogger<App> _logger;

        public App(ILogger<App> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Run(string[] args)
        {
            _logger.LogInformation($"Starting with injected App {nameof(App)}");

            await Task.CompletedTask;
        }
    }
}