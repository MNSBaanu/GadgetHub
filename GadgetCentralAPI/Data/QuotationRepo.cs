using Microsoft.EntityFrameworkCore;
using GadgetCentralAPI.Model;
using GadgetCentralAPI.DTO;

namespace GadgetCentralAPI.Data
{
    public class QuotationRepo
    {
        private readonly GadgetCentralDBContext _context;

        public QuotationRepo(GadgetCentralDBContext context)
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

                // GadgetCentral's pricing logic - budget-friendly with volume discounts
                decimal unitPrice = product.Price;
                if (request.Quantity >= 50)
                {
                    unitPrice *= 0.85m; // 15% discount for large orders
                }
                else if (request.Quantity >= 25)
                {
                    unitPrice *= 0.90m; // 10% discount for medium orders
                }
                else if (request.Quantity >= 10)
                {
                    unitPrice *= 0.95m; // 5% discount for small bulk orders
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
                    ExpiryDate = DateTime.UtcNow.AddHours(72), // 72-hour validity
                    Notes = "GadgetCentral - Best value for money"
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
            // GadgetCentral's delivery logic - consistent delivery times
            if (quantity <= 10)
                return 3; // 3 days for small orders
            else if (quantity <= 30)
                return 4; // 4 days for medium orders
            else
                return 6; // 6 days for large orders
        }
    }
}
