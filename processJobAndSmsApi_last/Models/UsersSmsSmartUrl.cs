using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace processJobAndSmsApi.Models
{
    [Table("users_sms_smart_urls")]
    public class UsersSmsSmartUrl
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public string UserId { get; set; }

        [Column("job_id")]
        public string JobId { get; set; }

        [Column("mobile_number")]
        public string MobileNumber { get; set; }

        [Column("filename")]
        public string Filename { get; set; }

        [Column("long_url")]
        public string LongUrl { get; set; }

        [Column("short_code")]
        public string ShortCode { get; set; }

        [Column("short_url")]
        public string ShortUrl { get; set; }

        [Column("add_to_group")]
        public string AddToGroup { get; set; }

        [Column("group_id")]
        public string GroupId { get; set; }

        [Column("send_notification")]
        public string SendNotification { get; set; }

        [Column("sms_message")]
        public string SmsMessage { get; set; }

        [Column("hits")]
        public int Hits { get; set; }

        [Column("pdate")]
        public string Pdate { get; set; }

        [Column("jdate")]
        public string Jdate { get; set; }

        [Column("ptime")]
        public string Ptime { get; set; }

        [Column("ipaddress")]
        public string Ipaddress { get; set; }
    }
}