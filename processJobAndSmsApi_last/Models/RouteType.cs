using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace processJobAndSmsApi.Models
{
    public class RouteType
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("country_id")]
        public int CountryId { get; set; }

        [Column("start_time")]
        public string StartTime { get; set; }

        [Column("end_time")]
        public string EndTime { get; set; }

        [Column("balancer")]
        public int Balancer { get; set; }

        [Column("sender_id_status")]
        public string SenderIdStatus { get; set; }

        [Column("dnd_check")]
        public string DndCheck { get; set; }

        [Column("created_by_id")]
        public int CreatedById { get; set; }

        [Column("modified_by_id")]
        public int ModifiedById { get; set; }

        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [Column("modified_date")]
        public DateTime? ModifiedDate { get; set; }

        [Column("pdate")]
        public string PDate { get; set; }

        [Column("jdate")]
        public int JDate { get; set; }

        [Column("ptime")]
        public int PTime { get; set; }

        [Column("ipaddress")]
        public string IPAddress { get; set; }

        [Column("status")]
        public string Status { get; set; }
    }
}