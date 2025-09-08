using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Context;
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

        public async Task Edit(TaskData data)
        {
            var task = await context.Tasks.FindAsync(data.Id);
            if (task == null) return;

            task.Title = data.Title;
            task.Description = data.Description;
            task.DueDate = data.DueDate;
            task.Priority = data.Priority;
            task.Status = data.Status;
            task.UserId = data.UserId;
            await Save();
        }

        public async Task<List<TaskData>> Get()
        {
            return await context.Tasks.ToListAsync();
        }

        public async Task<TaskData> GetById(int id)
        {
            return await context.Tasks.FindAsync(id);
        }

        public async Task Remove(TaskData data)
        {
            var task = await context.Tasks.FindAsync(data.Id);
            if (task == null) return;
            context.Tasks.Remove(task);
            await Save();
        }

        public async Task Save()
        {
           await context.SaveChangesAsync();
        }
    }
}
