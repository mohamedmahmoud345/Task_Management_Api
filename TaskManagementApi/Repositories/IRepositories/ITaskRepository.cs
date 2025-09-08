using TaskManagementApi.Model;

namespace TaskManagementApi.Repositories.IRepositories
{
    public interface ITaskRepository
    {
        public Task<List<TaskData>> Get();
        public Task<TaskData> GetById(int id);
        public Task Remove(TaskData data);
        public Task Edit(TaskData data);
        public Task Save();
    }
}
