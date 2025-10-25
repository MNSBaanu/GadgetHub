using Microsoft.EntityFrameworkCore;
using TechWorldAPI.Model;

namespace TechWorldAPI.Data
{
    public class ProductRepo
    {
        private readonly TechWorldDBContext _context;

        public ProductRepo(TechWorldDBContext context)
        {
            _context = context;
        }

        public List<Product> GetProducts()
        {
            return _context.Products.ToList();
        }

        public Product? GetProductById(int id)
        {
            return _context.Products.FirstOrDefault(p => p.Id == id);
        }

        public bool AddProduct(Product product)
        {
            try
            {
                _context.Products.Add(product);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateProduct(Product product)
        {
            try
            {
                _context.Products.Update(product);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteProduct(Product product)
        {
            try
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public List<Product> GetProductsByCategory(string category)
        {
            return _context.Products.Where(p => p.Category == category).ToList();
        }

        public List<Product> SearchProducts(string searchTerm)
        {
            return _context.Products.Where(p => 
                p.Name.Contains(searchTerm) || 
                p.Description.Contains(searchTerm) || 
                p.Category.Contains(searchTerm)).ToList();
        }
    }
}
