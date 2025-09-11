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

        public AccountController(UserManager<ApplicationUser> user, IConfiguration config)
        {
            _user = user;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var user = new ApplicationUser()
            {
                UserName = request.UserName,
                Email = request.Email
            };

            var result = await _user.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));


            return Ok();
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(UserDto request)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);

            var user =await _user.FindByNameAsync(request.UserName);
            if(user != null &&await _user.CheckPasswordAsync(user , request.Password))
            {
                var token = GenerateJwtToken(user);

                return Ok(token);
            }

            return Unauthorized();
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            List<Claim> userClaim = new List<Claim>
            {
                new Claim("UserName" , user.UserName)
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                claims: userClaim,
                expires: DateTime.Now.AddDays(1),
                signingCredentials : creds
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
