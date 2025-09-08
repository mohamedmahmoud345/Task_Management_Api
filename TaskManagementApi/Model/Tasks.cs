using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskManagementApi.Enums;

namespace TaskManagementApi.Model
{
    public class TaskData
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? DueDate { get; set; }
        public PriorityEnum Priority { get; set; }
        public StatusEnum Status { get; set; }

        [ForeignKey("user")]
        public string? UserId { get; set; } // must not allow null i will edit it after create account controller
        public ApplicationUser User { get; set; }
    }
}
