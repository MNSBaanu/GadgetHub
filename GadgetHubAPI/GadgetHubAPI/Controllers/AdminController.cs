using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using GadgetHubAPI.Data;
using GadgetHubAPI.Services;

namespace GadgetHubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ILogger<AdminController> _logger;
        private readonly OrderRepo _orderRepo;
        private readonly ProductRepo _productRepo;
        private readonly QuotationComparisonRepo _quotationComparisonRepo;
        private readonly CustomerRepo _customerRepo;

        // Hardcoded admin credentials
        private const string ADMIN_EMAIL = "admin@gadgethub.com";
        private const string ADMIN_PASSWORD = "admin123";

        public AdminController(
            ILogger<AdminController> logger,
            OrderRepo orderRepo,
            ProductRepo productRepo,
            QuotationComparisonRepo quotationComparisonRepo,
            CustomerRepo customerRepo)
        {
            _logger = logger;
            _orderRepo = orderRepo;
            _productRepo = productRepo;
            _quotationComparisonRepo = quotationComparisonRepo;
            _customerRepo = customerRepo;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] AdminLoginRequest request)
        {
            try
            {
                // Validate hardcoded admin credentials
                if (request.Email == ADMIN_EMAIL && request.Password == ADMIN_PASSWORD)
                {
                    // Generate admin session
                    var sessionId = Guid.NewGuid().ToString();
                    
                    return Ok(new
                    {
                        success = true,
                        sessionId = sessionId,
                        admin = new
                        {
                            email = ADMIN_EMAIL,
                            role = "Administrator",
                            permissions = new[] { "products", "orders", "customers", "analytics" }
                        }
                    });
                }

                return Ok(new { success = false, message = "Invalid admin credentials" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during admin login");
                return Ok(new { success = false, message = "An error occurred during admin login" });
            }
        }

        [HttpPost("validate-session")]
        public IActionResult ValidateSession([FromBody] AdminValidateSessionRequest request)
        {
            try
            {
                // For simplicity, we'll accept any session ID for admin
                // In production, you'd want to store and validate admin sessions
                if (!string.IsNullOrEmpty(request.SessionId))
                {
                    return Ok(new
                    {
                        success = true,
                        admin = new
                        {
                            email = ADMIN_EMAIL,
                            role = "Administrator",
                            permissions = new[] { "products", "orders", "customers", "analytics" }
                        }
                    });
                }

                return Ok(new { success = false, message = "Invalid admin session" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during admin session validation");
                return Ok(new { success = false, message = "An error occurred during admin session validation" });
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout([FromBody] AdminLogoutRequest request)
        {
            try
            {
                // For simplicity, we'll just return success
                // In production, you'd want to invalidate the session
                _logger.LogInformation($"Admin logout requested for session: {request.SessionId}");
                
                return Ok(new { success = true, message = "Admin logged out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during admin logout");
                return Ok(new { success = false, message = "An error occurred during admin logout" });
            }
        }

        [HttpGet("dashboard-stats")]
        public IActionResult GetDashboardStats()
        {
            try
            {
                var orders = _orderRepo?.GetAllOrders() ?? new List<Model.Order>();
                var products = _productRepo?.GetProducts() ?? new List<Model.Product>();
                var quotations = _quotationComparisonRepo?.GetQuotationComparisons() ?? new List<Model.QuotationComparison>();

                var stats = new
                {
                    success = true,
                    totalOrders = orders.Count,
                    totalProducts = products.Count,
                    totalQuotations = quotations.Count,
                    pendingOrders = orders.Count(o => o.Status == "Pending"),
                    completedOrders = orders.Count(o => o.Status == "Completed"),
                    totalRevenue = orders.Where(o => o.Status == "Completed").Sum(o => o.TotalAmount)
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats");
                return Ok(new { success = false, message = "Error loading dashboard statistics" });
            }
        }

        [HttpGet("customers")]
        public IActionResult GetCustomers()
        {
            try
            {
                // Get real customer data from database
                var customers = _customerRepo?.GetCustomers() ?? new List<Model.Customer>();
                
                // Transform to match frontend expectations
                var customerData = customers.Select(c => new
                {
                    customerId = c.Id,
                    id = c.Id,
                    name = $"{c.FirstName} {c.LastName}".Trim(),
                    firstName = c.FirstName,
                    lastName = c.LastName,
                    email = c.Email,
                    phone = c.Phone ?? "",
                    address = c.Address ?? "",
                    city = c.City ?? "",
                    country = c.Country ?? "",
                    createdAt = c.CreatedDate,
                    registrationDate = c.CreatedDate,
                    status = c.IsActive ? "Active" : "Inactive",
                    isActive = c.IsActive,
                    lastLogin = c.LastLogin,
                    totalOrders = c.Orders?.Count ?? 0,
                    totalSpent = c.Orders?.Where(o => o.Status == "Completed").Sum(o => o.TotalAmount) ?? 0m
                }).ToList();

                return Ok(new { success = true, customers = customerData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customers");
                return Ok(new { success = false, message = "Error loading customers" });
            }
        }

        [HttpGet("products")]
        public IActionResult GetProducts()
        {
            try
            {
                var products = _productRepo?.GetProducts() ?? new List<Model.Product>();
                var productList = products.Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    price = 0, // Price now fetched dynamically from distributors
                    stock = 0, // Stock now fetched dynamically from distributors
                    category = p.Category,
                    status = "Dynamic Pricing" // Prices and stock are now dynamic
                }).ToList();

                return Ok(new { success = true, products = productList });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products");
                return Ok(new { success = false, message = "Error loading products" });
            }
        }

    }

    public class AdminLoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AdminValidateSessionRequest
    {
        public string SessionId { get; set; } = string.Empty;
    }

    public class AdminLogoutRequest
    {
        public string SessionId { get; set; } = string.Empty;
    }
}
