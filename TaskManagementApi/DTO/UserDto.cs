using System.ComponentModel.DataAnnotations;

namespace TaskManagementApi.DTO
{
    public class UserDto
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
