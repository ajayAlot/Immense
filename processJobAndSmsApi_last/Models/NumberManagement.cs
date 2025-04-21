using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace processJobAndSmsApi.Models
{
    [Table("number_management")] // Move the Table attribute here
    public class NumberManagement
    {
        [Column("id")] 
        public int Id { get; set; }

        [Column("user_id")] 
        public string? UserId { get; set; }

        // [Column("parent_user_id")] 
        // public string? ParentUserId { get; set; }

        [Column("number")] 
        public string? Number { get; set; }

        // [Column("split1")] 
        // public string? Split1 { get; set; }

        // [Column("split2")] 
        // public string? Split2 { get; set; }

        // [Column("split3")] 
        // public string? Split3 { get; set; }

        // [Column("split4")] 
        // public string? Split4 { get; set; }

        // [Column("reason")] 
        // public string? Reason { get; set; }

        [Column("status")] 
        public string? Status { get; set; }

        // [Column("created_by_id")] 
        // public string? CreatedById { get; set; }

        // [Column("modified_by_id")] 
        // public string? ModifiedById { get; set; }

        // [Column("created_date")] 
        // public string? CreatedDate { get; set; }

        // [Column("modified_date")] 
        // public string? ModifiedDate { get; set; }

        // [Column("pdate")] 
        // public string? Pdate { get; set; }

        // [Column("jdate")] 
        // public string? Jdate { get; set; }

        // [Column("ptime")] 
        // public string? Ptime { get; set; }

        // [Column("ipaddress")] 
        // public string? IpAddress { get; set; }
    }
}
