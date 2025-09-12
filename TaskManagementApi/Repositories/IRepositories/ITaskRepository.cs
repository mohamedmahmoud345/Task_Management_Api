using TaskManagementApi.DTO;
using TaskManagementApi.Model;

namespace TaskManagementApi.Repositories.IRepositories
{
    public interface ITaskRepository
    {
        public Task<List<TaskData>> GetAsync(string userId);
        public Task<TaskData> GetByIdAsync(int id , string UserId);
        public Task<TaskData> AddAsync(TaskDto data , string UserId);
        public Task<bool> RemoveAsync(int id, string UserId);
        public Task<bool> EditAsync(TaskDto data, string UserId);
        public Task SaveAsync();
    }
}
