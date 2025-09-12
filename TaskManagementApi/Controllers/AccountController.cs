using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManagementApi.DTO;
using TaskManagementApi.Model;

namespace TaskManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _user;
        private readonly IConfiguration _config;
        private readonly ILogger<AccountController> _logger;
        public AccountController
            (UserManager<ApplicationUser> user, IConfiguration config, ILogger<AccountController> logger)
        {
            _user = user;
            _config = config;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var user = new ApplicationUser()
                {
                    UserName = request.UserName,
                    Email = request.Email
                };

                var result = await _user.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                    return BadRequest(result.Errors.Select(e => e.Description));


                return Ok(new {message = "User Register Successfully"});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error During User Registeration");
                return StatusCode(500, "An Error Occured During Registeration");
            }
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(UserDto request)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var user =await _user.FindByNameAsync(request.UserName);
                if(user != null &&await _user.CheckPasswordAsync(user , request.Password))
                {
                    var token = GenerateJwtToken(user);

                    return Ok(new
                    {
                        token = token,
                        userId = user.Id,
                        userName = user.UserName,
                        email = user.Email,
                    });
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error During User Login");
                return StatusCode(500, "An Error Occured During User Login");
            }
            return Unauthorized();
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            List<Claim> userClaim = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier , user.Id),
                new Claim(ClaimTypes.Name , user.UserName ?? ""),
                new Claim(ClaimTypes.Email , user.Email??"")
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: userClaim,
                expires: DateTime.Now.AddDays(1),
                signingCredentials : creds
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
