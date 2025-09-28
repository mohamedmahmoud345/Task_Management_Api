using TaskManagement.Api.DTO;
using TaskManagement.Api.Model;

namespace TaskManagement.Api.Repositories.IRepositories
{
    public interface ITaskRepository
    {
        public Task<List<TaskData>> GetAsync(string userId);
        public Task<TaskData> GetByIdAsync(int id , string userId);
        public Task<TaskData> AddAsync(TaskDto data , string userId);
        public Task<bool> RemoveAsync(int id, string userId);
        public Task<bool> EditAsync(TaskDto data, string userId);
        public Task<List<TaskData>> FilterByStatus(int statusNum , string userId);
        public Task<List<TaskData>> FilterByPriority(int priorityNum , string userId);
        public Task<List<TaskData>> SearchByTitle(string title, string userId);
        public Task SaveAsync();
    }
}
