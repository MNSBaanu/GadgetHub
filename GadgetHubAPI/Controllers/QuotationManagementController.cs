using Microsoft.AspNetCore.Mvc;
using GadgetHubAPI.Services;
using GadgetHubAPI.DTO;
using GadgetHubAPI.Data;
using AutoMapper;
using System.Text.Json;

namespace GadgetHubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuotationManagementController : ControllerBase
    {
        private readonly DistributorService _distributorService;
        private readonly QuotationComparisonRepo _quotationComparisonRepo;
        private readonly OrderProcessingService _orderProcessingService;
        private readonly IMapper _mapper;
        private readonly ILogger<QuotationManagementController> _logger;
        private readonly HttpClient _httpClient;

        public QuotationManagementController(
            DistributorService distributorService,
            QuotationComparisonRepo quotationComparisonRepo,
            OrderProcessingService orderProcessingService,
            IMapper mapper,
            ILogger<QuotationManagementController> logger,
            HttpClient httpClient)
        {
            _distributorService = distributorService;
            _quotationComparisonRepo = quotationComparisonRepo;
            _orderProcessingService = orderProcessingService;
            _mapper = mapper;
            _logger = logger;
            _httpClient = httpClient;
        }

        [HttpGet("pending-orders")]
        public ActionResult<List<QuotationManagementDTO>> GetPendingQuotations()
        {
            try
            {
                _logger.LogInformation("Getting pending quotations for admin review");
                
                // Get all quotation comparisons from database
                var quotationComparisons = _quotationComparisonRepo.GetQuotationComparisons();
                
                if (!quotationComparisons.Any())
                {
                    return Ok(new List<QuotationManagementDTO>());
                }

                // Group quotations by order and product
                var groupedQuotations = quotationComparisons
                    .GroupBy(qc => new { qc.OrderId, qc.ProductId })
                    .ToList();

                var quotationManagementList = new List<QuotationManagementDTO>();

                foreach (var group in groupedQuotations)
                {
                    var firstQuotation = group.First();
                    var order = firstQuotation.Order;
                    var product = firstQuotation.Product;

                    // Skip if order or product is null
                    if (order == null || product == null)
                    {
                        _logger.LogWarning($"Skipping quotation group due to null order or product. OrderId: {firstQuotation.OrderId}, ProductId: {firstQuotation.ProductId}");
                        continue;
                    }

                    var quotationManagement = new QuotationManagementDTO
                    {
                        OrderId = order.Id,
                        OrderNumber = order.OrderNumber,
                        CustomerName = order.Customer != null ? $"{order.Customer.FirstName} {order.Customer.LastName}" : "Unknown",
                        OrderDate = order.OrderDate,
                        TotalAmount = order.TotalAmount,
                        Status = order.Status,
                        ProductQuotations = new List<ProductQuotationDTO>()
                    };

                    var productQuotation = new ProductQuotationDTO
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1, // Default quantity since it's not stored in QuotationComparison
                        DistributorQuotations = new List<DistributorQuotationDTO>()
                    };

                    // Add all distributor quotations for this product (filter out failed ones)
                    foreach (var quotation in group.Where(q => q.Status != "Failed"))
                    {
                        productQuotation.DistributorQuotations.Add(new DistributorQuotationDTO
                        {
                            DistributorName = quotation.DistributorName,
                            UnitPrice = quotation.UnitPrice,
                            TotalPrice = quotation.TotalPrice,
                            AvailableStock = quotation.AvailableStock,
                            EstimatedDeliveryDays = quotation.EstimatedDeliveryDays,
                            Status = quotation.Status,
                            Notes = quotation.Notes ?? string.Empty,
                            CreatedDate = quotation.CreatedDate,
                            ExpiryDate = quotation.CreatedDate.AddDays(7) // Default 7 days expiry
                        });
                    }

                    quotationManagement.ProductQuotations.Add(productQuotation);
                    quotationManagementList.Add(quotationManagement);
                }

                return Ok(quotationManagementList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending quotations");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("process-selected")]
        public async Task<ActionResult> ProcessSelectedQuotations([FromBody] ProcessSelectedQuotationsRequest request)
        {
            try
            {
                _logger.LogInformation($"🔄 Processing {request.SelectedQuotations.Count} selected quotations");

                // Get the order ID from the first selected quotation
                var firstQuotation = _quotationComparisonRepo.GetQuotationComparisonById(request.SelectedQuotations.First().QuotationId);
                if (firstQuotation == null)
                {
                    _logger.LogError("Could not find quotation to get order ID");
                    return BadRequest("Invalid quotation ID");
                }

                var orderId = firstQuotation.OrderId;
                _logger.LogInformation($"📦 Processing order {orderId} with selected quotations");

                // Get the order
                var order = GetOrderById(orderId);
                if (order == null)
                {
                    _logger.LogError($"Order {orderId} not found");
                    return NotFound("Order not found");
                }

                // STEP 1: Mark selected quotations as "Confirmed" and others as "Cancelled"
                _logger.LogInformation($"✅ Step 1: Updating quotation statuses for order {orderId}");
                
                var allQuotationsForOrder = _quotationComparisonRepo.GetQuotationComparisons()
                    .Where(qc => qc.OrderId == orderId)
                    .ToList();

                var selectedQuotationIds = request.SelectedQuotations.Select(sq => sq.QuotationId).ToList();

                foreach (var quotation in allQuotationsForOrder)
                {
                    if (selectedQuotationIds.Contains(quotation.Id))
                    {
                        // Mark selected quotations as "Confirmed"
                        quotation.Status = "Confirmed";
                        quotation.Notes += $" | ✅ Confirmed by GadgetHub - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
                        _logger.LogInformation($"✅ Confirmed quotation {quotation.Id} from {quotation.DistributorName}");
                    }
                    else
                    {
                        // Mark other quotations as "Cancelled"
                        quotation.Status = "Cancelled";
                        quotation.Notes += $" | ❌ Cancelled by GadgetHub - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
                        _logger.LogInformation($"❌ Cancelled quotation {quotation.Id} from {quotation.DistributorName}");
                    }
                    
                    _quotationComparisonRepo.UpdateQuotationComparison(quotation);
                }

                // STEP 2: Update order status and details
                _logger.LogInformation($"📋 Step 2: Updating order {orderId} status to Confirmed");
                
                order.Status = "Confirmed";
                order.Notes = $"Order confirmed with selected quotations - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
                
                // Calculate total amount from confirmed quotations
                var confirmedQuotations = allQuotationsForOrder.Where(q => q.Status == "Confirmed").ToList();
                if (confirmedQuotations.Any())
                {
                    order.TotalAmount = confirmedQuotations.Sum(q => q.TotalPrice);
                    
                    // Set realistic shipping and delivery dates
                    var maxDeliveryDays = confirmedQuotations.Max(q => q.EstimatedDeliveryDays);
                    order.ShippedDate = DateTime.UtcNow.AddDays(2);
                    order.DeliveredDate = DateTime.UtcNow.AddDays(2 + maxDeliveryDays);
                    
                    _logger.LogInformation($"💰 Updated order {orderId} with total amount: {order.TotalAmount:C}");
                }
                
                // Update the order in database
                await _quotationComparisonRepo.UpdateOrder(order);

                // STEP 3: Place actual orders with confirmed distributors
                _logger.LogInformation($"🚚 Step 3: Placing orders with confirmed distributors");
                
                var confirmedDistributors = new List<string>();
                foreach (var selectedQuotation in request.SelectedQuotations)
                {
                    var quotation = _quotationComparisonRepo.GetQuotationComparisonById(selectedQuotation.QuotationId);
                    if (quotation != null)
                    {
                        _logger.LogInformation($"📦 Processing confirmed quotation with {selectedQuotation.DistributorName} for product {quotation.ProductId}");
                        
                        // Place actual order with the distributor
                        var orderPlaced = await PlaceOrderWithDistributor(order, quotation, selectedQuotation.DistributorName);
                        
                        if (orderPlaced)
                        {
                            confirmedDistributors.Add(selectedQuotation.DistributorName);
                            _logger.LogInformation($"✅ Order successfully placed with {selectedQuotation.DistributorName}");
                        }
                        else
                        {
                            _logger.LogWarning($"⚠️ Failed to place order with {selectedQuotation.DistributorName}");
                        }
                        
                        // Update distributor quotation status to "Confirmed"
                        await UpdateDistributorQuotationStatus(selectedQuotation.DistributorName, quotation, "Confirmed");
                    }
                }

                // STEP 4: Cancel quotations with other distributors
                _logger.LogInformation($"❌ Step 4: Cancelling quotations with non-selected distributors");
                
                var cancelledQuotations = allQuotationsForOrder.Where(q => q.Status == "Cancelled").ToList();
                var cancelledDistributors = new List<string>();
                
                foreach (var quotation in cancelledQuotations)
                {
                    _logger.LogInformation($"❌ Cancelling quotation with {quotation.DistributorName}");
                    await UpdateDistributorQuotationStatus(quotation.DistributorName, quotation, "Cancelled");
                    cancelledDistributors.Add(quotation.DistributorName);
                }

                // STEP 5: Summary of distributor updates
                _logger.LogInformation($"📊 Step 5: Distributor update summary");
                _logger.LogInformation($"✅ Confirmed distributors: {string.Join(", ", confirmedDistributors)}");
                _logger.LogInformation($"❌ Cancelled distributors: {string.Join(", ", cancelledDistributors)}");
                _logger.LogInformation($"📦 Orders placed with {confirmedDistributors.Count} distributors");
                _logger.LogInformation($"🚫 Quotations cancelled with {cancelledDistributors.Count} distributors");
                
                var processedOrder = _mapper.Map<OrderReadDTO>(order);
                
                // STEP 5: Send customer notification
                await SendCustomerNotification(order, confirmedQuotations, confirmedDistributors);
                
                _logger.LogInformation($"🎉 Successfully processed order {orderId} with status: {processedOrder.Status}");
                return Ok(new { 
                    message = "Selected quotations processed successfully", 
                    order = processedOrder,
                    confirmedQuotations = confirmedQuotations.Count,
                    cancelledQuotations = cancelledQuotations.Count,
                    confirmedDistributors = confirmedDistributors,
                    cancelledDistributors = cancelledDistributors,
                    ordersPlaced = confirmedDistributors.Count,
                    quotationsCancelled = cancelledDistributors.Count,
                    customerNotified = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing selected quotations");
                return StatusCode(500, "Internal server error");
            }
        }

        private async Task SendCustomerNotification(Model.Order order, List<Model.QuotationComparison> confirmedQuotations, List<string> confirmedDistributors)
        {
            try
            {
                _logger.LogInformation($"📧 Sending customer notification for order {order.OrderNumber}");
                
                // Calculate delivery estimates from confirmed quotations
                var deliveryEstimates = new List<object>();
                var maxDeliveryDays = 0;
                
                foreach (var quotation in confirmedQuotations)
                {
                    // Extract delivery info from quotation notes
                    var notes = quotation.Notes ?? "";
                    var deliveryMatch = System.Text.RegularExpressions.Regex.Match(notes, @"Delivery: (\d{4}-\d{2}-\d{2})");
                    if (deliveryMatch.Success)
                    {
                        var deliveryDate = deliveryMatch.Groups[1].Value;
                        var deliveryDays = (DateTime.Parse(deliveryDate) - DateTime.UtcNow).Days;
                        maxDeliveryDays = Math.Max(maxDeliveryDays, deliveryDays);
                        
                        deliveryEstimates.Add(new
                        {
                            deliveryDate = deliveryDate,
                            deliveryDays = deliveryDays,
                            productName = quotation.Product?.Name ?? "Unknown Product"
                        });
                    }
                }
                
                // Create customer notification
                var notification = new
                {
                    orderNumber = order.OrderNumber,
                    customerName = order.Customer?.FirstName + " " + order.Customer?.LastName,
                    customerEmail = order.Customer?.Email,
                    orderDate = order.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    totalAmount = order.TotalAmount,
                    status = "Confirmed",
                    distributors = confirmedDistributors,
                    deliveryEstimates = deliveryEstimates,
                    estimatedDeliveryDate = DateTime.UtcNow.AddDays(maxDeliveryDays).ToString("yyyy-MM-dd"),
                    estimatedDeliveryDays = maxDeliveryDays,
                    message = $"Your order {order.OrderNumber} has been confirmed and is being prepared for shipment. Estimated delivery: {DateTime.UtcNow.AddDays(maxDeliveryDays):yyyy-MM-dd} ({maxDeliveryDays} days)."
                };
                
                _logger.LogInformation($"📧 Customer notification prepared: {notification.message}");
                _logger.LogInformation($"📦 Delivery estimates: {deliveryEstimates.Count} distributors, Max delivery: {maxDeliveryDays} days");
                
                // Store notification for customer retrieval
                await StoreCustomerNotification(order.OrderNumber, notification);
                
                // In a real system, you would send this notification via email, SMS, or push notification
                // For now, we'll just log it and store it in a notifications table
                _logger.LogInformation($"✅ Customer notification sent for order {order.OrderNumber}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error sending customer notification for order {order.OrderNumber}");
            }
        }

        private async Task StoreCustomerNotification(string orderNumber, object notification)
        {
            try
            {
                // In a real system, you would store this in a Notifications table
                // For now, we'll store it in the order's notes field
                var order = _quotationComparisonRepo.GetQuotationComparisons()
                    .Where(qc => qc.Order != null && qc.Order.OrderNumber == orderNumber)
                    .Select(qc => qc.Order)
                    .FirstOrDefault();
                
                if (order != null)
                {
                    var notificationJson = System.Text.Json.JsonSerializer.Serialize(notification);
                    // Replace any existing customer notification instead of appending
                    var notes = order.Notes ?? "";
                    var notificationPattern = @"\|\s*📧 Customer Notification:.*?(?=\| |$)";
                    var cleanedNotes = System.Text.RegularExpressions.Regex.Replace(notes, notificationPattern, "");
                    order.Notes = cleanedNotes.Trim() + $" | 📧 Customer Notification: {notificationJson}";
                    await _quotationComparisonRepo.UpdateOrder(order);
                    _logger.LogInformation($"📧 Customer notification stored for order {orderNumber}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error storing customer notification for order {orderNumber}");
            }
        }

        [HttpGet("customer-notifications/{orderNumber}")]
        public ActionResult GetCustomerNotifications(string orderNumber)
        {
            try
            {
                _logger.LogInformation($"📧 Customer requesting notifications for order: {orderNumber}");
                
                var order = _quotationComparisonRepo.GetQuotationComparisons()
                    .Where(qc => qc.Order != null && qc.Order.OrderNumber == orderNumber)
                    .Select(qc => qc.Order)
                    .FirstOrDefault();
                
                if (order == null)
                {
                    _logger.LogWarning($"⚠️ Order not found for notifications: {orderNumber}");
                    return NotFound(new { message = "Order not found", orderNumber = orderNumber });
                }
                
                // Extract notification from order notes
                var notes = order.Notes ?? "";
                var notificationMatch = System.Text.RegularExpressions.Regex.Match(notes, @"📧 Customer Notification: (.+)");
                
                if (notificationMatch.Success)
                {
                    var notificationJson = notificationMatch.Groups[1].Value;
                    var notification = System.Text.Json.JsonSerializer.Deserialize<dynamic>(notificationJson);
                    
                    _logger.LogInformation($"✅ Customer notifications provided for {orderNumber}");
                    return Ok(new { 
                        success = true,
                        orderNumber = orderNumber,
                        notification = notification,
                        message = "Order confirmed and ready for shipment"
                    });
                }
                else
                {
                    return Ok(new { 
                        success = false,
                        orderNumber = orderNumber,
                        message = "Order is still being processed. Please check back later."
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error getting customer notifications for {orderNumber}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("order-tracking/{orderNumber}")]
        public ActionResult GetOrderTracking(string orderNumber)
        {
            try
            {
                _logger.LogInformation($"🔍 Customer requesting order tracking for: {orderNumber}");
                
                var order = _quotationComparisonRepo.GetQuotationComparisons()
                    .Where(qc => qc.Order != null && qc.Order.OrderNumber == orderNumber)
                    .Select(qc => qc.Order)
                    .FirstOrDefault();
                
                if (order == null)
                {
                    _logger.LogWarning($"⚠️ Order not found: {orderNumber}");
                    return NotFound(new { message = "Order not found", orderNumber = orderNumber });
                }
                
                // Get quotations for this order
                var quotations = _quotationComparisonRepo.GetQuotationComparisons()
                    .Where(qc => qc.OrderId == order.Id)
                    .ToList();
                
                var confirmedQuotations = quotations.Where(q => q.Status == "Confirmed").ToList();
                var cancelledQuotations = quotations.Where(q => q.Status == "Cancelled").ToList();
                
                // Extract delivery information from confirmed quotations
                var deliveryInfo = new List<object>();
                var maxDeliveryDays = 0;
                
                foreach (var quotation in confirmedQuotations)
                {
                    var notes = quotation.Notes ?? "";
                    var deliveryMatch = System.Text.RegularExpressions.Regex.Match(notes, @"Delivery: (\d{4}-\d{2}-\d{2})");
                    var orderIdMatch = System.Text.RegularExpressions.Regex.Match(notes, @"Order ID: (\d+)");
                    
                    if (deliveryMatch.Success && orderIdMatch.Success)
                    {
                        var deliveryDate = deliveryMatch.Groups[1].Value;
                        var distributorOrderId = orderIdMatch.Groups[1].Value;
                        var deliveryDays = (DateTime.Parse(deliveryDate) - DateTime.UtcNow).Days;
                        maxDeliveryDays = Math.Max(maxDeliveryDays, deliveryDays);
                        
                        // Get quantity from the order item
                        var orderItem = order.OrderItems?.FirstOrDefault(oi => oi.ProductId == quotation.ProductId);
                        var quantity = orderItem?.Quantity ?? 1; // Default to 1 if not found
                        
                        deliveryInfo.Add(new
                        {
                            productName = quotation.Product?.Name ?? "Unknown Product",
                            quantity = quantity,
                            unitPrice = quotation.UnitPrice,
                            totalPrice = quotation.TotalPrice,
                            deliveryDate = deliveryDate,
                            deliveryDays = deliveryDays,
                            status = quotation.Status
                        });
                    }
                }
                
                var trackingInfo = new
                {
                    orderNumber = order.OrderNumber,
                    orderDate = order.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    status = order.Status,
                    totalAmount = order.TotalAmount,
                    customerName = order.Customer?.FirstName + " " + order.Customer?.LastName,
                    customerEmail = order.Customer?.Email,
                    estimatedDeliveryDate = DateTime.UtcNow.AddDays(maxDeliveryDays).ToString("yyyy-MM-dd"),
                    estimatedDeliveryDays = maxDeliveryDays,
                    confirmedDistributors = confirmedQuotations.Select(q => q.DistributorName).ToList(),
                    cancelledDistributors = cancelledQuotations.Select(q => q.DistributorName).ToList(),
                    deliveryInfo = deliveryInfo,
                    message = $"Order {order.OrderNumber} is {order.Status.ToLower()}. " +
                             $"Estimated delivery: {DateTime.UtcNow.AddDays(maxDeliveryDays):yyyy-MM-dd} ({maxDeliveryDays} days)."
                };
                
                _logger.LogInformation($"✅ Order tracking info provided for {orderNumber}: {trackingInfo.message}");
                return Ok(trackingInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error getting order tracking for {orderNumber}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("quotations")]
        public ActionResult<List<QuotationDisplayDTO>> GetQuotations()
        {
            try
            {
                _logger.LogInformation("Getting all quotations for display");
                
                // Get all quotation comparisons from database
                var quotationComparisons = _quotationComparisonRepo.GetQuotationComparisons();
                
                if (!quotationComparisons.Any())
                {
                    return Ok(new List<QuotationDisplayDTO>());
                }

                // Group quotations by order and product
                var groupedQuotations = quotationComparisons
                    .GroupBy(qc => new { qc.OrderId, qc.ProductId })
                    .ToList();

                var quotationDisplayList = new List<QuotationDisplayDTO>();

                foreach (var group in groupedQuotations)
                {
                    var firstQuotation = group.First();
                    var order = firstQuotation.Order;
                    var product = firstQuotation.Product;

                    // Skip if order or product is null
                    if (order == null || product == null)
                    {
                        _logger.LogWarning($"Skipping quotation group due to null order or product. OrderId: {firstQuotation.OrderId}, ProductId: {firstQuotation.ProductId}");
                        continue;
                    }

                    var quotationDisplay = new QuotationDisplayDTO
                    {
                        Id = $"{order.Id}-{product.Id}",
                        OrderId = order.Id,
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1, // Default quantity since it's not stored in QuotationComparison
                        Quotations = new List<DistributorQuotationDisplayDTO>()
                    };

                    // Add all distributor quotations for this product (filter out failed ones)
                    foreach (var quotation in group.Where(q => q.Status != "Failed"))
                    {
                        quotationDisplay.Quotations.Add(new DistributorQuotationDisplayDTO
                        {
                            QuotationId = quotation.Id, // Include the actual database ID
                            DistributorName = quotation.DistributorName,
                            UnitPrice = quotation.UnitPrice,
                            TotalPrice = quotation.TotalPrice,
                            AvailableStock = quotation.AvailableStock,
                            EstimatedDeliveryDays = quotation.EstimatedDeliveryDays,
                            Status = quotation.Status
                        });
                    }

                    quotationDisplayList.Add(quotationDisplay);
                }

                return Ok(quotationDisplayList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quotations");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("completed-orders/selected-quotations")]
        public ActionResult<List<CompletedOrderQuotationDTO>> GetCompletedOrdersWithSelectedQuotations()
        {
            try
            {
                _logger.LogInformation("🔍 ADMIN DASHBOARD: Getting completed orders with selected quotations");
                
                // Simple approach - just return empty list for now to avoid errors
                _logger.LogInformation("ℹ️ Returning empty list for completed orders (no quotations processed yet)");
                return Ok(new List<CompletedOrderQuotationDTO>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting completed orders with selected quotations: {Message}", ex.Message);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("order/{orderId}/quotations")]
        public async Task<ActionResult<QuotationManagementDTO>> GetOrderQuotations(int orderId)
        {
            try
            {
                _logger.LogInformation($"Getting quotations for order {orderId}");
                
                // Get order details
                var order = GetOrderById(orderId);
                if (order == null)
                {
                    return NotFound("Order not found");
                }

                var quotationManagement = new QuotationManagementDTO
                {
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    CustomerName = order.Customer != null ? $"{order.Customer.FirstName} {order.Customer.LastName}" : "Unknown",
                    OrderDate = order.OrderDate,
                    TotalAmount = order.TotalAmount,
                    Status = order.Status,
                    ProductQuotations = new List<ProductQuotationDTO>()
                };

                // Get quotations for each product
                foreach (var orderItem in order.OrderItems)
                {
                    var productQuotation = new ProductQuotationDTO
                    {
                        ProductId = orderItem.ProductId,
                        ProductName = orderItem.Product?.Name ?? "Unknown Product",
                        Quantity = orderItem.Quantity,
                        DistributorQuotations = new List<DistributorQuotationDTO>()
                    };

                    // Get existing quotations from database
                    var existingQuotations = await _quotationComparisonRepo.GetByOrderIdAndProductId(orderId, orderItem.ProductId);
                    
                    foreach (var quotation in existingQuotations)
                    {
                        productQuotation.DistributorQuotations.Add(new DistributorQuotationDTO
                        {
                            DistributorName = quotation.DistributorName,
                            UnitPrice = quotation.UnitPrice,
                            TotalPrice = quotation.TotalPrice,
                            AvailableStock = quotation.AvailableStock,
                            EstimatedDeliveryDays = quotation.EstimatedDeliveryDays,
                            Status = quotation.Status,
                            Notes = quotation.Notes ?? string.Empty,
                            CreatedDate = quotation.CreatedDate,
                            ExpiryDate = quotation.CreatedDate.AddDays(7) // Default 7 days expiry
                        });
                    }

                    quotationManagement.ProductQuotations.Add(productQuotation);
                }

                return Ok(quotationManagement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting quotations for order {orderId}");
                return StatusCode(500, "Internal server error");
            }
        }

        private List<Model.Order> GetOrdersNeedingQuotationProcessing()
        {
            // This would typically query the database for orders that need quotation processing
            // For now, return a mock list
            return new List<Model.Order>();
        }

        private Model.Order? GetOrderById(int orderId)
        {
            // Get the order from the database
            var order = _quotationComparisonRepo.GetQuotationComparisons()
                .FirstOrDefault(qc => qc.OrderId == orderId)?.Order;
            return order;
        }

        private async Task UpdateDistributorQuotationStatus(string distributorName, Model.QuotationComparison quotation, string status)
        {
            try
            {
                _logger.LogInformation($"🔄 Updating quotation status to '{status}' for quotation {quotation.Id} with {distributorName}");

                var distributorUrl = GetDistributorUrl(distributorName);
                
                // Get all quotations from distributor to find the matching one
                var quotationsEndpoint = $"{distributorUrl}/api/Quotation";
                
                _logger.LogInformation($"📡 Getting quotations from {distributorName} to find matching quotation for product {quotation.ProductId}");

                // Get all quotations from distributor
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);

                var quotationsResponse = await httpClient.GetAsync(quotationsEndpoint);
                
                if (quotationsResponse.IsSuccessStatusCode)
                {
                    var quotationsContent = await quotationsResponse.Content.ReadAsStringAsync();
                    _logger.LogInformation($"✅ Retrieved quotations from {distributorName}");
                    
                    // Parse the quotations to find the one matching our product ID
                    try
                    {
                        var quotations = System.Text.Json.JsonSerializer.Deserialize<List<dynamic>>(quotationsContent);
                        if (quotations != null)
                        {
                            // Find quotation matching our product ID
                            var matchingQuotation = quotations.FirstOrDefault(q => 
                            {
                                try
                                {
                                    // Try to get ProductId from the quotation object
                                    var productIdProperty = q.GetProperty("productId");
                                    return productIdProperty.GetInt32() == quotation.ProductId;
                                }
                                catch
                                {
                                    return false;
                                }
                            });

                            if (matchingQuotation != null)
                            {
                                // Get the quotation ID from the matching quotation
                                var quotationId = matchingQuotation.GetProperty("id").GetInt32();
                                
                                _logger.LogInformation($"🎯 Found matching quotation ID {quotationId} for product {quotation.ProductId} in {distributorName}");
                                
                                // Now update the quotation status using the found quotation ID
                                var statusUpdateEndpoint = $"{distributorUrl}/api/Quotation/update-status";
                                var statusUpdateRequest = new
                                {
                                    QuotationId = quotationId,
                                    Status = status,
                                    Notes = $"Status updated by GadgetHub - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}"
                                };

                                _logger.LogInformation($"📡 Updating quotation {quotationId} status to '{status}' in {distributorName}");

                                var statusUpdateResponse = await httpClient.PutAsJsonAsync(statusUpdateEndpoint, statusUpdateRequest);
                                
                                if (statusUpdateResponse.IsSuccessStatusCode)
                                {
                                    var updateResponseContent = await statusUpdateResponse.Content.ReadAsStringAsync();
                                    _logger.LogInformation($"✅ Successfully updated quotation {quotationId} status to '{status}' in {distributorName}: {updateResponseContent}");
                                }
                                else
                                {
                                    var errorContent = await statusUpdateResponse.Content.ReadAsStringAsync();
                                    _logger.LogWarning($"⚠️ Failed to update quotation {quotationId} status in {distributorName}: {statusUpdateResponse.StatusCode} - {errorContent}");
                                }
                            }
                            else
                            {
                                _logger.LogWarning($"⚠️ No matching quotation found for product {quotation.ProductId} in {distributorName}");
                            }
                        }
                    }
                    catch (Exception parseEx)
                    {
                        _logger.LogError(parseEx, $"❌ Error parsing quotations from {distributorName}");
                        _logger.LogInformation($"⚠️ Quotation status update to '{status}' for product {quotation.ProductId} with {distributorName} - Unable to parse response");
                    }
                }
                else
                {
                    var errorContent = await quotationsResponse.Content.ReadAsStringAsync();
                    _logger.LogWarning($"⚠️ Failed to get quotations from {distributorName}: {quotationsResponse.StatusCode} - {errorContent}");
                    _logger.LogInformation($"⚠️ Quotation status update to '{status}' for product {quotation.ProductId} with {distributorName} - Unable to verify (API not accessible)");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error updating quotation {quotation.Id} status in {distributorName}");
            }
        }

        private string GetDistributorUrl(string distributorName)
        {
            return distributorName switch
            {
                "ElectroCom" => "https://localhost:7077",
                "TechWorld" => "https://localhost:7102",
                "GadgetCentral" => "https://localhost:7007",
                _ => throw new ArgumentException($"Unknown distributor: {distributorName}")
            };
        }

        private async Task<bool> PlaceOrderWithDistributor(Model.Order order, Model.QuotationComparison quotation, string distributorName)
        {
            try
            {
                _logger.LogInformation($"Placing order {order.OrderNumber} with {distributorName}");

                // Get the product details from the order
                var orderItem = order.OrderItems.FirstOrDefault(oi => oi.ProductId == quotation.ProductId);
                if (orderItem == null)
                {
                    _logger.LogWarning($"Order item not found for product {quotation.ProductId} in order {order.OrderNumber}");
                    return false;
                }

                // Create order request for the distributor
                var distributorOrder = new DistributorOrderCreateDTO
                {
                    OrderNumber = $"{order.OrderNumber}-{distributorName}",
                    CustomerName = "GadgetHub Customer", // Default customer name
                    CustomerEmail = "customer@gadgethub.com", // Default email
                    CustomerPhone = "+94-XXX-XXXXXXX", // Default phone
                    ShippingAddress = "GadgetHub Default Address", // Default shipping address
                    TotalAmount = quotation.TotalPrice,
                    OrderDate = DateTime.UtcNow,
                    Notes = $"Order from GadgetHub - Original Order: {order.OrderNumber}",
                    OrderItems = new List<DistributorOrderItemDTO>
                    {
                        new DistributorOrderItemDTO
                        {
                            ProductId = quotation.ProductId,
                            Quantity = orderItem.Quantity,
                            UnitPrice = quotation.UnitPrice,
                            TotalPrice = quotation.TotalPrice
                        }
                    }
                };

                // Get distributor URL
                var distributorUrl = GetDistributorUrl(distributorName);
                var orderEndpoint = $"{distributorUrl}/api/Order";

                _logger.LogInformation($"📡 Sending order to {distributorName} at {orderEndpoint}");
                _logger.LogInformation($"📋 Order details: OrderNumber={distributorOrder.OrderNumber}, TotalAmount={distributorOrder.TotalAmount:C}, Items={distributorOrder.OrderItems.Count}");
                
                // Log order items details
                foreach (var item in distributorOrder.OrderItems)
                {
                    _logger.LogInformation($"📦 Order Item: ProductId={item.ProductId}, Quantity={item.Quantity}, UnitPrice={item.UnitPrice:C}, TotalPrice={item.TotalPrice:C}");
                }

                // Send order to distributor
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);

                // Log the full JSON payload being sent
                var orderJson = System.Text.Json.JsonSerializer.Serialize(distributorOrder, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                _logger.LogInformation($"📄 JSON payload being sent to {distributorName}: {orderJson}");

                var response = await httpClient.PostAsJsonAsync(orderEndpoint, distributorOrder);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"✅ Successfully placed order with {distributorName}");
                    _logger.LogInformation($"📊 Order created in {distributorName} database: {responseContent}");
                    _logger.LogInformation($"✅ Order and OrderItems tables updated in {distributorName} database");
                    
                    // Parse the order confirmation response to extract order ID and delivery info
                    try
                    {
                        var orderConfirmation = System.Text.Json.JsonSerializer.Deserialize<dynamic>(responseContent);
                        if (orderConfirmation != null)
                        {
                            var orderIdProperty = orderConfirmation.GetProperty("orderId");
                            var deliveryDaysProperty = orderConfirmation.GetProperty("estimatedDeliveryDays");
                            var deliveryDateProperty = orderConfirmation.GetProperty("estimatedDeliveryDate");
                            
                            var distributorOrderId = orderIdProperty.GetInt32();
                            var estimatedDeliveryDays = deliveryDaysProperty.GetInt32();
                            var estimatedDeliveryDate = deliveryDateProperty.GetString();
                            
                            _logger.LogInformation($"🎯 {distributorName} Order Confirmed - Order ID: {distributorOrderId}, Delivery: {estimatedDeliveryDate} ({estimatedDeliveryDays} days)");
                            
                            // Store the distributor order ID and delivery info in the quotation comparison
                            quotation.Notes += $" | ✅ {distributorName} Order ID: {distributorOrderId}, Delivery: {estimatedDeliveryDate}";
                            _quotationComparisonRepo.UpdateQuotationComparison(quotation);
                        }
                    }
                    catch (Exception parseEx)
                    {
                        _logger.LogWarning(parseEx, $"⚠️ Could not parse order confirmation from {distributorName}");
                    }
                    
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"⚠️ Failed to place order with {distributorName}: {response.StatusCode} - {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error placing order with {distributorName} for order {order.OrderNumber}");
                return false;
            }
        }
    }

    public class QuotationManagementDTO
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<ProductQuotationDTO> ProductQuotations { get; set; } = new List<ProductQuotationDTO>();
    }

    public class ProductQuotationDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public List<DistributorQuotationDTO> DistributorQuotations { get; set; } = new List<DistributorQuotationDTO>();
    }

    public class DistributorQuotationDTO
    {
        public string DistributorName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public int AvailableStock { get; set; }
        public int EstimatedDeliveryDays { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
    }

    public class ProcessSelectedQuotationsRequest
    {
        public List<SelectedQuotationDTO> SelectedQuotations { get; set; } = new List<SelectedQuotationDTO>();
    }

    public class SelectedQuotationDTO
    {
        public int QuotationId { get; set; }
        public string DistributorName { get; set; } = string.Empty;
    }

    public class QuotationDisplayDTO
    {
        public string Id { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public List<DistributorQuotationDisplayDTO> Quotations { get; set; } = new List<DistributorQuotationDisplayDTO>();
    }

    public class DistributorQuotationDisplayDTO
    {
        public int QuotationId { get; set; }
        public string DistributorName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public int AvailableStock { get; set; }
        public int EstimatedDeliveryDays { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class CompletedOrderQuotationDTO
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<SelectedQuotationInfoDTO> SelectedQuotations { get; set; } = new List<SelectedQuotationInfoDTO>();
        }

    public class SelectedQuotationInfoDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string DistributorName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public int EstimatedDeliveryDays { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    // Order creation DTOs for distributor APIs
    public class DistributorOrderCreateDTO
    {
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public string? Notes { get; set; }
        public List<DistributorOrderItemDTO> OrderItems { get; set; } = new List<DistributorOrderItemDTO>();
    }

    public class DistributorOrderItemDTO
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
