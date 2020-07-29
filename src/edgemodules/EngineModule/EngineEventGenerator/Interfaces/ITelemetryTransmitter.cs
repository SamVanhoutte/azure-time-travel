using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EngineEventGenerator.Models;

namespace EngineEventGenerator.Interfaces
{
    public interface ITelemetryTransmitter
    {
        Task Transmit(string[] header, List<EngineCycle> cycleList, CancellationToken cancellationToken);
    }
}