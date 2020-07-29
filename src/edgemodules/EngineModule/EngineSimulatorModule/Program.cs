using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using EngineEventGenerator.Configuration;
using EngineEventGenerator.Interfaces;
using EngineEventGenerator.Receivers;
using EngineEventGenerator.Transmitters;

namespace EngineSimulatorModule
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                // create service collection
                var services = new ServiceCollection();
                ConfigureServices(services);

                // create service provider
                var serviceProvider = services.BuildServiceProvider();

                // entry to run app
                await serviceProvider.GetService<App>().Run(args);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // configure logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
            });
    
            // build config
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.dev.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            services.Configure<FileSettings>(configuration.GetSection("file"));
            services.Configure<IoTHubSettings>(configuration.GetSection("iothub"));

            // add services:
            services.AddTransient<ITelemetryReceiver, FileReceiver>();
            services.AddTransient<ITelemetryTransmitter, ConsoleTransmitter>();

            // add app
            services.AddTransient<App>();
        }
    }
}