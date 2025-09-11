using Microsoft.EntityFrameworkCore;
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

        public async Task EditAsync(TaskDto data)
        {
            var task = await context.Tasks.FindAsync(data.Id);
            if (task == null) return;

            task.Title = data.Title;
            task.Description = data.Description;
            task.DueDate = data.DueDate;
            task.Priority = data.Priority;
            task.Status = data.Status;

        }

        public async Task<List<TaskData>> GetAsync()
        {
            return await context.Tasks.ToListAsync();
        }

        public async Task<TaskData> GetByIdAsync(int id)
        {
            return await context.Tasks.FindAsync(id);
        }
        public async Task AddAsync(TaskDto data)
        {
            if(data == null) return;
            var task = new TaskData
            {
                Title = data.Title,
                Description = data.Description,
                DueDate = data.DueDate,
                Priority = data.Priority,
                Status = data.Status,
            };
            await context.Tasks.AddAsync(task);
        }
        public async Task RemoveAsync(int id)
        {
            var task = await context.Tasks.FindAsync(id);
            if (task == null) return;
            context.Tasks.Remove(task);
        }

        public async Task SaveAsync()
        {
           await context.SaveChangesAsync();
        }
    }
}
