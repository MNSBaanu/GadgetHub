using Microsoft.AspNetCore.Mvc;

namespace GadgetHubWeb.Controllers
{
    [ApiController]
    [Route("api/simple-admin")]
    public class SimpleAdminController : ControllerBase
    {
        private readonly ILogger<SimpleAdminController> _logger;

        public SimpleAdminController(ILogger<SimpleAdminController> logger)
        {
            _logger = logger;
        }

        [HttpPost("login")]
        public IActionResult AdminLogin([FromBody] AdminLoginRequest request)
        {
            try
            {
                _logger.LogInformation($"Simple admin login attempt for email: {request?.Email}");
                
                // Validate input
                if (string.IsNullOrEmpty(request?.Email) || string.IsNullOrEmpty(request?.Password))
                {
                    _logger.LogWarning("Simple admin login failed: Missing email or password");
                    return BadRequest(new { success = false, message = "Email and password are required" });
                }

                // Hardcoded admin credentials
                const string ADMIN_EMAIL = "admin@gadgethub.com";
                const string ADMIN_PASSWORD = "admin123";

                bool isValid = request.Email == ADMIN_EMAIL && request.Password == ADMIN_PASSWORD;
                _logger.LogInformation($"Simple admin authentication result: {isValid}");

                if (isValid)
                {
                    // Generate session ID
                    var sessionId = Guid.NewGuid().ToString();
                    _logger.LogInformation($"Simple admin login successful, session created: {sessionId}");
                    
                    return Ok(new
                    {
                        success = true,
                        message = "Admin login successful",
                        sessionId = sessionId,
                        admin = new
                        {
                            id = 1,
                            email = "admin@gadgethub.com",
                            name = "Administrator",
                            role = "admin"
                        }
                    });
                }

                _logger.LogWarning($"Simple admin login failed for email: {request.Email}");
                return Unauthorized(new { success = false, message = "Invalid admin credentials" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Simple admin login error for email: {request?.Email}");
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { success = true, message = "Simple admin controller is working" });
        }
    }
}
