using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EngineEventGenerator.Configuration;
using EngineEventGenerator.Interfaces;
using EngineEventGenerator.Models;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using IotHubConnectionStringBuilder = Microsoft.Azure.Devices.IotHubConnectionStringBuilder;
using Message = Microsoft.Azure.Devices.Client.Message;

namespace EngineEventGenerator.Transmitters
{
    public class IoTHubTransmitter : ITelemetryTransmitter
    {
        // The following fields will not be sent with telemetry as they should not take part in it
        private static IList _fieldsToSkip = new[] {"ttf", "cycle", "engine_id"};
        
        private IoTHubSettings _iotHubSettings;
        private RegistryManager _iotHubManager;
        private IDictionary<string, DeviceClient> _deviceClients;

        private IDictionary<string, DeviceClient> DeviceClients =>
            _deviceClients ??= new Dictionary<string, DeviceClient>();

        private RegistryManager IoTHubManager => _iotHubManager ??=
            RegistryManager.CreateFromConnectionString(_iotHubSettings.IoTHubOwnerConnectionString);

        public IoTHubTransmitter(IOptions<IoTHubSettings> iotHubSettings)
        {
            _iotHubSettings = iotHubSettings.Value ?? throw new ArgumentNullException(nameof(iotHubSettings));
        }

        public async Task Transmit(string[] header, List<EngineCycle> cycleList, CancellationToken cancellationToken)
        {
            foreach (var item in cycleList)
            {
                var client = await GetDeviceClient(item.EngineId, CancellationToken.None);
                var sensorMessage = new JObject();
                for (var i = 0; i < item.SensorData.Length; i++)
                {
                    if (!_fieldsToSkip.Contains(header[i]))
                    {
                        sensorMessage[header[i]] = Convert.ToDecimal(item.SensorData[i]);
                    }
                }

                var msg = new Message(Encoding.UTF8.GetBytes(sensorMessage.ToString()));
                await client.SendEventAsync(msg, cancellationToken);
                Console.WriteLine(sensorMessage.ToString());
            }
        }


        private async Task<DeviceClient> GetDeviceClient(string deviceId, CancellationToken cancellationToken)
        {
            if (!DeviceClients.ContainsKey(deviceId))
            {
                // Check if device exists
                var device = await IoTHubManager.GetDeviceAsync(deviceId, cancellationToken);
                if (device == null)
                {
                    Console.WriteLine("Creating device " + deviceId);
                    await IoTHubManager.AddDeviceAsync(new Device(deviceId), cancellationToken);
                    device = await IoTHubManager.GetDeviceAsync(deviceId, cancellationToken);
                    Console.WriteLine("Device " + deviceId + " created");
                }

                var deviceConnectionString =
                    $"HostName={IotHubConnectionStringBuilder.Create(_iotHubSettings.IoTHubOwnerConnectionString).HostName};DeviceId={deviceId};SharedAccessKey={device.Authentication.SymmetricKey.PrimaryKey}";
                DeviceClients.Add(deviceId, DeviceClient.CreateFromConnectionString(deviceConnectionString));
            }

            return DeviceClients[deviceId];
        }
    }
}