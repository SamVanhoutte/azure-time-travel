using System;

namespace MaintenanceEventRouter.EventData
{
    public class MaintenanceData
    {
        public string Engine { get; set; }
        public double Probability { get; set; }
        public DateTime DetectionTime { get; set; }
    }
}