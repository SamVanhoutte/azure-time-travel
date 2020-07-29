using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EngineEventGenerator.Interfaces;
using EngineEventGenerator.Models;
using Newtonsoft.Json.Linq;

namespace EngineEventGenerator.Transmitters
{
    public class ConsoleTransmitter : ITelemetryTransmitter
    {
        // The following fields will not be sent with telemetry as they should not take part in it
        private static IList _fieldsToSkip = new[] {"ttf", "cycle", "engine_id"};
        
        public async Task Transmit(string[] header, List<EngineCycle> cycleList, CancellationToken cancellationToken)
        {
            foreach (var item in cycleList)
            {
                var sensorMessage = new JObject();
                for (var i = 0; i < item.SensorData.Length; i++)
                {

                    if (!_fieldsToSkip.Contains(header[i]))
                    {
                        sensorMessage[header[i]] = Convert.ToDecimal(item.SensorData[i]);
                    }
                }

                Console.WriteLine(item.EngineId);
                Console.WriteLine(sensorMessage.ToString());
                Console.WriteLine();
            }
        }
    }
}