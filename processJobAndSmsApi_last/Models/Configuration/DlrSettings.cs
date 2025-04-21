namespace processJobAndSmsApi.Models.Configuration
{
    public class DlrSettings
    {
        public int BatchSize { get; set; } = 5000;
        public int RetryCount { get; set; } = 3;
        public int RetryDelayMs { get; set; } = 200;
        public string LogFilePath { get; set; } = "/mnt/data/kannel-logs/access_server.log";
        public int MaxDegreeOfParallelism { get; set; } = Environment.ProcessorCount; // Default to CPU cores
    }
}