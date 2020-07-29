using System.Threading;
using System.Threading.Tasks;

namespace EngineEventGenerator.Interfaces
{
    public interface ITelemetryReceiver
    {
        Task Initialize();
        Task Run(CancellationToken cancellationToken);
    }
}