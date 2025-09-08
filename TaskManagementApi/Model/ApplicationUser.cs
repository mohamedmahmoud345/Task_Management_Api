using Microsoft.AspNetCore.Identity;

namespace TaskManagementApi.Model
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<TaskData> Tasks { get; set; } = new List<TaskData>();
    }
}
 