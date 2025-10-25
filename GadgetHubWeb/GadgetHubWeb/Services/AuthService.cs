using GadgetHubWeb.Data;
using GadgetHubWeb.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace GadgetHubWeb.Services
{
    public class AuthService
    {
        private readonly GadgetHubDBContext _context;
        private readonly ILogger<AuthService> _logger;

        public AuthService(GadgetHubDBContext context, ILogger<AuthService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Register a new customer
        public async Task<Customer?> RegisterCustomer(string firstName, string lastName, string email, string password, string phone)
        {
            try
            {
                _logger.LogInformation($"Starting registration for email: {email}");

                // Test database connection first
                try
                {
                    var canConnect = await _context.Database.CanConnectAsync();
                    _logger.LogInformation($"Database connection test: {(canConnect ? "Success" : "Failed")}");
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Database connection test failed");
                    throw new Exception($"Database connection failed: {dbEx.Message}", dbEx);
                }

                // Check if user already exists
                _logger.LogInformation("Checking for existing customer...");
                var existingCustomer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == email && c.IsActive);

                if (existingCustomer != null)
                {
                    _logger.LogWarning($"Registration failed: User with email {email} already exists");
                    return null;
                }

                _logger.LogInformation("No existing customer found, proceeding with registration");

                // Hash password
                _logger.LogInformation("Hashing password...");
                var hashedPassword = HashPassword(password);
                _logger.LogInformation("Password hashed successfully");

                // Create new customer
                _logger.LogInformation("Creating customer object...");
                var customer = new Customer
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    Password = hashedPassword,
                    Phone = string.IsNullOrEmpty(phone) ? "" : phone,
                    Address = "",
                    City = "",
                    Country = "",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                _logger.LogInformation("Customer object created, adding to database...");
                _context.Customers.Add(customer);
                
                _logger.LogInformation("Attempting to save changes to database...");
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Customer registered successfully: {email} with ID: {customer.Id}");
                return customer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error registering customer: {email}. Exception: {ex.Message}");
                _logger.LogError(ex, $"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        // Authenticate customer login
        public async Task<Customer?> AuthenticateCustomer(string email, string password)
        {
            try
            {
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == email && c.IsActive);

                if (customer == null)
                {
                    _logger.LogWarning($"Login failed: Customer not found for email {email}");
                    return null;
                }

                if (customer.Password == null || !VerifyPassword(password, customer.Password))
                {
                    _logger.LogWarning($"Login failed: Invalid password for email {email}");
                    return null;
                }

                _logger.LogInformation($"Customer authenticated successfully: {email}");
                return customer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error authenticating customer: {email}");
                throw;
            }
        }

        // Authenticate admin login (hardcoded credentials)
        public bool AuthenticateAdmin(string email, string password)
        {
            // Hardcoded admin credentials
            const string ADMIN_EMAIL = "admin@gadgethub.com";
            const string ADMIN_PASSWORD = "admin123";

            if (email == ADMIN_EMAIL && password == ADMIN_PASSWORD)
            {
                _logger.LogInformation("Admin authenticated successfully");
                return true;
            }

            _logger.LogWarning($"Admin login failed for email: {email}");
            return false;
        }

        // Create session for customer
        public async Task<string> CreateSession(Customer customer)
        {
            try
            {
                // Generate session ID
                var sessionId = Guid.NewGuid().ToString();

                // Store session in database (you might want to create a Sessions table)
                // For now, we'll use a simple approach with customer ID as session reference
                customer.LastLogin = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Session created for customer: {customer.Email}");
                return sessionId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating session for customer: {customer.Email}");
                throw;
            }
        }

        // Get customer by session ID (simplified approach)
        public Customer? GetCustomerBySession(string sessionId)
        {
            try
            {
                // This is a simplified approach - in production, you'd have a proper sessions table
                // For now, we'll return null and let the frontend handle session validation
                _logger.LogInformation($"Session lookup requested for: {sessionId}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting customer by session: {sessionId}");
                return null;
            }
        }

        // Hash password using SHA256 with salt
        private string HashPassword(string password)
        {
            const string SALT = "GadgetHub2024";
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = password + SALT;
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        // Verify password
        private bool VerifyPassword(string password, string hashedPassword)
        {
            var hashedInput = HashPassword(password);
            return hashedInput == hashedPassword;
        }

        // Logout customer (clean up session)
        public bool LogoutCustomer(string sessionId)
        {
            try
            {
                // In a real implementation, you'd remove the session from database
                _logger.LogInformation($"Customer logged out: {sessionId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error logging out customer: {sessionId}");
                return false;
            }
        }

        // Get customer count for admin dashboard
        public async Task<int> GetCustomerCount()
        {
            try
            {
                return await _context.Customers.CountAsync(c => c.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer count");
                return 0;
            }
        }

        // Test database connection
        public async Task<bool> TestDatabaseConnection()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                _logger.LogInformation($"Database connection test: {(canConnect ? "Success" : "Failed")}");
                return canConnect;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database connection test failed");
                return false;
            }
        }
    }
}
