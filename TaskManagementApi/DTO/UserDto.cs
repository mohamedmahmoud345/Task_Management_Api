using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Api.DTO
{
    public class UserDto
    {
        [Required(ErrorMessage =  "User Name Is Required")]
        [StringLength(100 , MinimumLength = 6 , ErrorMessage = "Name Must Be Between 5 to 100")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Please Enter The Password")]
        public string Password { get; set; }
    }
}
