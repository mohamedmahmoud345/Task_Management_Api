using System.ComponentModel.DataAnnotations;
using TaskManagementApi.Enums;

namespace TaskManagementApi.DTO
{
    public class TaskDto
    {
        public int Id { get; set; }
        [Required , MaxLength(200)]
        public string Title { get; set; }
        [MaxLength(1000)]
        public string Description { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }
        public PriorityEnum Priority { get; set; }
        public StatusEnum Status { get; set; }
    }
}
