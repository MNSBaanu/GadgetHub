using Microsoft.AspNetCore.Mvc;
using GadgetHubWeb.Services;
using GadgetHubWeb.Data;
using Microsoft.EntityFrameworkCore;

namespace GadgetHubWeb.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly GadgetHubDBContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(AuthService authService, GadgetHubDBContext context, ILogger<AdminController> logger)
        {
            _authService = authService;
            _context = context;
            _logger = logger;
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { success = true, message = "Admin controller is working" });
        }

        [HttpPost("login")]
        public IActionResult AdminLogin([FromBody] AdminLoginRequest request)
        {
            try
            {
                _logger.LogInformation($"Admin login attempt for email: {request?.Email}");
                
                // Validate input
                if (string.IsNullOrEmpty(request?.Email) || string.IsNullOrEmpty(request?.Password))
                {
                    _logger.LogWarning("Admin login failed: Missing email or password");
                    return BadRequest(new { success = false, message = "Email and password are required" });
                }

                // Test database connection first
                try
                {
                    var canConnect = _context.Database.CanConnect();
                    _logger.LogInformation($"Database connection test: {canConnect}");
                }
                catch (Exception dbEx)
                {
                    _logger.LogWarning(dbEx, "Database connection test failed, but continuing with admin login");
                }

                // Authenticate admin with hardcoded credentials (doesn't need database)
                bool isValid = _authService.AuthenticateAdmin(request.Email, request.Password);
                _logger.LogInformation($"Admin authentication result: {isValid}");

                if (isValid)
                {
                    // Generate session ID
                    var sessionId = Guid.NewGuid().ToString();
                    _logger.LogInformation($"Admin login successful, session created: {sessionId}");
                    
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

                _logger.LogWarning($"Admin login failed for email: {request.Email}");
                return Unauthorized(new { success = false, message = "Invalid admin credentials" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Admin login error for email: {request?.Email}");
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPost("logout")]
        public IActionResult AdminLogout([FromBody] AdminLogoutRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.SessionId))
                {
                    return BadRequest(new { success = false, message = "Session ID is required" });
                }

                _logger.LogInformation($"Admin logged out: {request.SessionId}");
                return Ok(new { success = true, message = "Admin logout successful" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin logout error");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        [HttpPost("verify")]
        public IActionResult VerifyAdminSession([FromBody] AdminVerifyRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.SessionId))
                {
                    return BadRequest(new { success = false, message = "Session ID is required" });
                }

                // For simplicity, we'll just verify the session ID format
                // In production, you'd validate against a sessions table
                if (Guid.TryParse(request.SessionId, out _))
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Admin session valid",
                        admin = new
                        {
                            id = 1,
                            email = "admin@gadgethub.com",
                            name = "Administrator",
                            role = "admin"
                        }
                    });
                }

                return Unauthorized(new { success = false, message = "Invalid admin session" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin session verification error");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var totalCustomers = await _context.Customers.CountAsync(c => c.IsActive);
                var totalOrders = await _context.Orders.CountAsync();
                var totalRevenue = await _context.Orders.SumAsync(o => o.TotalAmount);
                var totalProducts = await _context.Products.CountAsync();

                return Ok(new
                {
                    success = true,
                    totalCustomers = totalCustomers,
                    totalOrders = totalOrders,
                    totalRevenue = totalRevenue,
                    totalProducts = totalProducts
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats");
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetOrders()
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.OrderItems)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(50)
                    .Select(o => new
                    {
                        id = o.Id,
                        orderNumber = o.OrderNumber,
                        customerId = o.CustomerId,
                        totalAmount = o.TotalAmount,
                        status = o.Status,
                        orderDate = o.OrderDate,
                        itemCount = o.OrderItems.Count
                    })
                    .ToListAsync();

                return Ok(new { success = true, orders = orders });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        [HttpGet("customers")]
        public async Task<IActionResult> GetCustomers()
        {
            try
            {
                var customers = await _context.Customers
                    .Where(c => c.IsActive)
                    .OrderByDescending(c => c.CreatedDate)
                    .Select(c => new
                    {
                        id = c.Id,
                        firstName = c.FirstName,
                        lastName = c.LastName,
                        email = c.Email,
                        phone = c.Phone,
                        createdDate = c.CreatedDate,
                        lastLoginDate = c.LastLogin
                    })
                    .ToListAsync();

                return Ok(new { success = true, customers = customers });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customers");
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("products")]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var products = await _context.Products
                    .OrderBy(p => p.Name)
                    .Select(p => new
                    {
                        id = p.Id,
                        name = p.Name,
                        price = p.Price,
                        category = p.Category,
                        brand = "GadgetHub", // Default brand since it's not in the model
                        stock = p.Stock
                    })
                    .ToListAsync();
                
                return Ok(new { success = true, products = products });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products");
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("analytics")]
        public IActionResult GetAnalytics()
        {
            try
            {
                var analytics = new
                {
                    // Monthly revenue for last 12 months
                    monthlyRevenue = new List<object>
                    {
                        new { month = "Jan", revenue = 15000 },
                        new { month = "Feb", revenue = 18000 },
                        new { month = "Mar", revenue = 22000 },
                        new { month = "Apr", revenue = 19000 },
                        new { month = "May", revenue = 25000 },
                        new { month = "Jun", revenue = 28000 }
                    },
                    // Top selling categories
                    topCategories = new List<object>
                    {
                        new { category = "Smartphones", sales = 45 },
                        new { category = "Laptops", sales = 32 },
                        new { category = "Accessories", sales = 28 },
                        new { category = "Tablets", sales = 15 }
                    }
                };

                return Ok(new { success = true, analytics = analytics });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting analytics");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }
    }

    // Request models
    public class AdminLoginRequest
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class AdminLogoutRequest
    {
        public string SessionId { get; set; } = "";
    }

    public class AdminVerifyRequest
    {
        public string SessionId { get; set; } = "";
    }
}
