using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Api.DTO
{
    public class NameDto
    {
        [Required(ErrorMessage ="Name Is Required"), Length(5 , 100 , ErrorMessage = "Name Must Be Between 5 to 100")]
        public string Name { get; set; }
    }
}
