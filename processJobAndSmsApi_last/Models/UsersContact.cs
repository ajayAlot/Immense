// Models/UsersContact.cs
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace  processJobAndSmsApi.Models
{   
    [Table("users_contact")]
    public class UsersContact
    {
        [Key]
        public int Id { get; set; } 
        [Column("mobile")]
        public string? Mobile { get; set; } 
        [Column("group_id")]
        public int GroupId { get; set; }  
        public string? Status { get; set; }  
        public int[]? UsersGroupIds { get; set; }  

        // public string? UserId { get; set; } 
        // public string? Name { get; set; }
    }


}