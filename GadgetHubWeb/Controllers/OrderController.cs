using Microsoft.AspNetCore.Mvc;
using GadgetHubWeb.Models;
using GadgetHubWeb.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GadgetHubWeb.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrderController : ControllerBase
    {
        private readonly GadgetHubDBContext _context;
        private readonly HttpClient _httpClient;
        private readonly ILogger<OrderController> _logger;

        public OrderController(GadgetHubDBContext context, HttpClient httpClient, ILogger<OrderController> logger)
        {
            _context = context;
            _httpClient = httpClient;
            _logger = logger;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                _logger.LogInformation($"Creating order: {request.OrderNumber}");
                
                // Authentication removed - create order without customer validation

                // Create order in local database
                var order = new Order
                {
                    CustomerId = 1, // Default customer ID since authentication is removed
                    OrderNumber = request.OrderNumber,
                    TotalAmount = request.Total,
                    Status = "pending",
                    OrderDate = DateTime.UtcNow,
                    Notes = $"Payment Method: {request.PaymentMethod}"
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Order created in local database with ID: {order.Id}");

                // Create order items with real product prices from database
                foreach (var item in request.Items)
                {
                    // Map product name to ProductId
                    var productId = GetProductIdFromName(item.Name);
                    
                    // Fetch the actual product from database to get real price
                    var product = await _context.Products.FindAsync(productId);
                    if (product == null)
                    {
                        _logger.LogWarning($"Product not found for ID {productId}, using frontend price");
                        product = new Product { Price = item.Price }; // Fallback to frontend price
                    }
                    
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = productId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price, // Use real database price
                        TotalPrice = product.Price * item.Quantity
                    };

                    _context.OrderItems.Add(orderItem);
                    _logger.LogInformation($"Created order item for product {item.Name} (ID: {productId}) with real price: {product.Price}");
                }

                await _context.SaveChangesAsync();

                // Recalculate total amount based on real product prices
                order.TotalAmount = order.OrderItems.Sum(item => item.TotalPrice);
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Order items created for order {order.Id} with updated total amount: {order.TotalAmount}");

                // Also forward to GadgetHubAPI for processing with distributors
                try
                {
                    var apiOrderRequest = new
                    {
                        CustomerId = 1, // Default customer ID since authentication is removed
                        OrderItems = request.Items.Select(item => new
                        {
                            ProductId = GetProductIdFromName(item.Name), // Map product name to ID
                            Quantity = item.Quantity
                        }).ToList(),
                        Notes = $"Payment Method: {request.PaymentMethod}"
                    };

                    var apiResponse = await _httpClient.PostAsJsonAsync("https://localhost:7000/api/Order/create", apiOrderRequest);
                    
                    if (apiResponse.IsSuccessStatusCode)
                    {
                        _logger.LogInformation($"Order forwarded to GadgetHubAPI successfully");
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to forward order to GadgetHubAPI: {apiResponse.StatusCode}");
                    }
                }
                catch (Exception apiEx)
                {
                    _logger.LogError(apiEx, "Error forwarding order to GadgetHubAPI");
                    // Don't fail the order creation if API call fails
                }

                return Ok(new
                {
                    success = true,
                    orderId = order.Id,
                    orderNumber = order.OrderNumber,
                    message = "Order created successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        [HttpGet("test-db")]
        public async Task<IActionResult> TestDatabase()
        {
            try
            {
                _logger.LogInformation("Testing database connection...");
                
                // Test database connection
                var canConnect = await _context.Database.CanConnectAsync();
                _logger.LogInformation($"Database can connect: {canConnect}");
                
                // Test if tables exist
                var ordersCount = await _context.Orders.CountAsync();
                var customersCount = await _context.Customers.CountAsync();
                
                _logger.LogInformation($"Orders count: {ordersCount}, Customers count: {customersCount}");
                
                return Ok(new { 
                    success = true, 
                    canConnect = canConnect,
                    ordersCount = ordersCount,
                    customersCount = customersCount,
                    message = "Database test successful"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database test failed");
                return StatusCode(500, new { success = false, message = "Database test failed", error = ex.Message });
            }
        }

        [HttpGet("customer/{sessionId}")]
        public IActionResult GetCustomerOrders(string sessionId)
        {
            try
            {
                _logger.LogInformation($"Getting orders for sessionId: {sessionId}");
                
                // Return mock empty orders for now to avoid database issues
                var mockOrders = new List<object>();

                _logger.LogInformation($"Returning {mockOrders.Count} mock orders");

                return Ok(new { success = true, orders = mockOrders });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer orders");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        [HttpGet("simple")]
        public IActionResult GetSimpleOrders()
        {
            try
            {
                _logger.LogInformation("Simple orders endpoint called");
                
                // Return a simple test response
                return Ok(new
                {
                    success = true,
                    message = "Simple orders endpoint is working",
                    timestamp = DateTime.UtcNow,
                    orders = new List<object>()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in simple orders endpoint");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        private int GetProductIdFromName(string productName)
        {
            // Map product names to IDs - in a real application, this would come from a database
            var productMap = new Dictionary<string, int>
            {
                {"iPhone 15 Pro Max", 1},
                {"iPhone 15 Pro", 2},
                {"iPhone 15", 3},
                {"MacBook Pro 16-inch M3 Max", 4},
                {"MacBook Pro 14-inch M3", 5},
                {"MacBook Air 15-inch M2", 6},
                {"iPad Pro 12.9-inch M2", 7},
                {"iPad Pro 11-inch M2", 8},
                {"iPad Air 10.9-inch M1", 9},
                {"Apple Watch Series 9", 10},
                {"Apple Watch SE 2nd Gen", 11},
                {"Apple Watch Ultra 2", 12},
                {"AirPods Pro 2nd Gen", 13},
                {"AirPods 3rd Gen", 14},
                {"AirPods Max", 15},
                {"Apple TV 4K 3rd Gen", 16},
                {"Apple TV HD", 17},
                {"Apple TV 4K 2nd Gen", 18},
                {"Magic Keyboard for iPad Pro", 19},
                {"Apple Pencil 2nd Gen", 20},
                {"MagSafe Charger", 21}
            };

            return productMap.TryGetValue(productName, out var productId) ? productId : 1; // Default to 1 if not found
        }
    }

    public class CreateOrderRequest
    {
        public string SessionId { get; set; } = "";
        public string OrderNumber { get; set; } = "";
        public decimal Subtotal { get; set; }
        public decimal Shipping { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public string PaymentMethod { get; set; } = "";
        public CustomerInfo CustomerInfo { get; set; } = new();
        public List<OrderItemRequest> Items { get; set; } = new();
    }

    public class CustomerInfo
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Address { get; set; } = "";
        public string City { get; set; } = "";
        public string Country { get; set; } = "";
    }

    public class OrderItemRequest
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Image { get; set; } = "";
        public string DistributorName { get; set; } = "";
    }
}
