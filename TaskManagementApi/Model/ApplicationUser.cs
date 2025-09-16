using Microsoft.AspNetCore.Identity;

namespace TaskManagementApi.Model
{
    public class ApplicationUser : IdentityUser
    {
        public string? ProfilePicturePath { get; set; }
        public ICollection<TaskData> Tasks { get; set; } = new List<TaskData>();

    }
}
 