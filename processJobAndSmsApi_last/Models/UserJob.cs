using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace processJobAndSmsApi.Models
{
    [Table("users_job")]
    public class UserJob
    {
        [Key]
        public int Id { get; set; } 
        [Column("job_id")]
        public string? JobId { get; set; }
        [Column("job_status")]
        public string? JobStatus { get; set; }
        public string? UserId { get; internal set; }
        public int JobSmsSent { get; internal set; }
        public string? Remarks { get; internal set; }
        public int SMSCount { get; internal set; }
        [Column("filepath")]
        public string? FilePath { get; internal set; }
        public int[]? UsersGroupIds { get; internal set; }
    }
}
