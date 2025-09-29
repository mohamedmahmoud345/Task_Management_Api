using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;
using TaskManagement.Api.DTO;
using TaskManagement.Api.Model;
using TaskManagement.Api.Repositories.IRepositories;

namespace TaskManagement.Api.Controllers
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
        public UserController(ILogger<UserController> logger, IWebHostEnvironment environment, IUserRepository repo, UserManager<ApplicationUser> userManager, IConfiguration config)
        {
            this.logger = logger;
            this.environment = environment;
            this.repo = repo;
            this.userManager = userManager;
        }

        private string GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) throw new UnauthorizedAccessException("User Not Found");

            return userId;
        }

        /// <summary>
        /// Uploads a profile photo for the authenticated user
        /// </summary>
        /// <param name="file">The image file to upload (JPG or PNG, max 10MB)</param>
        /// <returns>Information about the uploaded file</returns>
        /// <response code="200">Photo uploaded successfully</response>
        /// <response code="400">Invalid file type, size, or no file provided</response>
        /// <response code="500">An error occurred while uploading the photo</response>   
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
                    file.FileName,
                    Size = file.Length,
                    file.ContentType,
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
        /// <summary>
        /// Retrieves the profile photo URL for the authenticated user
        /// </summary>
        /// <returns>The URL of the user's profile photo</returns>
        /// <response code="200">Returns the profile photo URL</response>
        /// <response code="404">No profile photo found for the user</response>
        /// <response code="500">An error occurred while retrieving the photo</response>
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
        /// <summary>
        /// Deletes the profile photo for the authenticated user
        /// </summary>
        /// <returns>No content if successful</returns>
        /// <response code="200">Photo deleted successfully</response>
        /// <response code="400">User doesn't have a profile photo</response>
        /// <response code="500">An error occurred while deleting the photo</response>
        [HttpGet("delete-photo")]
        public async Task<IActionResult> DeletePhoto()
        {
            try
            {
                var userId = GetUserId();
                var user = await repo.GetUserById(userId);

                if (user.ProfilePicturePath is null)
                    return BadRequest("The user doesn't have a photo.");

                var path = Path.Combine(environment.WebRootPath, "UpLoads", user.ProfilePicturePath);

                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);

                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "error while delete photo endpoint");
                return StatusCode(500, "Internal Server Error");
            }
        }
        /// <summary>
        /// Changes the display name for the authenticated user
        /// </summary>
        /// <param name="nameDto">The new name data</param>
        /// <returns>No content if successful</returns>
        /// <response code="204">Name changed successfully</response>
        /// <response code="400">Invalid name data or update failed</response>
        /// <response code="404">User not found</response>
        /// <response code="500">An error occurred while changing the name</response>
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
        /// <summary>
        /// Changes the email address for the authenticated user
        /// </summary>
        /// <param name="newEmail">The new email address</param>
        /// <returns>No content if successful</returns>
        /// <response code="204">Email changed successfully</response>
        /// <response code="400">Invalid email format or update failed</response>
        /// <response code="404">User not found</response>
        /// <response code="500">An error occurred while changing the email</response>
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
        /// <summary>
        /// Resets the password for the authenticated user
        /// </summary>
        /// <param name="passwordDto">The password change data containing old and new passwords</param>
        /// <returns>No content if successful</returns>
        /// <response code="204">Password changed successfully</response>
        /// <response code="400">Password change failed</response>
        /// <response code="401">Old password is incorrect</response>
        /// <response code="500">An error occurred while resetting the password</response>
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
        /// <summary>
        /// Retrieves the profile information for the authenticated user
        /// </summary>
        /// <returns>The user's profile information including ID, name, email, and profile picture URL</returns>
        /// <response code="200">Returns the user profile information</response>
        /// <response code="404">User not found</response>
        /// <response code="500">An error occurred while retrieving the profile</response>
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
