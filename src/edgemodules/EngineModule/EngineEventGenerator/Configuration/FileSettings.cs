namespace EngineEventGenerator.Configuration
{
    public class FileSettings
    {
        public string Filename { get; set; } =
            "https://raw.githubusercontent.com/SamVanhoutte/azure-time-travel/main/resources/enginedata.csv";
        public int TransmitIntervalInMilliseconds { get; set; } = 10000;
    }
}