using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GadgetHubAPI.Data;
using GadgetHubAPI.Model;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GadgetHubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly GadgetHubDBContext _context;
        private readonly ILogger<AuthController> _logger;

        public AuthController(GadgetHubDBContext context, ILogger<AuthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Check if user exists
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == request.Email && c.IsActive);

                if (customer == null)
                {
                    return Ok(new { success = false, message = "Invalid email or password" });
                }

                // Verify password
                if (!VerifyPassword(request.Password, customer.Password ?? string.Empty))
                {
                    return Ok(new { success = false, message = "Invalid email or password" });
                }

                // Create session
                var sessionId = GenerateSessionId();
                var session = new UserSession
                {
                    SessionId = sessionId,
                    CustomerId = customer.Id,
                    Email = customer.Email,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(30) // 30 days session
                };

                // Update last login
                customer.LastLogin = DateTime.UtcNow;

                _context.UserSessions.Add(session);
                await _context.SaveChangesAsync();

                // Return success response
                return Ok(new
                {
                    success = true,
                    sessionId = sessionId,
                    user = new
                    {
                        id = customer.Id,
                        firstName = customer.FirstName,
                        lastName = customer.LastName,
                        email = customer.Email,
                        phone = customer.Phone
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return Ok(new { success = false, message = "An error occurred during login" });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                // Check if email already exists
                var existingCustomer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == request.Email);

                if (existingCustomer != null)
                {
                    return Ok(new { success = false, message = "Email already registered" });
                }

                // Create new customer
                var customer = new Customer
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Phone = request.Phone,
                    Address = request.Address,
                    City = request.City,
                    Country = request.Country,
                    Password = HashPassword(request.Password),
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Registration successful" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return Ok(new { success = false, message = "An error occurred during registration" });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            try
            {
                var session = await _context.UserSessions
                    .FirstOrDefaultAsync(s => s.SessionId == request.SessionId);

                if (session != null)
                {
                    _context.UserSessions.Remove(session);
                    await _context.SaveChangesAsync();
                }

                return Ok(new { success = true, message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return Ok(new { success = false, message = "An error occurred during logout" });
            }
        }

        [HttpPost("validate-session")]
        public async Task<IActionResult> ValidateSession([FromBody] ValidateSessionRequest request)
        {
            try
            {
                var session = await _context.UserSessions
                    .Include(s => s.Customer)
                    .FirstOrDefaultAsync(s => s.SessionId == request.SessionId && s.ExpiresAt > DateTime.UtcNow);

                if (session == null)
                {
                    return Ok(new { success = false, message = "Invalid or expired session" });
                }

                return Ok(new
                {
                    success = true,
                    user = new
                    {
                        id = session.Customer?.Id ?? 0,
                        firstName = session.Customer?.FirstName ?? string.Empty,
                        lastName = session.Customer?.LastName ?? string.Empty,
                        email = session.Customer?.Email ?? string.Empty,
                        phone = session.Customer?.Phone ?? string.Empty
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during session validation");
                return Ok(new { success = false, message = "An error occurred during session validation" });
            }
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(hashedPassword))
                return false;

            var hashedInput = HashPassword(password);
            return hashedInput == hashedPassword;
        }

        private string GenerateSessionId()
        {
            return Guid.NewGuid().ToString();
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LogoutRequest
    {
        public string SessionId { get; set; } = string.Empty;
    }

    public class ValidateSessionRequest
    {
        public string SessionId { get; set; } = string.Empty;
    }
}
