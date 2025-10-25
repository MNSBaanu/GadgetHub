using GadgetHubAPI.Model;
using GadgetHubAPI.DTO;

namespace GadgetHubAPI.Services
{
    public class NotificationService
    {
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger;
        }

        public async Task SendOrderConfirmationAsync(Order order, List<QuotationComparison> quotations)
        {
            try
            {
                var customerEmail = order.Customer?.Email;
                if (string.IsNullOrEmpty(customerEmail))
                {
                    _logger.LogWarning($"No email address found for customer {order.CustomerId}");
                    return;
                }

                var distributors = quotations.Select(q => q.DistributorName).Distinct().ToList();
                var totalAmount = quotations.Sum(q => q.TotalPrice);
                var estimatedDelivery = quotations.Max(q => q.EstimatedDeliveryDays);

                var emailContent = GenerateOrderConfirmationEmail(order, quotations, distributors, totalAmount, estimatedDelivery);
                
                _logger.LogInformation($"ORDER CONFIRMATION EMAIL TO: {customerEmail}");
                _logger.LogInformation($"Subject: Order Confirmation - {order.OrderNumber}");
                _logger.LogInformation($"Content: {emailContent}");
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending order confirmation for order {order.OrderNumber}");
            }
        }


        private string GenerateOrderConfirmationEmail(Order order, List<QuotationComparison> quotations, List<string> distributors, decimal totalAmount, int estimatedDeliveryDays)
        {
            var customerName = $"{order.Customer?.FirstName} {order.Customer?.LastName}";
            var orderDate = order.OrderDate.ToString("MMMM dd, yyyy");
            var estimatedDeliveryDate = order.OrderDate.AddDays(estimatedDeliveryDays).ToString("MMMM dd, yyyy");

            return $@"
Dear {customerName},

Thank you for your order with GadgetHub! We're excited to confirm that your order has been processed and is being fulfilled by our trusted distributors.

ORDER DETAILS:
Order Number: {order.OrderNumber}
Order Date: {orderDate}
Total Amount: ${totalAmount:F2}

DISTRIBUTORS:
{string.Join("\n", distributors.Select(d => $"• {d}"))}

ORDER ITEMS:
{string.Join("\n", quotations.Select(q => $"• Product ID {q.ProductId} - ${q.UnitPrice:F2} each (from {q.DistributorName})"))}

DELIVERY INFORMATION:
Estimated Delivery: {estimatedDeliveryDate} ({estimatedDeliveryDays} business days)

Your order is now being processed by our distributors. You'll receive updates as your items are prepared for shipment.

Thank you for choosing GadgetHub!

Best regards,
The GadgetHub Team

---
This is an automated message. Please do not reply to this email.
";
        }

    }
}

