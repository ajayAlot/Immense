using System.ComponentModel.DataAnnotations;
namespace processJobAndSmsApi.Models
{
    public class UserRegister
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string? Username { get; set; }
        [Required]
        public string? Fullname { get; set; }
        [Required]
        public string? Companyname { get; set; }
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        [Required]
        public string? Mobile { get; set; }
        [Required]
        public string? PostCode { get; set; }
        [Required(ErrorMessage = "You must accept the terms and conditions.")]
        public bool AcceptTerms { get; set; }
    }
}
