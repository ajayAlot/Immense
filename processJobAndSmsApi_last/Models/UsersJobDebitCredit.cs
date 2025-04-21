

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace processJobAndSmsApi.Models
{
    public class UsersJobDebitCredit
    {
        public int UserId { get; set; }
        public string? JobId { get; set; }
        public int RouteTypeId { get; set; }
        public string? TransactionType { get; set; }
        public string? Type { get; set; }
        public int SmsQty { get; set; }
        public decimal PrevSmsQty { get; set; }
        public decimal SmsBalanceQty { get; set; }
        public string? Remark { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? PDate { get; set; }
        public string? JDate { get; set; }
        public long PTime { get; set; } // Use long to store Unix timestamp
    }

}
