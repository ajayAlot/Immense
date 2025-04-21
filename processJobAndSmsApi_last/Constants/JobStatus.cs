
namespace processJobAndSmsApi.Constants
{
    public class JobStatus
    {
        public const string COMPLETED = "Completed";
        public const string FAILED = "Failed";
        public const string SCHEDULED = "Scheduled";
        public const string CANCELLED = "Cancelled";
        public const string PROCESSING = "On Going";
        public const string SPAM = "On Hold";
 
        // public static IEnumerable<string> GetAllJobStatuses()
        // {
        //     return
        //     [
        //         COMPLETED,
        //         FAILED,
        //         SCHEDULED,
        //         CANCELLED,
        //         PROCESSING,
        //         SPAM
        //     ];
        // }
    }

}