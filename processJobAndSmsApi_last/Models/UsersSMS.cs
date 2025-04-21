using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace processJobAndSmsApi.Models
{
    // public class UsersSMS
    // {
        // public int Id { get; set; } // This is an int
        // public string Number { get; set; } // This is a string
        // public string Status { get; set; } // This is a string
        // public string Smart { get; set; } // This is a string
        // public string Dnd { get; set; } // This is a string
        // public string Block { get; set; } // This is a string
        // public string Refund { get; set; } // This is a string
        // public string MsgId { get; set; } // This is a string
        // public int? Jdate { get; set; } // This is a string
        // public string SentAt { get; set; } // This is a string
        // public string? MsgStat { get; set; } // This is a string
        // public int SMPPSMSCID { get; set; }  // SMSC ID
        // public string SMPPSMSCIDName { get; set; }  // SMSC Name

        [Table("users_sms")]
public class UsersSMS
{
    [Column("id")]
    public int Id { get; set; }
    [Column("mobile_number")]
    public string Number { get; set; }
    [Column("status")]
    public string Status { get; set; }
    [Column("smart")]
    public string Smart { get; set; }
    [Column("dnd")]
    public string Dnd { get; set; }
    [Column("block")]
    public string Block { get; set; }
    [Column("refund")]
    public string Refund { get; set; }
    [Column("message_id")]
    public string MsgId { get; set; }
    [Column("jdate")]
    public int? Jdate { get; set; }
    [Column("sent_at")]
    public string SentAt { get; set; }
    [Column("smsc_id")]
    public int SMPPSMSCID { get; set; }
    [Column("smsc_name")]
    public string SMPPSMSCIDName { get; set; }

    [Column("job_id")]
    public string job_id { get; set; }
    [Column("user_id")]
    public string user_id { get; set; }
    [Column("schedule_sms")]
    public string schedule_sms { get; set; }
    [Column("schedule_date")]
    public string schedule_date { get; set; }
    [Column("schedule_time")]
    public string schedule_time { get; set; }
    [Column("sms_message")]
    public string sms_message { get; set; }
}

    // }

}
