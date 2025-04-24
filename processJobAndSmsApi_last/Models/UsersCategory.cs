using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace processJobAndSmsApi.Models
{
    [Table("users_category")]
    public class UsersCategory
    {
        [Key]
        [Column("id")]
        public int? Id { get; set; }
        [Column("type")]
        public string? Type { get; set; }
        [Column("created_by_id")]
        public string? CreatedById { get; set; }
        [Column("modified_by_id")]
        public string? ModifiedById {  get; set; }
        [Column("created_date")]
        public string? CreatedDate { get; set; }
        [Column("modified_date")]
        public string? ModifiedDate { get; set; }
        [Column("pdate")]
        public string? Pdate { get; set; }
        [Column("jdate")]
        public string? Jdate { get; set; }
        [Column("ptime")]
        public string? Ptime { get; set; }
        [Column("ipaddress")]
        public string? IpAdress { get; set; }
    }
}
