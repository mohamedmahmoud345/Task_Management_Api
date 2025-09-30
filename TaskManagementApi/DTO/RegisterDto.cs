using Azure;
using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Api.DTO
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Name Is Required")]
        [StringLength(100 , MinimumLength = 5 , ErrorMessage = "Name Must Be Between 5 to 100 Charcters")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email Is Required")]
        [EmailAddress(ErrorMessage = "Invalid Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password Is Required")]
        public string Password { get; set; }
    }
}
