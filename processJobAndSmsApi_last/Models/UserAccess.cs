using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace processJobAndSmsApi.Models
{
    public class UserAccess
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("user_id")]
        public string UserId { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("text_sms")]
        public string TextSms { get; set; }

        [Required]
        [MaxLength(10)]
        [Column("flash_sms")]
        public string FlashSms { get; set; }

        [Required]
        [MaxLength(10)]
        [Column("smart_sms")]
        public string SmartSms { get; set; }

        [Required]
        [MaxLength(10)]
        [Column("unicode_sms")]
        public string UnicodeSms { get; set; }

        [Required]
        [MaxLength(10)]
        [Column("campaign")]
        public string Campaign { get; set; }

        [Required]
        [MaxLength(10)]
        [Column("compose")]
        public string Compose { get; set; }

        [Required]
        [MaxLength(10)]
        [Column("dynamic_sms")]
        public string DynamicSms { get; set; }

        [Required]
        [MaxLength(10)]
        [Column("allow_spam")]
        public string AllowSpam { get; set; }

        [Required]
        [MaxLength(10)]
        [Column("restricted_report")]
        public string RestrictedReport { get; set; }

        [Required]
        [MaxLength(10)]
        [Column("number_management")]
        public string NumberManagement { get; set; }

        [Required]
        [MaxLength(10)]
        [Column("allow_smpp")]
        public string AllowSmpp { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("gateway_family_id")]
        public string GatewayFamilyId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("region_id")]
        public string RegionId { get; set; }

        [Required]
        [MaxLength(10)]
        [Column("api_access")]
        public string ApiAccess { get; set; }

        [Required]
        [MaxLength(10)]
        [Column("report_block")]
        public string ReportBlock { get; set; }

        [Required]
        [MaxLength(10)]
        [Column("dnd_refund")]
        public string DndRefund { get; set; }

        [Required]
        [MaxLength(10)]
        [Column("on_refund")]
        public string OnRefund { get; set; }

        [Required]
        [MaxLength(10)]
        [Column("template")]
        public string Template { get; set; }

        [Required]
        [MaxLength(10)]
        [Column("restricted_template")]
        public string RestrictedTemplate { get; set; }

        [Required]
        [MaxLength(10)]
        [Column("dynamic_sender")]
        public string DynamicSender { get; set; }

        [Required]
        [MaxLength(10)]
        [Column("overselling")]
        public string Overselling { get; set; }

        [Required]
        [MaxLength(10)]
        [Column("apply_tree")]
        public string ApplyTree { get; set; }

        [MaxLength(10)]
        [Column("apply_tree_cutting")]
        public string? ApplyTreeCutting { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("sms_limit")]
        public string SmsLimit { get; set; }

        [Required]
        [MaxLength(3)]
        [Column("verified_sms")]
        public string VerifiedSms { get; set; } = "No";

        [Column("rcs_msg", TypeName = "enum('Yes','No')")]
        public string? RcsMsg { get; set; } = "No";
    }
}
