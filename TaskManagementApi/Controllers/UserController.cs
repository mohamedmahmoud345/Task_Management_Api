using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TaskManagementApi.DTO;
using TaskManagementApi.Model;
using TaskManagementApi.Repositories.IRepositories;

namespace TaskManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IWebHostEnvironment environment;
        private readonly ILogger<UserController> logger;
        private readonly IUserRepository repo;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration config;
        public UserController(ILogger<UserController> logger, IWebHostEnvironment environment, IUserRepository repo, UserManager<ApplicationUser> userManager, IConfiguration config)
        {
            this.logger = logger;
            this.environment = environment;
            this.repo = repo;
            this.userManager = userManager;
            this.config = config;
        }

        private string GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) throw new UnauthorizedAccessException("User Not Found");

            return userId;
        }

        // upload file 
        [HttpPost("upload-photo")]
        public async Task<IActionResult> UploadPhoto(IFormFile file)
        {
            try
            {
                var userId = GetUserId();

                var userPhoto = await repo.IsUserHasProfilePicture(userId);

                if (userPhoto.HasPhoto)
                {
                    var path = Path.Combine(environment.WebRootPath, "Uploads", userPhoto.PhotoPath);

                    if(System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }

                if (file == null || file.Length == 0) return BadRequest("No file uploaded");

                var extenstions = new[] { ".jpg", ".png" };
                var fileExtenstion = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!extenstions.Contains(fileExtenstion)) return BadRequest("Invalid file type");

                if (file.Length > 10_000_000) return BadRequest("file too large");

                var uploadFolder = Path.Combine(environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "Uploads");

                
                if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + fileExtenstion;
                var filePath = Path.Combine(uploadFolder , uniqueFileName);

                using(var stream = new FileStream(filePath , FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                await repo.UploadPhotoAsync(uniqueFileName , userId);

                var fileInfo = new
                {
                    FileName = file.FileName,
                    Size = file.Length,
                    ContentType = file.ContentType,
                    SavedPath = filePath
                };

                return Ok(JsonSerializer.Serialize(fileInfo));
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error uploading photo");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("Profile-Photo")]
        public async Task<IActionResult> GetPhoto()
        {
            try
            {
                var userId = GetUserId();

                var userPhoto = await repo.IsUserHasProfilePicture(userId);
                if (!userPhoto.HasPhoto)
                    return NotFound("No profile photo found");

                var baseUrl = $"{Request.Scheme}";
                var fileUrl = $"{baseUrl}/Uploads/{userPhoto.PhotoPath}";

                return Ok(new {Url = fileUrl });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error while return the profile photo");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPut("Change-Name")]
        public async Task<IActionResult> ChangeName(NameDto nameDto)
        {
            try
            {
                var userId = GetUserId();

                var user = await repo.GetUserById(userId);
                if (user == null)
                    return NotFound();
                var result =await userManager.SetUserNameAsync(user, nameDto.Name);

                if (!result.Succeeded)
                    return BadRequest(result.Errors);

                return NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while try to change the user name");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPut("Change-Email/{newEmail}")]
        public async Task<IActionResult> ChangeEmail(string newEmail)
        {
            try
            {
                var userId = GetUserId();
                var user = await repo.GetUserById(userId);

                if (user == null)
                    return BadRequest("User Not Found");

                var emailValidat = new EmailAddressAttribute();
                if (!emailValidat.IsValid(newEmail))
                    return BadRequest("Email Not Valid");

                user.Email = newEmail;

                var result = await userManager.UpdateAsync(user);
                if (!result.Succeeded)
                    return BadRequest(result.Errors);

                return NoContent();
                
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while try to change the user email");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPut("reset-password")]
        public async Task<IActionResult> ResetPassword
            (ChangePasswordDto passwordDto)
        {
            try
            {
                var userId = GetUserId();

                var user = await repo.GetUserById(userId);
               
                var isValidPassword = await userManager.CheckPasswordAsync(user , passwordDto.OldPassword);
                if (!isValidPassword)
                    return Unauthorized("Old Password Not Correct");

                var result = await userManager.ChangePasswordAsync
                    (user, passwordDto.OldPassword , passwordDto.NewPassword);
                if (!result.Succeeded)
                    return BadRequest(result.Errors);

                return NoContent();
                
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while try to change the user password");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = GetUserId();
                var user = await repo.GetUserById(userId);

                if(user == null) return NotFound("User Not Found");

                var info = new ProfileDto
                {
                    Id = user.Id,
                    Name = user.UserName,
                    Email = user.Email,
                    ProfilePicture = $"{Request.Scheme}/Uploads/{user.ProfilePicturePath}",
                };

                return Ok(info);

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while try to return profile info");
                return StatusCode(500, "Internal Server Error");
            }
        }

    }
}
