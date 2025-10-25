using Microsoft.AspNetCore.Mvc;
using GadgetHubAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace GadgetHubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseController : ControllerBase
    {
        private readonly GadgetHubDBContext _context;
        private readonly ILogger<DatabaseController> _logger;

        public DatabaseController(GadgetHubDBContext context, ILogger<DatabaseController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetDatabaseStatus()
        {
            try
            {
                var status = new
                {
                    CanConnect = await _context.Database.CanConnectAsync(),
                    ProductsCount = await _context.Products.CountAsync(),
                    CustomersCount = await _context.Customers.CountAsync(),
                    OrdersCount = await _context.Orders.CountAsync(),
                    OrderItemsCount = await _context.OrderItems.CountAsync(),
                    QuotationComparisonsCount = await _context.QuotationComparisons.CountAsync(),
                    UserSessionsCount = await _context.UserSessions.CountAsync(),
                    LastOrder = await _context.Orders
                        .OrderByDescending(o => o.OrderDate)
                        .FirstOrDefaultAsync(),
                    RecentQuotations = await _context.QuotationComparisons
                        .OrderByDescending(qc => qc.CreatedDate)
                        .Take(5)
                        .ToListAsync()
                };

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database status");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("orders/{orderId}/details")]
        public async Task<IActionResult> GetOrderDetails(int orderId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderItems)
                    .Include(o => o.QuotationComparisons)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return NotFound($"Order {orderId} not found");
                }

                var details = new
                {
                    Order = new
                    {
                        order.Id,
                        order.OrderNumber,
                        order.TotalAmount,
                        order.Status,
                        order.OrderDate,
                        order.Notes,
                        Customer = order.Customer != null ? new
                        {
                            order.Customer.FirstName,
                            order.Customer.LastName,
                            order.Customer.Email
                        } : null
                    },
                    OrderItems = order.OrderItems.Select(oi => new
                    {
                        oi.Id,
                        oi.ProductId,
                        oi.Quantity,
                        oi.UnitPrice,
                        oi.TotalPrice
                    }),
                    QuotationComparisons = order.QuotationComparisons.Select(qc => new
                    {
                        qc.Id,
                        qc.ProductId,
                        qc.DistributorName,
                        qc.UnitPrice,
                        qc.AvailableStock,
                        qc.EstimatedDeliveryDays,
                        qc.TotalPrice,
                        qc.Status,
                        qc.CreatedDate,
                        qc.Notes
                    })
                };

                return Ok(details);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order details");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("quotations/recent")]
        public async Task<IActionResult> GetRecentQuotations()
        {
            try
            {
                var quotations = await _context.QuotationComparisons
                    .Include(qc => qc.Order)
                    .Include(qc => qc.Product)
                    .OrderByDescending(qc => qc.CreatedDate)
                    .Take(20)
                    .Select(qc => new
                    {
                        qc.Id,
                        qc.OrderId,
                        qc.ProductId,
                        ProductName = qc.Product != null ? qc.Product.Name : "Unknown",
                        qc.DistributorName,
                        qc.UnitPrice,
                        qc.AvailableStock,
                        qc.EstimatedDeliveryDays,
                        qc.TotalPrice,
                        qc.Status,
                        qc.CreatedDate,
                        qc.Notes
                    })
                    .ToListAsync();

                return Ok(quotations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent quotations");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
