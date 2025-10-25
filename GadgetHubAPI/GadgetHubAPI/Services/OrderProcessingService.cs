using GadgetHubAPI.DTO;
using GadgetHubAPI.Data;
using AutoMapper;

namespace GadgetHubAPI.Services
{
    public class OrderProcessingService
    {
        private readonly HttpClient _httpClient;
        private readonly OrderRepo _orderRepo;
        private readonly QuotationComparisonRepo _quotationComparisonRepo;
        private readonly NotificationService _notificationService;
        private readonly DistributorService _distributorService;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderProcessingService> _logger;

        public OrderProcessingService(
            HttpClient httpClient,
            OrderRepo orderRepo,
            QuotationComparisonRepo quotationComparisonRepo,
            NotificationService notificationService,
            DistributorService distributorService,
            IMapper mapper,
            ILogger<OrderProcessingService> logger)
        {
            _httpClient = httpClient;
            _orderRepo = orderRepo;
            _quotationComparisonRepo = quotationComparisonRepo;
            _notificationService = notificationService;
            _distributorService = distributorService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<OrderReadDTO> ProcessOrderWithDistributors(OrderCreateDTO orderRequest)
        {
            try
            {
                _logger.LogInformation($"Starting order processing for customer {orderRequest.CustomerId} with {orderRequest.OrderItems.Count} items");

                // Create the order in GadgetHub database
                var order = _mapper.Map<Model.Order>(orderRequest);
                order.OrderNumber = GenerateOrderNumber();
                order.OrderDate = DateTime.UtcNow;
                order.Status = "Processing";

                var success = _orderRepo.AddOrder(order);
                if (!success)
                {
                    throw new Exception("Failed to create order in database");
                }

                _logger.LogInformation($"Order {order.OrderNumber} created successfully in database");

                // Step 1: Request quotations from all distributors
                var quotationRequests = orderRequest.OrderItems.Select(item => new DTO.QuotationRequestDTO
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Notes = $"Order {order.OrderNumber}"
                }).ToList();

                _logger.LogInformation($"Requesting quotations for {quotationRequests.Count} products from distributors");

                var quotations = await _distributorService.GetQuotationsFromAllDistributors(quotationRequests);

                _logger.LogInformation($"Received {quotations.Count} quotations from distributors");

                // If no quotations received, still create the order but mark it as pending
                if (!quotations.Any())
                {
                    _logger.LogWarning($"No quotations received from distributors for order {order.OrderNumber}");
                    order.Status = "Pending Quotations";
                    order.Notes = "Order created but no quotations received from distributors. Manual processing required.";
                    
                    // Set default prices and dates even without quotations
                    SetDefaultPricesAndDates(order);
                    
                    _orderRepo.UpdateOrder(order);
                    return _mapper.Map<OrderReadDTO>(order);
                }

                // Step 2: Save quotations to database
                await _quotationComparisonRepo.SaveQuotationComparisons(quotations, order.Id);
                _logger.LogInformation($"Saved {quotations.Count} quotations to database");

                // Step 3: Compare quotations and select best ones
                var bestQuotations = SelectBestQuotations(quotations, order.Id);
                _logger.LogInformation($"Selected {bestQuotations.Count} best quotations");

                // Step 3.5: Update OrderItems with final prices
                UpdateOrderItemsWithPrices(order.Id, bestQuotations);
                _logger.LogInformation($"Updated order items with final prices");

                if (!bestQuotations.Any())
                {
                    _logger.LogWarning($"No best quotations selected for order {order.OrderNumber}");
                    order.Status = "Pending Selection";
                    order.Notes = "Order created but no suitable quotations selected. Manual processing required.";
                    _orderRepo.UpdateOrder(order);
                    return _mapper.Map<OrderReadDTO>(order);
                }

                // Step 4: Group quotations by distributor
                var distributorOrders = bestQuotations
                    .GroupBy(qc => qc.DistributorName)
                    .ToDictionary(g => g.Key, g => g.ToList());

                _logger.LogInformation($"Grouped quotations by {distributorOrders.Count} distributors");

                // Step 5: Place orders with each distributor
                var successfulOrders = 0;
                foreach (var distributorOrder in distributorOrders)
                {
                    try
                    {
                        await PlaceOrderWithDistributor(distributorOrder.Key, distributorOrder.Value, order);
                        successfulOrders++;
                        _logger.LogInformation($"Successfully placed order with {distributorOrder.Key}");
                    }
                    catch (Exception distributorEx)
                    {
                        _logger.LogError(distributorEx, $"Failed to place order with {distributorOrder.Key}");
                    }
                }

                // Update order status based on results
                if (successfulOrders > 0)
                {
                    order.Status = "Confirmed";
                    order.Notes = $"Order confirmed with {successfulOrders} distributor(s)";
                }
                else
                {
                    order.Status = "Pending Confirmation";
                    order.Notes = "Order created but failed to confirm with distributors. Manual processing required.";
                }

                _orderRepo.UpdateOrder(order);

                // Send order confirmation notification to customer
                try
                {
                    await _notificationService.SendOrderConfirmationAsync(order, bestQuotations);
                    _logger.LogInformation($"Order confirmation notification sent for order {order.OrderNumber}");
                }
                catch (Exception notificationEx)
                {
                    _logger.LogError(notificationEx, $"Failed to send order confirmation notification for order {order.OrderNumber}");
                }

                _logger.LogInformation($"Order processing completed for order {order.OrderNumber} with status: {order.Status}");
                return _mapper.Map<OrderReadDTO>(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing order with distributors");
                throw;
            }
        }

        private List<Model.QuotationComparison> SelectBestQuotations(List<DTO.QuotationResponseDTO> quotations, int orderId)
        {
            var bestQuotations = new List<Model.QuotationComparison>();

            if (!quotations.Any())
            {
                _logger.LogWarning($"No quotations received for order {orderId}");
                return bestQuotations;
            }

            // Group quotations by product
            var productGroups = quotations.GroupBy(q => q.ProductId);

            _logger.LogInformation($"Processing quotations for {productGroups.Count()} products in order {orderId}");

            foreach (var productGroup in productGroups)
            {
                var productQuotations = productGroup.ToList();
                var productId = productGroup.Key;
                
                _logger.LogInformation($"Evaluating {productQuotations.Count} quotations for product {productId}");

                // Filter out quotations with insufficient stock
                var availableQuotations = productQuotations.Where(q => q.AvailableStock > 0).ToList();
                
                if (!availableQuotations.Any())
                {
                    _logger.LogWarning($"No quotations with sufficient stock for product {productId}");
                    // Select the quotation with the highest stock anyway (even if 0)
                    availableQuotations = productQuotations;
                }
                
                // Apply markup pricing (20% markup) and calculate scores with relative comparison
                var prices = availableQuotations.Select(q => q.UnitPrice).ToList();
                var stocks = availableQuotations.Select(q => q.AvailableStock).ToList();
                var minPrice = prices.Min();
                var maxPrice = prices.Max();
                var minStock = stocks.Min();
                var maxStock = stocks.Max();
                
                var markedUpQuotations = availableQuotations.Select(q => new
                {
                    Quotation = q,
                    MarkedUpPrice = q.UnitPrice * 1.20m, // 20% markup
                    Score = CalculateQuotationScoreRelative(q.UnitPrice, q.AvailableStock, q.EstimatedDeliveryDays, minPrice, maxPrice, minStock, maxStock),
                    OriginalPrice = q.UnitPrice
                }).ToList();

                // Select the best quotation based on score
                var bestQuotation = markedUpQuotations.OrderByDescending(q => q.Score).First();
                
                _logger.LogInformation($"Selected {bestQuotation.Quotation.DistributorName} for product {productId} " +
                    $"(Score: {bestQuotation.Score}, Price: {bestQuotation.OriginalPrice:C}, " +
                    $"Stock: {bestQuotation.Quotation.AvailableStock}, Delivery: {bestQuotation.Quotation.EstimatedDeliveryDays} days)");

                // Create quotation comparison record
                var quotationComparison = new Model.QuotationComparison
                {
                    OrderId = orderId,
                    ProductId = bestQuotation.Quotation.ProductId,
                    DistributorName = bestQuotation.Quotation.DistributorName,
                    UnitPrice = bestQuotation.MarkedUpPrice, // Use marked up price
                    AvailableStock = bestQuotation.Quotation.AvailableStock,
                    EstimatedDeliveryDays = bestQuotation.Quotation.EstimatedDeliveryDays,
                    TotalPrice = bestQuotation.MarkedUpPrice * bestQuotation.Quotation.Quantity,
                    Status = bestQuotation.Quotation.AvailableStock > 0 ? "Selected" : "Selected - Backorder",
                    CreatedDate = DateTime.UtcNow,
                    Notes = $"Best quotation selected (Score: {bestQuotation.Score}) - " +
                           $"Original Price: {bestQuotation.OriginalPrice:C}, " +
                           $"Markup: 20%, Stock: {bestQuotation.Quotation.AvailableStock}, " +
                           $"Delivery: {bestQuotation.Quotation.EstimatedDeliveryDays} days"
                };

                bestQuotations.Add(quotationComparison);
            }

            // Save selected quotations to database
            foreach (var quotation in bestQuotations)
            {
                _quotationComparisonRepo.AddQuotationComparison(quotation);
            }

            _logger.LogInformation($"Selected {bestQuotations.Count} quotations for order {orderId}");
            return bestQuotations;
        }

        private decimal CalculateQuotationScore(decimal price, int stock, int deliveryDays)
        {
            // Simple scoring algorithm: Only compare price and stock (like products page)
            // Price weight: 60%, Stock weight: 40%
            
            // Price score (0-100): Lower prices get higher scores
            // Use relative scoring - this will be calculated based on all quotations for the same product
            var priceScore = 50m; // Default score, will be adjusted in calling method
            
            // Stock score (0-100): More stock gets higher scores
            // Simple linear scoring for stock
            var stockScore = Math.Min(100m, stock * 5m); // Each unit of stock = 5 points, max 100
            
            // Simple weighted score: Price 60%, Stock 40%
            var weightedScore = (priceScore * 0.6m) + (stockScore * 0.4m);
            
            return Math.Round(weightedScore, 2);
        }

        private decimal CalculateQuotationScoreRelative(decimal price, int stock, int deliveryDays, decimal minPrice, decimal maxPrice, int minStock, int maxStock)
        {
            // Simple scoring algorithm: Only compare price and stock (like products page)
            // Price weight: 60%, Stock weight: 40%
            
            // Price score (0-100): Lower prices get higher scores (relative to other quotations)
            var priceScore = maxPrice == minPrice ? 100m : Math.Max(0m, ((maxPrice - price) / (maxPrice - minPrice)) * 100m);
            
            // Stock score (0-100): More stock gets higher scores (relative to other quotations)
            var stockScore = maxStock == minStock ? 100m : Math.Max(0m, ((stock - minStock) / (maxStock - minStock)) * 100m);
            
            // Simple weighted score: Price 60%, Stock 40%
            var weightedScore = (priceScore * 0.6m) + (stockScore * 0.4m);
            
            return Math.Round(weightedScore, 2);
        }

        private void UpdateOrderItemsWithPrices(int orderId, List<Model.QuotationComparison> bestQuotations)
        {
            try
            {
                // Get the order with its items
                var order = _orderRepo.GetOrderById(orderId);
                if (order == null)
                {
                    _logger.LogError($"Order {orderId} not found when updating prices");
                    return;
                }

                _logger.LogInformation($"Updating prices for order {orderId} with {order.OrderItems.Count} items and {bestQuotations.Count} quotations");

                // Update each order item with the corresponding quotation price
                foreach (var orderItem in order.OrderItems)
                {
                    var quotation = bestQuotations.FirstOrDefault(q => q.ProductId == orderItem.ProductId);
                    if (quotation != null)
                    {
                        var oldUnitPrice = orderItem.UnitPrice;
                        var oldTotalPrice = orderItem.TotalPrice;
                        
                        orderItem.UnitPrice = quotation.UnitPrice;
                        orderItem.TotalPrice = quotation.UnitPrice * orderItem.Quantity;
                        
                        _logger.LogInformation($"Updated order item {orderItem.Id} for product {orderItem.ProductId}: " +
                            $"UnitPrice: {oldUnitPrice:C} → {orderItem.UnitPrice:C}, " +
                            $"TotalPrice: {oldTotalPrice:C} → {orderItem.TotalPrice:C}");
                    }
                    else
                    {
                        _logger.LogWarning($"No quotation found for product {orderItem.ProductId} in order {orderId}. " +
                            $"Available quotations for products: {string.Join(", ", bestQuotations.Select(q => q.ProductId))}");
                    }
                }

                // Update the order total
                var oldTotalAmount = order.TotalAmount;
                order.TotalAmount = order.OrderItems.Sum(oi => oi.TotalPrice);
                
                _logger.LogInformation($"Updated order {orderId} total amount: {oldTotalAmount:C} → {order.TotalAmount:C}");
                
                // Set realistic shipping and delivery dates based on quotation estimates
                SetOrderDates(order, bestQuotations);
                
                // Save the updated order
                _orderRepo.UpdateOrder(order);
                _logger.LogInformation($"Successfully saved updated order {orderId} with prices and dates");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating order items with prices for order {orderId}");
                throw;
            }
        }

        private void SetOrderDates(Model.Order order, List<Model.QuotationComparison> bestQuotations)
        {
            try
            {
                if (!bestQuotations.Any())
                {
                    _logger.LogWarning($"No quotations available to set dates for order {order.Id}");
                    // Set default dates
                    order.ShippedDate = DateTime.UtcNow.AddDays(3);
                    order.DeliveredDate = DateTime.UtcNow.AddDays(7);
                    return;
                }

                // Calculate the maximum delivery days from all quotations
                var maxDeliveryDays = bestQuotations.Max(q => q.EstimatedDeliveryDays);
                var avgDeliveryDays = (int)Math.Ceiling(bestQuotations.Average(q => q.EstimatedDeliveryDays));
                
                _logger.LogInformation($"Order {order.Id} delivery estimates - Max: {maxDeliveryDays} days, Avg: {avgDeliveryDays} days");

                // Set shipping date (typically 1-2 days after order confirmation)
                var shippingDelay = 2; // Business days to prepare and ship
                order.ShippedDate = DateTime.UtcNow.AddDays(shippingDelay);
                
                // Set delivery date based on average delivery time + shipping delay
                var totalDeliveryDays = shippingDelay + avgDeliveryDays;
                order.DeliveredDate = DateTime.UtcNow.AddDays(totalDeliveryDays);
                
                _logger.LogInformation($"Set dates for order {order.Id}: " +
                    $"ShippedDate: {order.ShippedDate:yyyy-MM-dd}, " +
                    $"DeliveredDate: {order.DeliveredDate:yyyy-MM-dd} " +
                    $"(Total: {totalDeliveryDays} days)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting dates for order {order.Id}");
                // Set default dates as fallback
                order.ShippedDate = DateTime.UtcNow.AddDays(3);
                order.DeliveredDate = DateTime.UtcNow.AddDays(7);
            }
        }

        private async Task PlaceOrderWithDistributor(string distributorName, List<Model.QuotationComparison> quotations, Model.Order order)
        {
            try
            {
                _logger.LogInformation($"Placing order with {distributorName} for {quotations.Count} items");

                var distributorOrder = new DistributorOrderDTO
                {
                    OrderNumber = order.OrderNumber,
                    CustomerName = order.Customer != null ? $"{order.Customer.FirstName} {order.Customer.LastName}" : "",
                    CustomerEmail = order.Customer?.Email ?? "",
                    CustomerPhone = order.Customer?.Phone ?? "",
                    ShippingAddress = order.Customer?.Address ?? "",
                    TotalAmount = quotations.Sum(q => q.TotalPrice),
                    OrderDate = order.OrderDate,
                    Notes = $"Order from GadgetHub - {order.OrderNumber}",
                    OrderItems = quotations.Select(q => {
                        // Find the corresponding order item to get the correct quantity
                        var orderItem = order.OrderItems.FirstOrDefault(oi => oi.ProductId == q.ProductId);
                        var quantity = orderItem?.Quantity ?? 1;
                        
                        return new DistributorOrderItemDTO
                        {
                            ProductId = q.ProductId,
                            Quantity = quantity,
                            UnitPrice = q.UnitPrice,
                            TotalPrice = q.UnitPrice * quantity
                        };
                    }).ToList()
                };

                var distributorUrl = GetDistributorUrl(distributorName);
                var response = await _httpClient.PostAsJsonAsync($"{distributorUrl}/api/Order", distributorOrder);

                if (response.IsSuccessStatusCode)
                {
                    var confirmationResponse = await response.Content.ReadFromJsonAsync<dynamic>();
                    var distributorOrderId = confirmationResponse?.id?.ToString() ?? "Unknown";
                    var distributorOrderNumber = confirmationResponse?.orderNumber?.ToString() ?? order.OrderNumber;
                    
                    _logger.LogInformation($"Order placed successfully with {distributorName}. " +
                        $"Distributor Order ID: {distributorOrderId}, Order Number: {distributorOrderNumber}");

                    // Update quotation comparisons with distributor order confirmation
                    foreach (var quotation in quotations)
                    {
                        quotation.Status = "Confirmed";
                        quotation.Notes += $" | Confirmed by {distributorName} (Order ID: {distributorOrderId})";
                        _quotationComparisonRepo.UpdateQuotationComparison(quotation);
                    }

                    // CRITICAL FIX: Update quotation status in distributor database
                    await UpdateDistributorQuotationStatus(distributorName, quotations, "Confirmed");

                    // Update order status if this was the last distributor
                    var allQuotations = await _quotationComparisonRepo.GetByOrderId(order.Id);
                    var allConfirmed = allQuotations.All(q => q.Status == "Confirmed");
                    
                    if (allConfirmed)
                    {
                        order.Status = "Confirmed";
                        order.Notes = $"All orders confirmed with distributors. " +
                                    $"Distributors: {string.Join(", ", allQuotations.Select(q => q.DistributorName).Distinct())}";
                        _orderRepo.UpdateOrder(order);
                        
                        _logger.LogInformation($"Order {order.OrderNumber} fully confirmed with all distributors");
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Failed to place order with {distributorName}: {response.StatusCode} - {errorContent}");
                    
                    // Update quotation comparisons with failure status
                    foreach (var quotation in quotations)
                    {
                        quotation.Status = "Failed";
                        quotation.Notes += $" | Failed to place order with {distributorName}: {response.StatusCode}";
                        _quotationComparisonRepo.UpdateQuotationComparison(quotation);
                    }

                    // CRITICAL FIX: Update quotation status in distributor database
                    await UpdateDistributorQuotationStatus(distributorName, quotations, "Failed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error placing order with {distributorName}");
                
                // Update quotation comparisons with error status
                foreach (var quotation in quotations)
                {
                    quotation.Status = "Error";
                    quotation.Notes += $" | Error placing order with {distributorName}: {ex.Message}";
                    _quotationComparisonRepo.UpdateQuotationComparison(quotation);
                }

                // CRITICAL FIX: Update quotation status in distributor database
                await UpdateDistributorQuotationStatus(distributorName, quotations, "Error");
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

        private void SetDefaultPricesAndDates(Model.Order order)
        {
            try
            {
                _logger.LogInformation($"Setting default prices and dates for order {order.Id} without quotations");
                
                // Set default prices for order items (you might want to get these from a product catalog)
                foreach (var orderItem in order.OrderItems)
                {
                    // Set a default unit price (you can modify this logic based on your product catalog)
                    var defaultUnitPrice = 100.00m; // Default price - you might want to fetch from Product table
                    orderItem.UnitPrice = defaultUnitPrice;
                    orderItem.TotalPrice = defaultUnitPrice * orderItem.Quantity;
                    
                    _logger.LogInformation($"Set default price for order item {orderItem.Id}: {defaultUnitPrice:C}");
                }
                
                // Update order total
                order.TotalAmount = order.OrderItems.Sum(oi => oi.TotalPrice);
                
                // Set default shipping and delivery dates
                order.ShippedDate = DateTime.UtcNow.AddDays(3); // 3 days to ship
                order.DeliveredDate = DateTime.UtcNow.AddDays(7); // 7 days total delivery
                
                _logger.LogInformation($"Set default dates for order {order.Id}: " +
                    $"ShippedDate: {order.ShippedDate:yyyy-MM-dd}, " +
                    $"DeliveredDate: {order.DeliveredDate:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting default prices and dates for order {order.Id}");
                // Don't throw - this is a fallback method
            }
        }

        private string GenerateOrderNumber()
        {
            return $"GH-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }

        private async Task UpdateDistributorQuotationStatus(string distributorName, List<Model.QuotationComparison> quotations, string status)
        {
            try
            {
                _logger.LogInformation($"Updating quotation status to '{status}' for {quotations.Count} quotations with {distributorName}");

                var distributorUrl = GetDistributorUrl(distributorName);
                
                foreach (var quotation in quotations)
                {
                    try
                    {
                        // Update quotation status in distributor database
                        var updateRequest = new
                        {
                            QuotationId = quotation.Id,
                            Status = status,
                            Notes = $"Status updated by GadgetHub - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}"
                        };

                        var response = await _httpClient.PutAsJsonAsync($"{distributorUrl}/api/Quotation/update-status", updateRequest);
                        
                        if (response.IsSuccessStatusCode)
                        {
                            _logger.LogInformation($"Successfully updated quotation {quotation.Id} status to '{status}' in {distributorName}");
                        }
                        else
                        {
                            _logger.LogWarning($"Failed to update quotation {quotation.Id} status in {distributorName}: {response.StatusCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error updating quotation {quotation.Id} status in {distributorName}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating distributor quotation status for {distributorName}");
            }
        }
    }

}
