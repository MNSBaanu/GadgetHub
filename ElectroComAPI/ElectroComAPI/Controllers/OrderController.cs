using Microsoft.AspNetCore.Mvc;
using ElectroComAPI.Data;
using ElectroComAPI.DTO;
using ElectroComAPI.Model;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ElectroComAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly ElectroComDBContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderController> _logger;

        public OrderController(ElectroComDBContext context, IMapper mapper, ILogger<OrderController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<OrderResponseDTO>> CreateOrder([FromBody] OrderCreateDTO orderRequest)
        {
            try
            {
                _logger.LogInformation($"📥 ElectroCom: Creating order: {orderRequest.OrderNumber}");
                _logger.LogInformation($"💰 Order Total: {orderRequest.TotalAmount:C}, Items: {orderRequest.OrderItems.Count}");

                var order = _mapper.Map<Order>(orderRequest);
                order.Status = "Received";
                order.OrderDate = DateTime.UtcNow;

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Load the order with related data
                var createdOrder = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == order.Id);

                var orderDTO = _mapper.Map<OrderResponseDTO>(createdOrder);
                
                _logger.LogInformation($"✅ ElectroCom: Order created successfully in database");
                _logger.LogInformation($"📊 Order ID: {orderDTO.Id}, OrderNumber: {orderDTO.OrderNumber}");
                _logger.LogInformation($"📦 OrderItems created: {orderDTO.OrderItems.Count} items");
                _logger.LogInformation($"✅ ElectroCom: Order and OrderItems tables updated successfully");
                
                // Return order confirmation with order ID
                var orderConfirmation = new
                {
                    success = true,
                    message = "Order confirmed successfully",
                    orderId = orderDTO.Id,
                    orderNumber = orderDTO.OrderNumber,
                    status = orderDTO.Status,
                    totalAmount = orderDTO.TotalAmount,
                    estimatedDeliveryDays = 2, // ElectroCom delivery estimate
                    estimatedDeliveryDate = DateTime.UtcNow.AddDays(2).ToString("yyyy-MM-dd"),
                    distributorName = "ElectroCom"
                };
                
                _logger.LogInformation($"✅ ElectroCom: Order confirmation sent - Order ID: {orderDTO.Id}, Delivery: {DateTime.UtcNow.AddDays(2):yyyy-MM-dd}");
                
                return CreatedAtAction(nameof(GetOrder), new { id = orderDTO.Id }, orderConfirmation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ElectroCom: Error creating order");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponseDTO>> GetOrder(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    return NotFound();
                }

                var orderDTO = _mapper.Map<OrderResponseDTO>(order);
                return Ok(orderDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<OrderResponseDTO>>> GetOrders()
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                var orderDTOs = _mapper.Map<List<OrderResponseDTO>>(orders);
                return Ok(orderDTOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}/status")]
        public async Task<ActionResult> UpdateOrderStatus(int id, [FromBody] OrderStatusUpdateDTO statusUpdate)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                {
                    return NotFound();
                }

                order.Status = statusUpdate.Status;
                if (statusUpdate.ShippedDate.HasValue)
                    order.ShippedDate = statusUpdate.ShippedDate.Value;
                if (statusUpdate.DeliveredDate.HasValue)
                    order.DeliveredDate = statusUpdate.DeliveredDate.Value;
                if (!string.IsNullOrEmpty(statusUpdate.Notes))
                    order.Notes = statusUpdate.Notes;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Order {order.OrderNumber} status updated to: {order.Status}");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("by-number/{orderNumber}")]
        public async Task<ActionResult<OrderResponseDTO>> GetOrderByNumber(string orderNumber)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

                if (order == null)
                {
                    return NotFound();
                }

                var orderDTO = _mapper.Map<OrderResponseDTO>(order);
                return Ok(orderDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order by number");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
