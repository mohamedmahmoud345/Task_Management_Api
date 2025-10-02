using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Runtime.InteropServices;
using TaskManagement.Api.Controllers;
using TaskManagement.Api.DTO;
using TaskManagement.Api.Model;

namespace TaskManagement.Tests.Controllers
{
    public class AccountControllerTest
    {
        private readonly Mock<UserManager<ApplicationUser>> userManager;
        private readonly Mock<IConfiguration> config;
        private readonly Mock<ILogger<AccountController>> logger;
        private readonly AccountController controller;

        public AccountControllerTest()
        {
            config = new Mock<IConfiguration>();
            logger = new Mock<ILogger<AccountController>>();

            var userStore = new Mock<IUserStore<ApplicationUser>>();
            userManager = new Mock<UserManager<ApplicationUser>>(userStore.Object ,
                null, null, null, null, null, null, null, null);

            controller = new AccountController(userManager.Object , config.Object , logger.Object);
        }


        #region start register endpoint 
        [Fact]
        public async Task Register_WithInValidModelState_ReturensBadRequest()
        {
            var user = new ApplicationUser();
            var registerDto = new RegisterDto
            {
                UserName = "mohamed",
                Email = "email.com",
                Password = "123mo5_"
            };

            controller.ModelState.AddModelError("name", "name is required");
            userManager.Setup(x => x.CreateAsync(user , registerDto.Password));

            var result = await controller.Register(registerDto);

            Assert.IsType<BadRequestObjectResult>(result);
        }
        [Fact]
        public async Task Register_WithUnSuccussedCreateAccount_ReturnsBadRequest()
        {
            var registerDto = new RegisterDto
            {
                UserName = "mohamed",
                Email = "email.com",
                Password = "123mo5_"
            };


            userManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password))
                .ReturnsAsync(IdentityResult.Failed());

            var result = await controller.Register(registerDto);

            Assert.IsType<BadRequestObjectResult>(result);
        }
        [Fact]
        public async Task Register_WithSuccussedCreateAccount_ReturnsOk()
        {
            var registerDto = new RegisterDto
            {
                UserName = "mohamed",
                Email = "email.com",
                Password = "123mo5_"
            };
            userManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password))
               .ReturnsAsync(IdentityResult.Success);

            var result = await controller.Register(registerDto);
            Assert.IsType<OkObjectResult>(result);
        }
        [Fact]
        public async Task Register_WithExceptionThrown_Returns500()
        {
            var registerDto = new RegisterDto
            {
                UserName = "mohamed",
                Email = "email.com",
                Password = "123mo5_"
            };
            userManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password))
                .ThrowsAsync(new Exception());

            var result = await controller.Register(registerDto);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500 , objectResult.StatusCode);
        }
        #endregion
        #region start login endpoint
        [Fact]
        public async Task Login_WithInValidModelState_ReturensBadRequest()
        {
            var user = new ApplicationUser();
            var loginDto = new UserDto
            {
                UserName = "mohamed",
                Password = "123mo5_"
            };

            controller.ModelState.AddModelError("name", "name is required");
            userManager.Setup(x => x.FindByNameAsync(loginDto.UserName));

            var result = await controller.Login(loginDto);

            Assert.IsType<BadRequestObjectResult>(result);
        }
        [Fact]
        public async Task Login_WithInValidUser_ReturnsUnauthorized()
        {
            var login = new UserDto
            {
                UserName = "mohamed",
                Password = "123mo5_"
            };

            userManager.Setup(x => x.FindByNameAsync(login.UserName));

            var result = await controller.Login(login);

            Assert.IsType<UnauthorizedResult>(result);
        }
        [Fact]
        public async Task Login_WithInWrongPassword_ReturnsUnauthorized()
        {
            var login = new UserDto
            {
                UserName = "mohamed",
                Password = "123mo5_"
            };
            var user = new ApplicationUser()
            {
                UserName = login.UserName,
            };

            userManager.Setup(x => x.FindByNameAsync(login.UserName)).ReturnsAsync(user);
            userManager.Setup(x => x.CheckPasswordAsync(user, login.Password));

            var result = await controller.Login(login);

            Assert.IsType<UnauthorizedResult>(result);
        }
        [Fact]
        public async Task Login_WithLoginSuccussed_ReturnsOk()
        {
            var login = new UserDto
            {
                UserName = "mohamed",
                Password = "123mo5_"
            };
            var user = new ApplicationUser()
            {
                UserName = login.UserName,
                Email = "email.com",
                Id = "1"
            };

            userManager.Setup(x => x.FindByNameAsync(login.UserName)).ReturnsAsync(user);
            userManager.Setup(x => x.CheckPasswordAsync(It.IsAny<ApplicationUser>(), login.Password)).ReturnsAsync(true);
            config.Setup(x => x["Jwt:Key"]).Returns("thisiskeyforcreateunittest12332d._");
            config.Setup(x => x["Jwt:Issuer"]).Returns("this is Issuer");
            config.Setup(x => x["Jwt:Audience"]).Returns("this is Audience");

            var result = await controller.Login(login);

            Assert.IsType<OkObjectResult>(result);
        }
        [Fact]
        public async Task Login_WithExceptionThrown_Returns500()
        {
            var login = new UserDto
            {
                UserName = "mohamed",
                Password = "123mo5_"
            };

            userManager.Setup(x => x.FindByNameAsync(login.UserName)).ThrowsAsync(new Exception());
            var result = await controller.Login(login);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }
        #endregion
    }
}
