using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using TaskManagement.Api.Enums;

namespace TaskManagement.Api.Model
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
        public string UserId { get; set; } 
        public ApplicationUser User { get; set; }
    }
}
