# Engine Simulator

The engine simulator can be run in 3 different runtimes:

- As a dotnetcore Console App, by running the `src/edgemodules/EngineModule/EngineSimulatorModule` project and providing the correct configuration options in the `appsettings.json` file.
- As a docker container, by executing the following commands (and passing the configuration options as Environment Variables):
    ```
    docker run --rm -it -e iothub__IoTHubOwnerConnectionString="xxx" savanh/engine-module:latest
    ```
    - Replace the xxx with your connection string and put the connection string between quotes
- As an IoT Edge Module, by using the docker image `savanh/engine-module:latest` and providing the right configuration options as Environment Variables, as shown above.

## Configuration

### Configuration settings

The following configuration settings are possible for the simulator (in the following sections you can see how to set them through config file or environment variables):

- __IoTHubOwnerConnectionString__ (required): a connection string to the IoT Hub that has the rights to create new devices (every simulated device will be automatically created)
- __Filename__ (defaults to this repo's url): a file name, or an http(s) url to the csv file containing the measurements that will be read by the simulator. 
- __TransmitIntervalInMilliseconds__ (defaults to 10000): the interval that will be left between each cycle of telemetry.


### Configuration through appsettings.json

Two configuration files are read at startup in the following sequence: 
- appsettings.json (required)
- appsettings.dev.json (optional), overriding the above.

The appsettings.dev.json file is excluded through `.gitignore`, which prevents sensitive information to be added in source control.

The configuration file looks like this:

```json
{
  "file": {
    "Filename": "https://raw.githubusercontent.com/SamVanhoutte/azure-time-travel/main/resources/enginedata.csv",
    "TransmitIntervalInMilliseconds": 10000
  },
  "iothub": {
    "IoTHubOwnerConnectionString": "HostName=#youriothub#.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=#youriotownerkey#"
  }
}
```

### Configuration through environment variables

In order to set the configuration through Environment variables, the following variables are possible:

- `iothub__IoTHubOwnerConnectionString`: HostName=#youriothub#.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=#youriotownerkey#
- `file__Filename`: https://raw.githubusercontent.com/SamVanhoutte/azure-time-travel/main/resources/enginedata.csv
- `file__TransmitIntervalInMilliseconds`: 10000

