using TaskManagement.Api.DTO;
using TaskManagement.Api.Model;

namespace TaskManagement.Api.Extensions
{
    public static class TaskExtensions
    {
        public static TaskDto ToDto(this TaskData taskData)
        {
            return new TaskDto
            {
                Id = taskData.Id,
                Title = taskData.Title,
                Description = taskData.Description,
                DueDate = taskData.DueDate,
                Priority = taskData.Priority,
                Status = taskData.Status,
            };
        }

        public static List<TaskDto> ToDto(this List<TaskData> tasks)
        {
            return tasks.Select(t => t.ToDto()).ToList() ?? [];
        }

        public static TaskData ToModel(this TaskDto dto , string userId)
        {
            var task = new TaskData
            {
                Id = dto.Id,
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
                Priority = dto.Priority,
                Status = dto.Status,
                UserId = userId
            };

            return task;
        }
    }
}
