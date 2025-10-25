using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GadgetHubAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddGadgetHubSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Category", "CreatedDate", "Description", "Name", "Price", "Stock", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, "iPhone", new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(1681), "Latest iPhone with titanium design and A17 Pro chip", "iPhone 15 Pro Max", 450000m, 15, new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2060) },
                    { 2, "iPhone", new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2407), "Pro iPhone with titanium design and advanced camera system", "iPhone 15 Pro", 380000m, 20, new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2408) },
                    { 3, "iPhone", new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2412), "Latest iPhone with Dynamic Island and USB-C", "iPhone 15", 320000m, 25, new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2413) },
                    { 4, "Mac", new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2417), "Powerful MacBook Pro with M3 Max chip for professionals", "MacBook Pro 16-inch M3 Max", 850000m, 8, new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2418) },
                    { 5, "Mac", new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2421), "Compact MacBook Pro with M3 chip and Liquid Retina XDR", "MacBook Pro 14-inch M3", 650000m, 12, new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2422) },
                    { 6, "Mac", new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2424), "Large MacBook Air with M2 chip and all-day battery", "MacBook Air 15-inch M2", 420000m, 18, new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2425) },
                    { 7, "iPad", new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2429), "Professional iPad with M2 chip and Liquid Retina XDR", "iPad Pro 12.9-inch M2", 280000m, 10, new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2430) },
                    { 8, "iPad", new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2432), "Compact iPad Pro with M2 chip and ProMotion", "iPad Pro 11-inch M2", 220000m, 15, new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2433) },
                    { 9, "iPad", new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2436), "Powerful iPad Air with M1 chip and Touch ID", "iPad Air 10.9-inch M1", 150000m, 20, new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2437) },
                    { 10, "Apple Watch", new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2440), "Latest Apple Watch with S9 chip and advanced health features", "Apple Watch Series 9", 85000m, 30, new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2441) },
                    { 11, "Apple Watch", new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2443), "Affordable Apple Watch with essential features", "Apple Watch SE 2nd Gen", 45000m, 35, new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2444) },
                    { 12, "Apple Watch", new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2447), "Rugged Apple Watch for extreme sports and adventures", "Apple Watch Ultra 2", 120000m, 12, new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2448) },
                    { 13, "AirPods", new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2451), "Premium AirPods with Active Noise Cancellation", "AirPods Pro 2nd Gen", 65000m, 40, new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2452) },
                    { 14, "AirPods", new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2454), "Latest AirPods with spatial audio and adaptive EQ", "AirPods 3rd Gen", 35000m, 50, new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2455) },
                    { 15, "AirPods", new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2458), "Over-ear headphones with Active Noise Cancellation", "AirPods Max", 95000m, 15, new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2459) },
                    { 16, "Apple TV", new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2462), "Latest Apple TV with A15 Bionic chip and Siri Remote", "Apple TV 4K 3rd Gen", 45000m, 25, new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2463) },
                    { 17, "Apple TV", new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2465), "Apple TV with A8 chip and Siri Remote", "Apple TV HD", 25000m, 30, new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2466) },
                    { 18, "Apple TV", new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2469), "Apple TV 4K with A12 Bionic chip", "Apple TV 4K 2nd Gen", 35000m, 20, new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2470) },
                    { 19, "Accessories", new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2473), "Premium keyboard with trackpad for iPad Pro", "Magic Keyboard for iPad Pro", 85000m, 15, new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2474) },
                    { 20, "Accessories", new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2477), "Precision stylus for iPad Pro and iPad Air", "Apple Pencil 2nd Gen", 25000m, 25, new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2477) },
                    { 21, "Accessories", new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2480), "Wireless charger with magnetic alignment", "MagSafe Charger", 15000m, 40, new DateTime(2025, 9, 25, 6, 15, 12, 675, DateTimeKind.Utc).AddTicks(2481) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 21);
        }
    }
}
