using Microsoft.AspNetCore.Identity;
using TaskManagement.Api.Context;
using TaskManagement.Api.Model;
using TaskManagement.Api.Repositories.IRepositories;

namespace TaskManagement.Api.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext context;
        public UserRepository(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<ApplicationUser> GetUserById(string id)
        {
            var user = await context.Users.FindAsync(id);
            if (user == null) throw new ArgumentException(nameof(id));

            return user;
        }

        public async Task UploadPhotoAsync(string path, string userId)
        {
            var user = await GetUserById(userId);
            if (user == null) throw new ArgumentException(nameof(userId));
            user.ProfilePicturePath = path;
            await context.SaveChangesAsync();
        }


        public async Task<(bool HasPhoto, string PhotoPath)> IsUserHasProfilePicture(string userId)
        {
            var user = await GetUserById(userId);
            if (user == null) throw new ArgumentException(nameof(userId));

            var HasPhoto = user.ProfilePicturePath != null;
            var PhotoPath = user.ProfilePicturePath;

            return  (HasPhoto , PhotoPath!);
        }

        public async Task Save()
        {
            await context.SaveChangesAsync();
        }

    }
}
