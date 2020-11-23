using System;
using Arcus.EventGrid.Publishing;
using Arcus.EventGrid.Publishing.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;

namespace MaintenanceEventRouter
{
    internal static class ArcusEventGridClient
    {
        private static IEventGridPublisher _publisher;
        private static ExecutionContext _functionContext;

        public static void SaveContext(ExecutionContext context)
        {
            _functionContext = context;
        }

        public static IEventGridPublisher Publisher
        {
            get
            {
                if (_publisher != null) return _publisher;

                var config = new ConfigurationBuilder()
                    .SetBasePath(_functionContext.FunctionAppDirectory)
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();

                _publisher = EventGridPublisherBuilder
                    .ForTopic(config["event-grid-topic-name"])
                    .UsingAuthenticationKey(config["event-grid-authentication-key"])
                    .WithExponentialRetry<Exception>(3)
                    .Build();
                return _publisher;
            }
        }
    }
}