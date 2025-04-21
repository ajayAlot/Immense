using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace processJobAndSmsApi.Constants
{
    public class UserStatus
    {
        public const string SENT = "Sent";
        public const string FAILED = "Failed";
        public const string DELIVERED = "Delivered";
        public const string READ = "Read";
        public const string SUBMITTED = "Submitted";
        public const string UNDELIVERED = "Undelivered";
        public const string REJECTED = "Rejected";
        public const string EXPIRED = "Expired";
        public const string DND = "DND Number";
        public const string BLOCKED = "Blocked";
        public const string NUMBER_BLOCKED = "Number Blocked";
        public const string SPOOFED = "Spoofed";
        public const string NULLDATA = "Null";
        public const string YES = "Yes";
        public const string NO = "No";
 
        // public static IEnumerable<string> GetAllStatuses()
        // {
        //     return
        //     [
        //         SENT,
        //         FAILED,
        //         DELIVERED,
        //         READ,
        //         SUBMITTED,
        //         UNDELIVERED,
        //         REJECTED,
        //         EXPIRED,
        //         DND,
        //         BLOCKED,
        //         NUMBER_BLOCKED,
        //         SPOOFED
        //     ];
        // }
    }

}
