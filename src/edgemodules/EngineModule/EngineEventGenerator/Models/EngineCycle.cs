namespace EngineEventGenerator.Models
{
    public class EngineCycle
    {
        public int Cycle { get; set; }
        public string Data { get; set; }
        public string EngineId { get; set; }
        public string[] SensorData { get; set; }
    }
}