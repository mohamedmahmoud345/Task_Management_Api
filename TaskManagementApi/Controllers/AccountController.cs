using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManagement.Api.DTO;
using TaskManagement.Api.Model;

namespace TaskManagement.Api.Controllers
{
    /// <summary>
    /// Handles user authenitcation and account management
    /// </summary>
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

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="request">user registration details</param>
        /// <returns>Success message if registration is successful</returns>
        /// <response code = "200">User registered successfully</response>
        /// <response code = "400">Invalid registration data</response>
        /// <response code = "500">Internal server error</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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


            return Ok(new {message = "User Register Successfully"});
        }

        /// <summary>
        /// Authenticate user and generate JWT token 
        /// </summary>
        /// <param name="request">User login credentials</param>
        /// <returns>JWT token and user information</returns>
        /// <response code = "200">Returns the JWT token and user details</reponse> 
        /// <response code = "400">Invalid credentials</response>
        /// <response code = "500">Internal server error</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login(UserDto request)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);

            var user =await _user.FindByNameAsync(request.UserName);
            if(user != null &&await _user.CheckPasswordAsync(user , request.Password))
            {
                var token = GenerateJwtToken(user);

                return Ok(new
                {
                    token,
                    userId = user.Id,
                    userName = user.UserName,
                    email = user.Email,
                });
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
