using Microsoft.EntityFrameworkCore;
using ElectroComAPI.Model;

namespace ElectroComAPI.Data
{
    public class ElectroComDBContext : DbContext
    {
        public ElectroComDBContext(DbContextOptions<ElectroComDBContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Quotation> Quotations { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Configure Product entity
            builder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Price).HasColumnType("decimal(10,2)");
                entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
                entity.Property(p => p.Description).IsRequired().HasMaxLength(1000);
                entity.Property(p => p.Category).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Stock).IsRequired();
                entity.Property(p => p.CreatedDate).IsRequired();
                entity.Property(p => p.UpdatedDate).IsRequired();
            });

            // Configure Quotation entity
            builder.Entity<Quotation>(entity =>
            {
                entity.HasKey(q => q.Id);
                entity.Property(q => q.UnitPrice).HasColumnType("decimal(10,2)");
                entity.Property(q => q.TotalPrice).HasColumnType("decimal(10,2)");
                entity.Property(q => q.Status).IsRequired().HasMaxLength(50);
                entity.Property(q => q.Notes).HasMaxLength(500);

                entity.HasOne(q => q.Product)
                      .WithMany()
                      .HasForeignKey(q => q.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Order entity
            builder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.Property(o => o.TotalAmount).HasColumnType("decimal(10,2)");
                entity.Property(o => o.Status).IsRequired().HasMaxLength(50);
                entity.Property(o => o.Notes).HasMaxLength(500);
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

                entity.HasOne(oi => oi.Product)
                      .WithMany()
                      .HasForeignKey(oi => oi.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

        }
    }
}