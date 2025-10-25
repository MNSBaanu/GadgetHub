using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GadgetHubAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove all seeded product data
            migrationBuilder.Sql("DELETE FROM Products WHERE Id IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore seeded data if needed (for rollback)
            var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            
            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Name", "Description", "Price", "Stock", "Category", "CreatedDate", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, "iPhone 15 Pro Max", "Latest iPhone with titanium design and A17 Pro chip", 450000m, 15, "iPhone", seedDate, seedDate },
                    { 2, "iPhone 15 Pro", "Pro iPhone with titanium design and advanced camera system", 380000m, 20, "iPhone", seedDate, seedDate },
                    { 3, "iPhone 15", "Latest iPhone with Dynamic Island and USB-C", 320000m, 25, "iPhone", seedDate, seedDate },
                    { 4, "MacBook Pro 16-inch M3 Max", "Powerful MacBook Pro with M3 Max chip for professionals", 850000m, 8, "Mac", seedDate, seedDate },
                    { 5, "MacBook Pro 14-inch M3", "Compact MacBook Pro with M3 chip and Liquid Retina XDR", 650000m, 12, "Mac", seedDate, seedDate },
                    { 6, "MacBook Air 15-inch M2", "Large MacBook Air with M2 chip and all-day battery", 420000m, 18, "Mac", seedDate, seedDate },
                    { 7, "iPad Pro 12.9-inch M2", "Professional iPad with M2 chip and Liquid Retina XDR", 280000m, 10, "iPad", seedDate, seedDate },
                    { 8, "iPad Pro 11-inch M2", "Compact iPad Pro with M2 chip and ProMotion", 220000m, 15, "iPad", seedDate, seedDate },
                    { 9, "iPad Air 10.9-inch M1", "Powerful iPad Air with M1 chip and Touch ID", 150000m, 20, "iPad", seedDate, seedDate },
                    { 10, "Apple Watch Series 9", "Latest Apple Watch with S9 chip and advanced health features", 85000m, 30, "Apple Watch", seedDate, seedDate },
                    { 11, "Apple Watch SE 2nd Gen", "Affordable Apple Watch with essential features", 45000m, 35, "Apple Watch", seedDate, seedDate },
                    { 12, "Apple Watch Ultra 2", "Rugged Apple Watch for extreme sports and adventures", 120000m, 12, "Apple Watch", seedDate, seedDate },
                    { 13, "AirPods Pro 2nd Gen", "Premium AirPods with Active Noise Cancellation", 65000m, 40, "AirPods", seedDate, seedDate },
                    { 14, "AirPods 3rd Gen", "Latest AirPods with spatial audio and adaptive EQ", 35000m, 50, "AirPods", seedDate, seedDate },
                    { 15, "AirPods Max", "Over-ear headphones with Active Noise Cancellation", 95000m, 15, "AirPods", seedDate, seedDate },
                    { 16, "Apple TV 4K 3rd Gen", "Latest Apple TV with A15 Bionic chip and Siri Remote", 45000m, 25, "Apple TV", seedDate, seedDate },
                    { 17, "Apple TV HD", "Apple TV with A8 chip and Siri Remote", 25000m, 30, "Apple TV", seedDate, seedDate },
                    { 18, "Apple TV 4K 2nd Gen", "Apple TV 4K with A12 Bionic chip", 35000m, 20, "Apple TV", seedDate, seedDate },
                    { 19, "Magic Keyboard for iPad Pro", "Premium keyboard with trackpad for iPad Pro", 85000m, 15, "Accessories", seedDate, seedDate },
                    { 20, "Apple Pencil 2nd Gen", "Precision stylus for iPad Pro and iPad Air", 25000m, 25, "Accessories", seedDate, seedDate },
                    { 21, "MagSafe Charger", "Wireless charger with magnetic alignment", 15000m, 40, "Accessories", seedDate, seedDate }
                });
        }
    }
}
