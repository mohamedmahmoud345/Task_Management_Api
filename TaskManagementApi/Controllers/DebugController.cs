using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TaskManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DebugController : ControllerBase
    {
        private readonly ILogger<DebugController> _logger;

        public DebugController(ILogger<DebugController> logger)
        {
            _logger = logger;
        }

        [HttpGet("test-no-auth")]
        public IActionResult TestNoAuth()
        {
            return Ok(new
            {
                Message = "This endpoint works without authentication",
                Timestamp = DateTime.Now
            });
        }

        [HttpGet("test-auth-claims")]
        // ✅ NO [Authorize] attribute - let's see what claims we get
        public IActionResult TestAuthClaims()
        {
            try
            {
                var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userName = User.FindFirstValue(ClaimTypes.Name);
                var email = User.FindFirstValue(ClaimTypes.Email);

                // Get all claims for debugging
                var allClaims = User.Claims.Select(c => new {
                    Type = c.Type,
                    Value = c.Value
                }).ToList();

                return Ok(new
                {
                    IsAuthenticated = isAuthenticated,
                    UserId = userId,
                    UserName = userName,
                    Email = email,
                    ClaimsCount = allClaims.Count,
                    AllClaims = allClaims,
                    AuthenticationType = User.Identity?.AuthenticationType,
                    HasAuthorizationHeader = Request.Headers.ContainsKey("Authorization")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in test-auth-claims endpoint");
                return StatusCode(500, new
                {
                    Error = ex.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }

        [HttpPost("test-task-creation-no-auth")]
        // ✅ NO [Authorize] attribute - test the basic flow
        public IActionResult TestTaskCreationNoAuth([FromBody] object data)
        {
            try
            {
                var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var hasAuthHeader = Request.Headers.ContainsKey("Authorization");
                var authHeaderValue = Request.Headers.ContainsKey("Authorization")
                    ? Request.Headers["Authorization"].ToString().Substring(0, Math.Min(50, Request.Headers["Authorization"].ToString().Length)) + "..."
                    : "No auth header";

                return Ok(new
                {
                    Message = "Task creation test (no auth required)",
                    IsAuthenticated = isAuthenticated,
                    UserId = userId,
                    ReceivedData = data,
                    HasAuthorizationHeader = hasAuthHeader,
                    AuthHeaderStart = authHeaderValue,
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in test-task-creation-no-auth endpoint");
                return StatusCode(500, new
                {
                    Error = ex.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }

        [HttpGet("test-jwt-config")]
        public IActionResult TestJwtConfig([FromServices] IConfiguration config)
        {
            try
            {
                return Ok(new
                {
                    JwtKey = string.IsNullOrEmpty(config["Jwt:Key"]) ? "❌ MISSING" : "✅ Present",
                    JwtIssuer = config["Jwt:Issuer"] ?? "❌ MISSING",
                    JwtAudience = config["Jwt:Audience"] ?? "❌ MISSING",
                    KeyLength = config["Jwt:Key"]?.Length ?? 0,
                    Message = "JWT Configuration Check"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}
