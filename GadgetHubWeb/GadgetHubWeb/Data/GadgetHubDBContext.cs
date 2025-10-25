using Microsoft.EntityFrameworkCore;
using GadgetHubWeb.Models;

namespace GadgetHubWeb.Data
{
    public class GadgetHubDBContext : DbContext
    {
        public GadgetHubDBContext(DbContextOptions<GadgetHubDBContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Configure Customer entity
            builder.Entity<Customer>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(c => c.LastName).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Email).IsRequired().HasMaxLength(200);
                entity.Property(c => c.Phone).IsRequired().HasMaxLength(20);
                entity.Property(c => c.Address).HasMaxLength(500);
                entity.Property(c => c.City).HasMaxLength(100);
                entity.Property(c => c.Country).HasMaxLength(100);
                
                // Authentication fields
                entity.Property(c => c.Password).HasMaxLength(500);
                entity.Property(c => c.LastLogin).IsRequired(false);
                entity.Property(c => c.IsActive).IsRequired().HasDefaultValue(true);
                
                // Note: Removed unique constraint to prevent 500 errors
                // Email uniqueness will be handled in application logic
            });

            // Configure Order entity
            builder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.Property(o => o.OrderNumber).IsRequired().HasMaxLength(50);
                entity.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(o => o.Status).IsRequired().HasMaxLength(50);
                entity.Property(o => o.Notes).HasMaxLength(1000);

                entity.HasOne(o => o.Customer)
                      .WithMany(c => c.Orders)
                      .HasForeignKey(o => o.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure OrderItem entity
            builder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(oi => oi.Id);
                entity.Property(oi => oi.UnitPrice).HasColumnType("decimal(10,2)");
                entity.Property(oi => oi.TotalPrice).HasColumnType("decimal(10,2)");

                entity.HasOne(oi => oi.Order)
                      .WithMany(o => o.OrderItems)
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Product entity
            builder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
                entity.Property(p => p.Description).HasMaxLength(1000);
                entity.Property(p => p.Price).HasColumnType("decimal(10,2)");
                entity.Property(p => p.Category).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Stock).IsRequired().HasDefaultValue(0);
            });

            // Configure UserSession entity
            builder.Entity<UserSession>(entity =>
            {
                entity.HasKey(us => us.SessionId);
                entity.Property(us => us.SessionId).IsRequired().HasMaxLength(100);
                entity.Property(us => us.CustomerId).IsRequired();
                entity.Property(us => us.Email).IsRequired().HasMaxLength(255);
                entity.Property(us => us.CreatedAt).IsRequired();
                entity.Property(us => us.ExpiresAt).IsRequired();

                // Foreign key relationship
                entity.HasOne(us => us.Customer)
                      .WithMany()
                      .HasForeignKey(us => us.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

