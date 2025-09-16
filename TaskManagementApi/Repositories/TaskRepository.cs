using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Context;
using TaskManagementApi.DTO;
using TaskManagementApi.Extensions;
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
            return await context.Tasks.AsNoTracking().Where(t => t.UserId == userId).OrderBy(t => t.DueDate).ToListAsync();
        }

        public async Task<TaskData> GetByIdAsync(int id, string userId)
        {
            var task = await context.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (task == null)
                throw new ArgumentException(nameof(id));
            return task;
        }
        public async Task<TaskData> AddAsync(TaskDto data, string userId)
        {
            if(data == null) 
                throw new ArgumentException(nameof(data));
            if(string.IsNullOrEmpty(userId))
                throw new ArgumentException(nameof(userId));

            var task = data.ToModel(userId );
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
        public async Task<List<TaskData>> FilterByStatus(int statusNumber , string userId)
        {
            return 
                await context.Tasks.AsNoTracking().Where(t => (int)t.Status == statusNumber && t.UserId == userId).ToListAsync();
        }
        public async Task<List<TaskData>> FilterByPriority(int priorityNumber , string userId)
        {
            return 
                await context.Tasks.AsNoTracking().Where(t => (int)t.Priority == priorityNumber &&  t.UserId == userId).ToListAsync();
        }

        public async Task<List<TaskData>> SearchByTitle(string title , string userId)
        {
            return
                await context.Tasks.AsNoTracking().Where(task => task.Title.ToLower().Contains(title)&& task.UserId == userId).OrderBy(x => x.DueDate).ToListAsync();
        }

        public async Task SaveAsync()
        {
           await context.SaveChangesAsync();
        }
    }
}
