using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using TaskManagement.Api.Controllers;
using TaskManagement.Api.DTO;
using TaskManagement.Api.Model;
using TaskManagement.Api.Repositories.IRepositories;
using TaskManagement.Tests.HelperMethodes;

namespace TaskManagement.Tests.Controllers
{
    public class UserControllerTest
    {


        private readonly Mock<IWebHostEnvironment> environment;
        private readonly Mock<ILogger<UserController>> logger;
        private readonly Mock<IUserRepository> repo;
        private readonly Mock<UserManager<ApplicationUser>> userManager;
        private readonly UserController controller;

        private string path = @"c:\pc\photos";
        private string userId = "1";

        public UserControllerTest()
        {
            environment = new Mock<IWebHostEnvironment>();
            logger = new Mock<ILogger<UserController>>();
            repo = new Mock<IUserRepository>();

            var userStore = new Mock<IUserStore<ApplicationUser>>();
            userManager = new Mock<UserManager<ApplicationUser>>(userStore.Object, null, null, null, null, null, null, null, null);

            controller = new UserController(logger.Object, environment.Object, repo.Object, userManager.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier , "1")
            }));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }
        #region start upload photo endpoint
        [Fact]
        public async Task UploadPhoto_WithInvalidUserId_ReturnsNotFound()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() { User = user } };
            var mockfile = Helpers.CreateMockFile();

            repo.Setup(x => x.UploadPhotoAsync(path, userId));

            var result = await controller.UploadPhoto(mockfile.Object);

            var notFound = Assert.IsType<NotFoundResult>(result);
            Assert.NotNull(notFound);
        }

        [Fact]
        public async Task UploadPhoto_WithNullFile_ReturnsBadRequest()
        {
            repo.Setup(x => x.UploadPhotoAsync(path, userId));

            var result = await controller.UploadPhoto(null);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest);
        }

        [Fact]
        public async Task UploadPhoto_WithFileZeroLength_ReturnsBadRequest()
        {
            var mockFile = Helpers.CreateMockFile(fileLength: 0);

            repo.Setup(x => x.UploadPhotoAsync(path, userId));

            var result = await controller.UploadPhoto(mockFile.Object);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest);
        }

        [Fact]
        public async Task UplaodPhoto_WithLargeFileLength_ReturnsBadRequest()
        {
            var mockfile = Helpers.CreateMockFile(fileLength: 10_000_001);

            repo.Setup(x => x.UploadPhotoAsync(path, userId));

            var result = await controller.UploadPhoto(mockfile.Object);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest);
        }
        [Fact]
        public async Task UploadPhoto_WithInValidFileExtention_ReturnsBadRequest()
        {
            var mockFile = Helpers.CreateMockFile(fileName: "file.sdf");

            repo.Setup(_ => _.UploadPhotoAsync(path, userId));

            var result = await controller.UploadPhoto(mockFile.Object);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest);
        }
        [Fact]
        public async Task UploadPhoto_WithValidFile_ReturnsOkWithFileInfoJson()
        {
            var mockFile = Helpers.CreateMockFile().Object;

            repo.Setup(_ => _.UploadPhotoAsync(path, userId));

            var result = await controller.UploadPhoto(mockFile);


            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult);
        }
        #endregion

        #region start get photo endpoint 
        [Fact]
        public async Task GetPhoto_WithInValiduserId_ReturnsNotFound()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() { User = user } };

            repo.Setup(x => x.GetUserById(userId)).ThrowsAsync(new Exception());

            var result = await controller.GetPhoto();

            var notFound = Assert.IsType<NotFoundResult>(result);
            Assert.NotNull(notFound);
        }
        [Fact]
        public async Task GetPhoto_WithNoPictureFoundForUser_ReturnsNotFound()
        {

            repo.Setup(_ => _.IsUserHasProfilePicture(userId)).ReturnsAsync((false, null));

            var result = await controller.GetPhoto();

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFound);
        }
        [Fact]
        public async Task GetPhoto_WithPictureFound_ReturnsOk()
        {
            repo.Setup(_ => _.IsUserHasProfilePicture(userId)).ReturnsAsync((true, "path"));

            var result = await controller.GetPhoto();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult);
        }
        #endregion
        #region start delete photo endpoint 
        [Fact]
        public async Task DeletePhoto_WithInValidUser_ReturnsNotFound()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() { User = user } };

            repo.Setup(x => x.IsUserHasProfilePicture(userId));

            var result = await controller.DeletePhoto();

            var notFound = Assert.IsType<NotFoundResult>(result);
            Assert.NotNull(notFound);
        }
        [Fact]
        public async Task DeletePhoto_WithUserHasNotProfilePicture_ReturnsBadRequest()
        {
            var user = new Mock<ApplicationUser>();

            repo.Setup(x => x.GetUserById(userId)).ReturnsAsync(user.Object);

            var result = await controller.DeletePhoto();

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest);
        }
        [Fact]
        public async Task DeletePhoto_WithUserHasProfilePicture_ReturnsOk()
        {
            var user = new ApplicationUser() { ProfilePicturePath = "path" };
            var profilePicturePath = "users/photo.jpg";
            var webRootPath = "wwwroot";
            var fullPath = Path.Combine(webRootPath, "UpLoads", profilePicturePath);

            environment.Setup(x => x.WebRootPath).Returns(webRootPath);

            repo.Setup(x => x.GetUserById(userId)).ReturnsAsync(user);

            var result = await controller.DeletePhoto();

            var okResult = Assert.IsType<OkResult>(result);
            Assert.NotNull(okResult);
        }
        #endregion
        #region start change name endpoint 
        [Fact]
        public async Task ChangeName_WithInvalidUserId_ReturnsNotFound()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
            var changeNameDto = new NameDto()
            {
                Name = "ahmed"
            };


            repo.Setup(x => x.GetUserById(userId));

            var result = await controller.ChangeName(changeNameDto);

            var notFound = Assert.IsType<NotFoundResult>(result);
            Assert.NotNull(notFound);
        }
        [Fact]
        public async Task ChangeName_WithUnSuccussedChangeName_ReturnsBadRequest()
        {
            var nameDto = new NameDto
            {
                Name = "ahmed"
            };
            var user = new Mock<ApplicationUser>();
            var failedResult = IdentityResult.Failed(new IdentityError
            {
                Code = "User Name error",
                Description = "failed to set user name "
            });
            repo.Setup(x => x.GetUserById(userId)).ReturnsAsync(user.Object);
            userManager.Setup(x => x.SetUserNameAsync(user.Object, nameDto.Name)).ReturnsAsync(failedResult);

            var result = await controller.ChangeName(nameDto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest);
        }
        [Fact]
        public async Task ChangeName_WithSuccussedChangeName_ReturnsNoContent()
        {
            var nameDto = new NameDto
            {
                Name = "sayed"
            };
            var user = new Mock<ApplicationUser>();

            var identityResult = IdentityResult.Success;

            repo.Setup(x => x.GetUserById(userId)).ReturnsAsync(user.Object);
            userManager.Setup(x => x.SetUserNameAsync(user.Object, nameDto.Name)).ReturnsAsync(identityResult);

            var result = await controller.ChangeName(nameDto);

            var noContent = Assert.IsType<NoContentResult>(result);
            Assert.NotNull(noContent);
        }
        #endregion
        #region start change email endpoint
        [Fact]
        public async Task ChangeEmail_WithInvalidUserId_ReturnsNotFound()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
            var email = "email.com";

            repo.Setup(x => x.GetUserById(userId));

            var result = await controller.ChangeEmail(email);

            var notFound = Assert.IsType<NotFoundResult>(result);
            Assert.NotNull(notFound);
        }
        [Fact]
        public async Task ChangeEmail_WithInValidEmailAddress_ReturnsBadRequest()
        {
            var user = new Mock<ApplicationUser>();
            var email = "email.com";
            repo.Setup(x => x.GetUserById(userId)).ReturnsAsync(user.Object);

            var result = await controller.ChangeEmail(email);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest);
        }
        [Fact]
        public async Task ChangeEmail_WithUnSuccussedChangeEmail_ReturnsBadRequest()
        {
            var user = new Mock<ApplicationUser>();
            var email = "1234@gamil.com";
            var identityResult = IdentityResult.Failed(new IdentityError
            {
                Code = "change email",
                Description = "failed to set user name "
            });

            repo.Setup(x => x.GetUserById(userId)).ReturnsAsync(user.Object);
            userManager.Setup(x => x.UpdateAsync(user.Object)).ReturnsAsync(identityResult);

            var result = await controller.ChangeEmail(email);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest);
        }
        [Fact]
        public async Task ChangeEmail_WithSuccussedChangeEmail_ReturnsNoContent()
        {
            var user = new Mock<ApplicationUser>();
            var email = "1234@gmail.com";
            var identityResult = IdentityResult.Success;
            repo.Setup(x => x.GetUserById(userId)).ReturnsAsync(user.Object);
            userManager.Setup(x => x.UpdateAsync(user.Object)).ReturnsAsync(identityResult);

            var result = await controller.ChangeEmail(email);

            var noContent = Assert.IsType<NoContentResult>(result);
            Assert.NotNull(noContent);
        }
        #endregion
        #region start reset password endpoint 
        [Fact]
        public async Task ResetPassword_WithInValidUserId_ReturensNotFound()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
            var dto = new ChangePasswordDto
            {
                NewPassword = "newPass",
                OldPassword = "oldPass"
            };

            repo.Setup(x => x.GetUserById(userId));

            var result = await controller.ResetPassword(dto);

            var notFound = Assert.IsType<NotFoundResult>(result);
            Assert.NotNull(notFound);
        }
        [Fact]
        public async Task ResetPassword_WithNotCorrectOldPassword_ReturnsBadRequest()
        {
            var user = new ApplicationUser();
            var dto = new ChangePasswordDto
            {
                NewPassword = "newPass",
                OldPassword = "oldPass"
            };

            repo.Setup(x => x.GetUserById(userId)).ReturnsAsync(user);
            userManager.Setup(x => x.CheckPasswordAsync(user, dto.OldPassword));

            var result = await controller.ResetPassword(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest);
        }
        [Fact]
        public async Task ResetPassword_WithUnSuccussedChangePassword_ReturnsBadRequest()
        {
            var user = new ApplicationUser();
            var dto = new ChangePasswordDto
            {
                NewPassword = "newPass",
                OldPassword = "oldPass"
            };

            repo.Setup(x => x.GetUserById(userId)).ReturnsAsync(user);
            userManager.Setup(x => x.CheckPasswordAsync(user, dto.OldPassword)).ReturnsAsync(true);
            userManager.Setup(x => x.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword)).ReturnsAsync(IdentityResult.Failed());

            var result = await controller.ResetPassword(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest);
        }
        [Fact]
        public async Task ResetPassword_WithSuccussedChangePassword_ReturnsNoContent()
        {
            var user = new ApplicationUser();
            var dto = new ChangePasswordDto
            {
                NewPassword = "newPass",
                OldPassword = "oldPass"
            };

            repo.Setup(x => x.GetUserById(userId)).ReturnsAsync(user);
            userManager.Setup(x => x.CheckPasswordAsync(user, dto.OldPassword)).ReturnsAsync(true);
            userManager.Setup(x => x.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword)).ReturnsAsync(IdentityResult.Success);

            var result = await controller.ResetPassword(dto);

            var noContent = Assert.IsType<NoContentResult>(result);
            Assert.NotNull(noContent);
        }
        #endregion
        #region start get profile endpoint 
        [Fact]
        public async Task GetProfile_WithInValidUserId_ReturensNotFound()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
            var dto = new ChangePasswordDto
            {
                NewPassword = "newPass",
                OldPassword = "oldPass"
            };

            repo.Setup(x => x.GetUserById(userId));

            var result = await controller.GetProfile();

            var notFound = Assert.IsType<NotFoundResult>(result);
            Assert.NotNull(notFound);
        }
        [Fact]
        public async Task GetProfile_WithSuccussed_ReturnsOk()
        {
            var user = new ApplicationUser();
            var info = new ProfileDto
            {
                Id = userId,
                Name = "mohamed",
                Email = "email.com",
                ProfilePicture = $"picture/uploads/phot.jpg"
            };

            repo.Setup(x => x.GetUserById(userId)).ReturnsAsync(user);
            var result = await controller.GetProfile();

            Assert.IsType<OkObjectResult>(result);
        }
        #endregion
    }
}
