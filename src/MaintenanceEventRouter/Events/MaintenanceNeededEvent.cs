using MaintenanceEventRouter.EventData;
using Arcus.EventGrid.Contracts;

namespace MaintenanceEventRouter.Events
{
    public class MaintenanceNeededEvent : EventGridEvent<MaintenanceData>
    {
        public MaintenanceNeededEvent()
        {
        }

        public MaintenanceNeededEvent(string id, string subject, MaintenanceData eventData) :
            base(id, subject, eventData, DataVersion, EventType)
        {
        }

        private const string DataVersion = "1";
        private const string EventType = "TimeTravel.MaintenanceNeeded";
    }
}