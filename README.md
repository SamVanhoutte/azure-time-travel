# Time traveling with Microsoft Azure

This Repository contains all source code for the time series example that processes telemetry information from 100 simulated engines.

## Architecture

The overal architecture consists of the following components:

- __Engine telemetry simulator__:  netcore simulator that can be used as IoT Edge module).  [(Details)](./docs/simulator.md)
- __IoT Hub__: cloud based telemetry ingestion, having every simulated engine represented as a device
- __Time Series Insights__: Timeseries database instance in Azure that ingests all telemetry and allows for time series exploration
- __Azure Machine Learning__: Workspace that performs training of predictive maintenance model and hosts it as a service
- __Azure Stream Analytics__: Streaming instance that can process and perform standing queries in the incoming telemetry stream and uses the above mentioned model
- __Event Grid__: The event driven service that will be used to publish predictive maintenance events to and seperate handling of events from the detection of events
- __Azure Logic Apps__: The workflow service that will handle the different predictive maintenance events

![Architecture](./docs/images/architecture.png "Solution design")

## Pre-requisites

- An Active Azure Subscription
- A resource group with the following resources: IoT Hub, Azure Machine Learning Workspace, Time Series Insights environment, etc.  (arm template will be provided later)
- For the actual Machine Learning training, following these steps (to do)

## 
