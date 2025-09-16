using TaskManagementApi.Model;

namespace TaskManagementApi.Repositories.IRepositories
{
    public interface IUserRepository
    {
        public Task<ApplicationUser> GetUserById(string id);
        public Task UploadPhotoAsync(string path, string userId);
        public Task<(bool HasPhoto, string PhotoPath)> IsUserHasProfilePicture(string userId);
        public Task Save();
    }
}
