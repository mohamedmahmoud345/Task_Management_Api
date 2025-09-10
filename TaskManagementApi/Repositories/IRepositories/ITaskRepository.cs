using TaskManagementApi.DTO;
using TaskManagementApi.Model;

namespace TaskManagementApi.Repositories.IRepositories
{
    public interface ITaskRepository
    {
        public Task<List<TaskData>> GetAsync();
        public Task<TaskData> GetByIdAsync(int id);
        public Task AddAsync(TaskDto data);
        public Task RemoveAsync(int id);
        public Task EditAsync(TaskDto data);
        public Task SaveAsync();
    }
}
