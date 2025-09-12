using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManagementApi.Context;
using TaskManagementApi.DTO;
using TaskManagementApi.Model;
using TaskManagementApi.Repositories.IRepositories;

namespace TaskManagementApi.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext context;
        public TaskRepository(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<bool> EditAsync(TaskDto data, string userId)
        {
            if(data == null) throw new ArgumentException(nameof(data));
            if(string.IsNullOrEmpty(userId)) throw new ArgumentException(nameof(userId));
            
            var task = await GetByIdAsync(data.Id , userId);
            if (task == null) return false;

            task.Title = data.Title;
            task.Description = data.Description;
            task.DueDate = data.DueDate;
            task.Priority = data.Priority;
            task.Status = data.Status;

            return true;
        }

        public async Task<List<TaskData>> GetAsync(string userId)
        {
            return await context.Tasks.Where(t => t.UserId == userId).OrderBy(t => t.DueDate).ToListAsync();
        }

        public async Task<TaskData> GetByIdAsync(int id, string userId)
        {
            return await context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        }
        public async Task<TaskData> AddAsync(TaskDto data, string userId)
        {
            if(data == null) 
                throw new ArgumentException(nameof(data));
            if(string.IsNullOrEmpty(userId))
                throw new ArgumentException(nameof(userId));

            var task = new TaskData
            {
                Title = data.Title,
                Description = data.Description,
                DueDate = data.DueDate,
                Priority = data.Priority,
                Status = data.Status,
                UserId = userId
            };
            await context.Tasks.AddAsync(task);

            return task;
        }
        public async Task<bool> RemoveAsync(int id, string UserId)
        {
            if (string.IsNullOrEmpty(UserId))
                throw new ArgumentException(nameof(UserId));

            var task = await context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == UserId);

            if (task == null)
                return false;

            context.Tasks.Remove(task);
            return true;
        }

        public async Task SaveAsync()
        {
           await context.SaveChangesAsync();
        }
    }
}
