using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace processJobAndSmsApi.Models
{
    [Table("users_sms_smart_urls_visits")]
    public class SmartLinkHit
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public string JobId { get; set; }
        public string MobileNumber { get; set; }
        public string UserAgent { get; set; }
        public string Device { get; set; }
        public string Browser { get; set; }
        public string Version { get; set; }
        public string LongUrl { get; set; }
        public string ShortCode { get; set; }
        public string ShortUrl { get; set; }
        public string CreatedDate { get; set; }
        public string Pdate { get; set; }
        public int Jdate { get; set; }
        public long Ptime { get; set; }
        public string IpAddress { get; set; }
    }
}