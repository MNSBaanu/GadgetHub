using Microsoft.EntityFrameworkCore;
using TechWorldAPI.Model;
using TechWorldAPI.DTO;

namespace TechWorldAPI.Data
{
    public class QuotationRepo
    {
        private readonly TechWorldDBContext _context;

        public QuotationRepo(TechWorldDBContext context)
        {
            _context = context;
        }

        public Quotation? CreateQuotation(QuotationRequestDTO request)
        {
            try
            {
                var product = _context.Products.FirstOrDefault(p => p.Id == request.ProductId);
                if (product == null || product.Stock < request.Quantity)
                {
                    return null;
                }

                // TechWorld's pricing logic - premium pricing with loyalty discounts
                decimal unitPrice = product.Price;
                if (request.Quantity >= 20)
                {
                    unitPrice *= 0.90m; // 10% discount for large orders
                }
                else if (request.Quantity >= 10)
                {
                    unitPrice *= 0.95m; // 5% discount for medium orders
                }

                var quotation = new Quotation
                {
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    UnitPrice = unitPrice,
                    AvailableStock = product.Stock,
                    EstimatedDeliveryDays = CalculateDeliveryDays(request.Quantity),
                    TotalPrice = unitPrice * request.Quantity,
                    Status = "Pending",
                    CreatedDate = DateTime.UtcNow,
                    ExpiryDate = DateTime.UtcNow.AddHours(48), // 48-hour validity
                    Notes = "TechWorld - Premium quality guaranteed"
                };

                _context.Quotations.Add(quotation);
                _context.SaveChanges();
                return quotation;
            }
            catch
            {
                return null;
            }
        }

        public List<Quotation> GetQuotations()
        {
            return _context.Quotations.Include(q => q.Product).ToList();
        }

        public Quotation? GetQuotationById(int id)
        {
            return _context.Quotations.Include(q => q.Product).FirstOrDefault(q => q.Id == id);
        }

        public bool UpdateQuotationStatus(int id, string status)
        {
            try
            {
                var quotation = _context.Quotations.FirstOrDefault(q => q.Id == id);
                if (quotation != null)
                {
                    quotation.Status = status;
                    _context.SaveChanges();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private int CalculateDeliveryDays(int quantity)
        {
            // TechWorld's delivery logic - longer for larger quantities
            if (quantity <= 3)
                return 1; // 1 day for small orders
            else if (quantity <= 10)
                return 2; // 2 days for medium orders
            else
                return 4; // 4 days for large orders
        }
    }
}
