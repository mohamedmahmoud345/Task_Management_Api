using System.ComponentModel.DataAnnotations;
using TaskManagement.Api.Enums;

namespace TaskManagement.Api.DTO
{
    public class TaskDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Title Is Required")]
        [StringLength(200 , MinimumLength = 5 , ErrorMessage = "Title Must Be Between 5 to 200")]
        public string Title { get; set; }

        [StringLength(300)]
        public string Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        public PriorityEnum Priority { get; set; }

        public StatusEnum Status { get; set; }
    }
}
