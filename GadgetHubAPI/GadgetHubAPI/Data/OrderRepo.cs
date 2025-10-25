using Microsoft.EntityFrameworkCore;
using GadgetHubAPI.Model;

namespace GadgetHubAPI.Data
{
    public class OrderRepo
    {
        private readonly GadgetHubDBContext _context;

        public OrderRepo(GadgetHubDBContext context)
        {
            _context = context;
        }

        public List<Order> GetOrders()
        {
            return _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ToList();
        }

        public List<Order> GetAllOrders()
        {
            return _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        public Order? GetOrderById(int id)
        {
            return _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.QuotationComparisons)
                .FirstOrDefault(o => o.Id == id);
        }

        public bool AddOrder(Order order)
        {
            try
            {
                _context.Orders.Add(order);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateOrder(Order order)
        {
            try
            {
                _context.Orders.Update(order);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteOrder(Order order)
        {
            try
            {
                _context.Orders.Remove(order);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public List<Order> GetOrdersByCustomerId(int customerId)
        {
            return _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.CustomerId == customerId)
                .ToList();
        }

        public Product? GetProductById(int productId)
        {
            return _context.Products.FirstOrDefault(p => p.Id == productId);
        }

        public bool DeleteOrder(int orderId)
        {
            try
            {
                var order = _context.Orders
                    .Include(o => o.OrderItems)
                    .Include(o => o.QuotationComparisons)
                    .FirstOrDefault(o => o.Id == orderId);

                if (order == null)
                {
                    return false;
                }

                // Delete related data first (cascade delete)
                _context.QuotationComparisons.RemoveRange(order.QuotationComparisons);
                _context.OrderItems.RemoveRange(order.OrderItems);
                _context.Orders.Remove(order);

                _context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
