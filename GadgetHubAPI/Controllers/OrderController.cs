using Microsoft.AspNetCore.Mvc;
using GadgetHubAPI.Services;
using GadgetHubAPI.DTO;
using GadgetHubAPI.Data;
using AutoMapper;

namespace GadgetHubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly OrderRepo _orderRepo;
        private readonly QuotationComparisonRepo _quotationComparisonRepo;
        private readonly OrderProcessingService _orderProcessingService;
        private readonly DistributorService _distributorService;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderController> _logger;
        private readonly GadgetHubDBContext _context;

        public OrderController(
            OrderRepo orderRepo,
            QuotationComparisonRepo quotationComparisonRepo,
            OrderProcessingService orderProcessingService,
            DistributorService distributorService,
            IMapper mapper,
            ILogger<OrderController> logger,
            GadgetHubDBContext context)
        {
            _orderRepo = orderRepo;
            _quotationComparisonRepo = quotationComparisonRepo;
            _orderProcessingService = orderProcessingService;
            _distributorService = distributorService;
            _mapper = mapper;
            _logger = logger;
            _context = context;
        }

        [HttpPost("create")]
        public async Task<ActionResult<OrderReadDTO>> CreateOrder([FromBody] OrderCreateDTO orderRequest)
        {
            try
            {
                _logger.LogInformation($"Creating order for customer: {orderRequest.CustomerId}");
                
                // Create a simple order first without complex processing
                var order = _mapper.Map<Model.Order>(orderRequest);
                order.OrderNumber = GenerateOrderNumber();
                order.OrderDate = DateTime.UtcNow;
                order.Status = "Processing";
                order.Notes = "Order created - awaiting quotation processing";
                
                // Get current product prices from ProductService to match what's displayed on frontend
                foreach (var orderItem in order.OrderItems)
                {
                    // Get the current best product price (same as displayed on frontend)
                    var currentPrice = await GetCurrentProductPrice(orderItem.ProductId);
                    orderItem.UnitPrice = currentPrice;
                    orderItem.TotalPrice = orderItem.UnitPrice * orderItem.Quantity;
                    _logger.LogInformation($"Using current product price: {orderItem.UnitPrice:C} (Product ID: {orderItem.ProductId})");
                }
                
                // Calculate total amount
                order.TotalAmount = order.OrderItems.Sum(oi => oi.TotalPrice);
                _logger.LogInformation($"Order {order.OrderNumber} total amount calculated: {order.TotalAmount:C}");
                
                // Set default dates
                order.ShippedDate = DateTime.UtcNow.AddDays(2);
                order.DeliveredDate = DateTime.UtcNow.AddDays(7);
                
                var success = _orderRepo.AddOrder(order);
                if (!success)
                {
                    throw new Exception("Failed to create order in database");
                }
                
                _logger.LogInformation($"Order {order.OrderNumber} created successfully with total: {order.TotalAmount:C}");
                
                // ENABLED: Automatic quotation request - system will now request quotations from all distributors
                try
                {
                    _logger.LogInformation($"🔄 Starting automatic quotation request for order {order.OrderNumber}");
                    await RequestQuotationsForOrder(order.Id, order.OrderItems);
                    _logger.LogInformation($"✅ Successfully requested quotations for order {order.OrderNumber}");
                }
                catch (Exception quotationEx)
                {
                    _logger.LogWarning(quotationEx, $"⚠️ Quotation request failed for order {order.OrderNumber}, but order was created successfully");
                }
                
                var orderDTO = _mapper.Map<OrderReadDTO>(order);
                return Ok(orderDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, "Internal server error");
            }
        }
        
        private string GenerateOrderNumber()
        {
            return $"GH-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }

        [HttpGet("{id}")]
        public ActionResult<OrderReadDTO> GetOrder(int id)
        {
            try
            {
                var order = _orderRepo.GetOrderById(id);
                if (order == null)
                {
                    return NotFound();
                }

                var orderDTO = _mapper.Map<OrderReadDTO>(order);
                return Ok(orderDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("customer/{customerId}")]
        public ActionResult<List<OrderReadDTO>> GetOrdersByCustomer(int customerId)
        {
            try
            {
                var orders = _orderRepo.GetOrdersByCustomerId(customerId);
                var orderDTOs = _mapper.Map<List<OrderReadDTO>>(orders);
                
                return Ok(orderDTOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders by customer");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("all")]
        public ActionResult<List<OrderReadDTO>> GetAllOrders()
        {
            try
            {
                _logger.LogInformation("🔍 ADMIN DASHBOARD: Getting all orders");
                
                // Test database connection first
                try 
                {
                    var orderCount = _context.Orders.Count();
                    _logger.LogInformation($"🔍 Database has {orderCount} orders");
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "💥 Database connection failed: {Message}", dbEx.Message);
                    return StatusCode(500, $"Database connection failed: {dbEx.Message}");
                }
                
                var orders = _orderRepo.GetAllOrders();
                
                if (orders == null)
                {
                    _logger.LogWarning("⚠️ Orders list is null");
                    return Ok(new List<OrderReadDTO>());
                }
                
                if (!orders.Any())
                {
                    _logger.LogInformation("ℹ️ No orders found in database");
                    return Ok(new List<OrderReadDTO>());
                }
                
                _logger.LogInformation($"✅ Found {orders.Count} orders from repository");
                
                // Simple mapping without AutoMapper to avoid issues
                var orderDTOs = orders.Select(order => new OrderReadDTO
                {
                    Id = order.Id,
                    OrderNumber = order.OrderNumber,
                    CustomerId = order.CustomerId,
                    CustomerName = order.Customer != null ? $"{order.Customer.FirstName} {order.Customer.LastName}" : "Unknown",
                    OrderDate = order.OrderDate,
                    TotalAmount = order.TotalAmount,
                    Status = order.Status,
                    ShippedDate = order.ShippedDate,
                    DeliveredDate = order.DeliveredDate,
                    Notes = order.Notes,
                    OrderItems = order.OrderItems?.Select(oi => new OrderItemReadDTO
                    {
                        Id = oi.Id,
                        ProductId = oi.ProductId,
                        ProductName = oi.Product?.Name ?? "Unknown Product",
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice
                    }).ToList() ?? new List<OrderItemReadDTO>()
                }).ToList();
                
                _logger.LogInformation($"✅ Successfully created {orderDTOs.Count} order DTOs");
                
                return Ok(orderDTOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 CRITICAL ERROR in GetAllOrders: {Message}", ex.Message);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}/status")]
        public ActionResult UpdateOrderStatus(int id, [FromBody] OrderStatusUpdateDTO statusUpdate)
        {
            try
            {
                var order = _orderRepo.GetOrderById(id);
                if (order == null)
                {
                    return NotFound();
                }

                var previousStatus = order.Status;
                order.Status = statusUpdate.Status;
                
                if (statusUpdate.ShippedDate.HasValue)
                    order.ShippedDate = statusUpdate.ShippedDate.Value;
                if (statusUpdate.DeliveredDate.HasValue)
                    order.DeliveredDate = statusUpdate.DeliveredDate.Value;

                _orderRepo.UpdateOrder(order);
                
                _logger.LogInformation($"Order {order.OrderNumber} status updated from {previousStatus} to {statusUpdate.Status}");
                
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}/tracking")]
        public async Task<ActionResult<OrderTrackingDTO>> GetOrderTracking(int id)
        {
            try
            {
                var order = _orderRepo.GetOrderById(id);
                if (order == null)
                {
                    return NotFound();
                }

                var quotations = await _quotationComparisonRepo.GetByOrderId(order.Id);
                var trackingInfo = new OrderTrackingDTO
                {
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    Status = order.Status,
                    OrderDate = order.OrderDate,
                    ShippedDate = order.ShippedDate,
                    DeliveredDate = order.DeliveredDate,
                    EstimatedDeliveryDate = CalculateEstimatedDeliveryDate(order, quotations),
                    TrackingDetails = GenerateTrackingDetails(order, quotations),
                    Notes = order.Notes
                };

                return Ok(trackingInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order tracking information");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}/delivery-estimate")]
        public async Task<ActionResult<DeliveryEstimateDTO>> GetDeliveryEstimate(int id)
        {
            try
            {
                var order = _orderRepo.GetOrderById(id);
                if (order == null)
                {
                    return NotFound();
                }

                var quotations = await _quotationComparisonRepo.GetByOrderId(order.Id);
                var estimatedDelivery = CalculateEstimatedDeliveryDate(order, quotations);
                var maxDeliveryDays = quotations.Any() ? quotations.Max(q => q.EstimatedDeliveryDays) : 7;

                var estimate = new DeliveryEstimateDTO
                {
                    OrderNumber = order.OrderNumber,
                    EstimatedDeliveryDate = estimatedDelivery,
                    EstimatedDeliveryDays = maxDeliveryDays,
                    Status = order.Status,
                    Distributors = quotations.Select(q => q.DistributorName).Distinct().ToList(),
                    DeliveryNotes = GenerateDeliveryNotes(order, quotations)
                };

                return Ok(estimate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting delivery estimate");
                return StatusCode(500, "Internal server error");
            }
        }

        private DateTime CalculateEstimatedDeliveryDate(Model.Order order, List<Model.QuotationComparison> quotations)
        {
            if (quotations.Any())
            {
                var maxDeliveryDays = quotations.Max(q => q.EstimatedDeliveryDays);
                return order.OrderDate.AddDays(maxDeliveryDays);
            }
            return order.OrderDate.AddDays(7); // Default 7 days
        }

        private string GenerateDeliveryNotes(Model.Order order, List<Model.QuotationComparison> quotations)
        {
            if (!quotations.Any())
                return "No quotations available for delivery estimate";

            var distributors = quotations.Select(q => q.DistributorName).Distinct();
            var maxDays = quotations.Max(q => q.EstimatedDeliveryDays);
            
            return $"Estimated delivery based on {string.Join(", ", distributors)} quotations. " +
                   $"Maximum delivery time: {maxDays} days from order date.";
        }

        private List<TrackingDetailDTO> GenerateTrackingDetails(Model.Order order, List<Model.QuotationComparison> quotations)
        {
            var trackingDetails = new List<TrackingDetailDTO>();
            
            // Add order creation tracking detail
            trackingDetails.Add(new TrackingDetailDTO
            {
                Status = "Order Created",
                Date = order.OrderDate,
                Description = $"Order {order.OrderNumber} has been created"
            });
            
            // Add shipped date if available
            if (order.ShippedDate.HasValue)
            {
                trackingDetails.Add(new TrackingDetailDTO
                {
                    Status = "Shipped",
                    Date = order.ShippedDate.Value,
                    Description = $"Order has been shipped"
                });
            }
            
            // Add delivered date if available
            if (order.DeliveredDate.HasValue)
            {
                trackingDetails.Add(new TrackingDetailDTO
                {
                    Status = "Delivered",
                    Date = order.DeliveredDate.Value,
                    Description = $"Order has been delivered"
                });
            }
            
            // Add quotation details if available
            if (quotations.Any())
            {
                trackingDetails.Add(new TrackingDetailDTO
                {
                    Status = "Quotations Received",
                    Date = DateTime.UtcNow,
                    Description = $"Received {quotations.Count} quotations from {quotations.Select(q => q.DistributorName).Distinct().Count()} distributors"
                });
            }
            
            return trackingDetails;
        }

        private void FixOrderWithQuotations(Model.Order order, List<Model.QuotationComparison> quotations)
        {
            _logger.LogInformation($"Fixing order {order.OrderNumber} using quotation data");
            
            // Use quotation data to set prices
            foreach (var orderItem in order.OrderItems)
            {
                var quotation = quotations.FirstOrDefault(q => q.ProductId == orderItem.ProductId);
                if (quotation != null)
                {
                    orderItem.UnitPrice = quotation.UnitPrice;
                    orderItem.TotalPrice = quotation.UnitPrice * orderItem.Quantity;
                }
            }
            
            // Set dates based on quotations
            var maxDeliveryDays = quotations.Max(q => q.EstimatedDeliveryDays);
            order.ShippedDate = order.OrderDate.AddDays(2);
            order.DeliveredDate = order.OrderDate.AddDays(2 + maxDeliveryDays);
            
            // Update total amount
            order.TotalAmount = order.OrderItems.Sum(oi => oi.TotalPrice);
        }

        private void FixOrderWithDefaults(Model.Order order)
        {
            _logger.LogInformation($"Fixing order {order.OrderNumber} using default values");
            
            // Fix prices with default values
            foreach (var orderItem in order.OrderItems)
            {
                if (orderItem.UnitPrice == 0 || orderItem.TotalPrice == 0)
                {
                    var defaultUnitPrice = 100.00m; // Default price
                    orderItem.UnitPrice = defaultUnitPrice;
                    orderItem.TotalPrice = defaultUnitPrice * orderItem.Quantity;
                }
            }
            
            // Fix dates with default values
            if (!order.ShippedDate.HasValue)
                order.ShippedDate = order.OrderDate.AddDays(3);
            
            if (!order.DeliveredDate.HasValue)
                order.DeliveredDate = order.OrderDate.AddDays(7);
            
            // Update total amount
            order.TotalAmount = order.OrderItems.Sum(oi => oi.TotalPrice);
        }

        private async Task FixOrderWithRealPrices(Model.Order order)
        {
            _logger.LogInformation($"Fixing order {order.OrderNumber} using real product prices with markup");
            
            // Fix prices with real product prices and markup
            foreach (var orderItem in order.OrderItems)
            {
                var product = _orderRepo.GetProductById(orderItem.ProductId);
                if (product != null)
                {
                    // Get the best distributor price and apply 20% markup
                    var distributorPrice = await GetBestDistributorPrice(orderItem.ProductId);
                    orderItem.UnitPrice = distributorPrice * 1.20m;
                    orderItem.TotalPrice = orderItem.UnitPrice * orderItem.Quantity;
                }
                else
                {
                    // Fallback to default price
                    var defaultUnitPrice = 100.00m;
                    orderItem.UnitPrice = defaultUnitPrice;
                    orderItem.TotalPrice = defaultUnitPrice * orderItem.Quantity;
                }
            }
            
            // Fix dates with default values
            if (!order.ShippedDate.HasValue)
                order.ShippedDate = order.OrderDate.AddDays(3);
            
            if (!order.DeliveredDate.HasValue)
                order.DeliveredDate = order.OrderDate.AddDays(7);
            
            // Update total amount
            order.TotalAmount = order.OrderItems.Sum(oi => oi.TotalPrice);
        }

        private async Task<decimal> GetCurrentProductPrice(int productId)
        {
            try
            {
                _logger.LogInformation($"Getting current product price for product {productId} (same as displayed on frontend)");
                
                // Use ProductService to get the exact same price displayed on frontend
                var productService = HttpContext.RequestServices.GetRequiredService<Services.ProductService>();
                var bestProducts = await productService.GetBestProductsByCategory("all");
                
                var product = bestProducts.FirstOrDefault(p => p.ProductId == productId);
                if (product != null)
                {
                    _logger.LogInformation($"Found product price from ProductService: {product.Price:C}");
                    return product.Price;
                }
                else
                {
                    _logger.LogWarning($"Product {productId} not found in ProductService, using fallback price");
                    return 100.00m; // Fallback price
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting current product price for product {productId}");
                return 100.00m; // Fallback price
            }
        }

        private async Task<decimal> GetBestDistributorPrice(int productId)
        {
            try
            {
                _logger.LogInformation($"Getting best distributor price for product {productId}");
                
                // Create a test quotation request
                var testRequest = new QuotationRequestDTO
                {
                    ProductId = productId,
                    Quantity = 1,
                    Notes = "Price comparison test"
                };
                
                // Get quotations from all distributors
                var quotations = await _distributorService.GetQuotationsFromAllDistributors(new List<QuotationRequestDTO> { testRequest });
                
                if (quotations.Any())
                {
                    // Find the minimum price
                    var bestPrice = quotations.Min(q => q.UnitPrice);
                    _logger.LogInformation($"Best distributor price for product {productId}: {bestPrice:C}");
                    return bestPrice;
                }
                else
                {
                    _logger.LogWarning($"No quotations received for product {productId}, using fallback price");
                    return 100.00m; // Fallback price
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting best distributor price for product {productId}");
                return 100.00m; // Fallback price
            }
        }

        private async Task RequestQuotationsForOrder(int orderId, List<Model.OrderItem> orderItems)
        {
            try
            {
                _logger.LogInformation($"Requesting quotations for order {orderId} with {orderItems.Count} items");
                
                // Convert order items to quotation requests
                var quotationRequests = orderItems.Select(item => new QuotationRequestDTO
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Notes = $"Quotation request for order {orderId}"
                }).ToList();
                
                // Create the quotation request with order ID
                var requestWithOrderId = new QuotationRequestWithOrderId
                {
                    OrderId = orderId,
                    QuotationRequests = quotationRequests
                };
                
                // Get quotations from all distributors
                var quotations = await _distributorService.GetQuotationsFromAllDistributors(requestWithOrderId.QuotationRequests);
                
                // Save quotation comparisons to database
                await _quotationComparisonRepo.SaveQuotationComparisons(quotations, orderId);
                
                _logger.LogInformation($"Successfully requested and saved {quotations.Count} quotations for order {orderId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error requesting quotations for order {orderId}");
                throw; // Re-throw to be caught by the calling method
            }
        }
    }
}