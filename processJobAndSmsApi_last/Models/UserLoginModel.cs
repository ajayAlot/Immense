using System.ComponentModel.DataAnnotations;

namespace processJobAndSmsApi.Models
{
    public class UserLoginModel{
        [Required]
        public string username {get;set;}

        [Required]
        [DataType(DataType.Password)]
        public string password {get;set;}

        [Required]
        public bool rememberMe {get;set;}
    }

}