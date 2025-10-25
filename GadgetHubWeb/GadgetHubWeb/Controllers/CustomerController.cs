using Microsoft.AspNetCore.Mvc;
using GadgetHubWeb.Services;
using GadgetHubWeb.Models;

namespace GadgetHubWeb.Controllers
{
    [ApiController]
    [Route("api/customer")]
    public class CustomerController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(AuthService authService, ILogger<CustomerController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("test-register")]
        public IActionResult TestRegister([FromBody] RegisterRequest request)
        {
            try
            {
                _logger.LogInformation($"Test registration for: {request.Email}");
                
                // Simple test without AuthService
                var customer = new Customer
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Password = "test_password",
                    Phone = "",
                    Address = "",
                    City = "",
                    Country = "",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                return Ok(new
                {
                    success = true,
                    message = "Test registration successful",
                    user = new
                    {
                        firstName = customer.FirstName,
                        lastName = customer.LastName,
                        email = customer.Email
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Test registration error: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Test registration error", error = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                _logger.LogInformation($"Registration attempt for email: {request.Email}");

                // Basic validation
                if (string.IsNullOrEmpty(request.FirstName) || 
                    string.IsNullOrEmpty(request.LastName) || 
                    string.IsNullOrEmpty(request.Email) || 
                    string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { success = false, message = "All fields are required" });
                }

                if (request.Password.Length < 6)
                {
                    return BadRequest(new { success = false, message = "Password must be at least 6 characters long" });
                }

                if (request.Password != request.ConfirmPassword)
                {
                    return BadRequest(new { success = false, message = "Passwords do not match" });
                }

                _logger.LogInformation("All validations passed, attempting database save");

                // FORCE DATABASE SAVE - No fallback for now
                _logger.LogInformation("Attempting database save - NO FALLBACK MODE");
                
                var customer = await _authService.RegisterCustomer(
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.Password,
                    "" // Phone not required
                );

                if (customer != null)
                {
                    _logger.LogInformation($"Customer saved to database with ID: {customer.Id}");
                    
                    // Create session
                    var sessionId = await _authService.CreateSession(customer);

                    return Ok(new
                    {
                        success = true,
                        message = "Registration successful - saved to database!",
                        sessionId = sessionId,
                        user = new
                        {
                            id = customer.Id,
                            firstName = customer.FirstName,
                            lastName = customer.LastName,
                            email = customer.Email,
                            phone = customer.Phone,
                            createdAt = customer.CreatedDate
                        }
                    });
                }
                else
                {
                    return BadRequest(new { success = false, message = "User already exists with this email" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Registration error for email: {request.Email}");
                return StatusCode(500, new { success = false, message = "Registration failed", error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation($"Login attempt for email: {request.Email}");

                // Validate input
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { success = false, message = "Email and password are required" });
                }

                _logger.LogInformation("Input validation passed, attempting database authentication");

                // Try database authentication first
                try
                {
                    var customer = await _authService.AuthenticateCustomer(request.Email, request.Password);

                    if (customer != null)
                    {
                        _logger.LogInformation($"Database authentication successful for: {request.Email}");
                        
                        // Create session
                        var sessionId = await _authService.CreateSession(customer);

                        return Ok(new
                        {
                            success = true,
                            message = "Login successful - authenticated from database",
                            sessionId = sessionId,
                            user = new
                            {
                                id = customer.Id,
                                firstName = customer.FirstName,
                                lastName = customer.LastName,
                                email = customer.Email,
                                phone = customer.Phone,
                                createdAt = customer.CreatedDate,
                                role = "customer"
                            }
                        });
                    }
                    else
                    {
                        return Unauthorized(new { success = false, message = "Invalid email or password" });
                    }
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, $"Database authentication failed for {request.Email}, using fallback");
                    
                    // Fallback: Simple mock authentication for any email/password
                    if (request.Password.Length >= 6) // Basic validation
                    {
                        var fallbackCustomerId = new Random().Next(1000, 9999);
                        var fallbackSessionId = Guid.NewGuid().ToString();

                        return Ok(new
                        {
                            success = true,
                            message = "Login successful (database unavailable - using fallback)",
                            sessionId = fallbackSessionId,
                            user = new
                            {
                                id = fallbackCustomerId,
                                firstName = "User",
                                lastName = "Name",
                                email = request.Email,
                                phone = "",
                                createdAt = DateTime.UtcNow,
                                role = "customer"
                            }
                        });
                    }
                    else
                    {
                        return Unauthorized(new { success = false, message = "Invalid email or password" });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Login error for email: {request.Email}");
                return StatusCode(500, new { success = false, message = "Login failed", error = ex.Message });
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout([FromBody] LogoutRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.SessionId))
                {
                    return BadRequest(new { success = false, message = "Session ID is required" });
                }

                var success = _authService.LogoutCustomer(request.SessionId);

                if (success)
                {
                    return Ok(new { success = true, message = "Logout successful" });
                }

                return BadRequest(new { success = false, message = "Logout failed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout error");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCustomerCount()
        {
            try
            {
                var count = await _authService.GetCustomerCount();
                return Ok(new { success = true, count = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer count");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }


        // Helper method to validate email format
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }

    // Request/Response models
    public class RegisterRequest
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string ConfirmPassword { get; set; } = "";
    }

    public class LoginRequest
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class LogoutRequest
    {
        public string SessionId { get; set; } = "";
    }
}
