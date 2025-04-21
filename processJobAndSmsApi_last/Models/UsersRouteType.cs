using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace processJobAndSmsApi.Models
{
    [Table("users_route_type")]
    public class UsersRouteType
    {
        [Key]
        [Column("id")] 
        public int Id { get; set; } 
        [Column("sms_balance")]
        public int SmsBalance { get; set; }  
        [Column("user_id")]
        public string? UserId { get; set; }
        [Column("route_type_id")]
        public string? RouteTypeId { get; set; }
    }
}
