using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace processJobAndSmsApi.Models
{
    public class Users
    {
        public string? Id { get; set; }
        public string? CategoryId { get; set; }
        public string? ParentId { get; set; }
        public string? ManagerId { get; set; }
        public string? IndustryId { get; set; }
        public string? TimezoneId { get; set; }
        public string? CreatedById { get; set; }
        public string? Email { get; set; }
        public string? MdEmail { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Fullname { get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? State { get; set; }
        public string? City { get; set; }
        public string? Zipcode { get; set; }
        public string? Mobile { get; set; }
        public string? Status { get; set; }
        public string? Approve { get; set; }
        public string? Pdate { get; set; }
        public string? Jdate { get; set; }
        public string? TotalBalance { get; set; }
        public string? VerificationCode { get; set; }
        public string? Verified { get; set; }
        public string? RegistrationDate { get; set; }
        public string? RegistrationJdate { get; set; }
        public string? LastLoginDate { get; set; }
        public string? LastLoginJdate { get; set; }
        public string? TotalLogin { get; set; }
        public string? LastAccountUpdate { get; set; }
        public string? LastPasswordUpdate { get; set; }
        public string? ApiKey { get; set; }
        public string? BillType { get; set; }
        public string? IsOnline { get; set; }
        public string? Timestamp { get; set; }
    }
    // public class ResponseResult
    // {
    //     public string? Response { get; set; }
    //     public string? Url { get; set; }
    // }
}