using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GadgetCentralAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingProductColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryTime",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "GadgetHubProductId",
                table: "Products");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Products",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Products",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Category", "CreatedDate", "Description", "Name", "Price", "Stock", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, "iPhone", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Latest iPhone with titanium design and A17 Pro chip", "iPhone 15 Pro Max", 440000m, 18, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, "iPhone", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Pro iPhone with titanium design and advanced camera system", "iPhone 15 Pro", 370000m, 22, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, "iPhone", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Latest iPhone with Dynamic Island and USB-C", "iPhone 15", 310000m, 28, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, "Mac", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Powerful MacBook Pro with M3 Max chip for professionals", "MacBook Pro 16-inch M3 Max", 830000m, 10, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, "Mac", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Compact MacBook Pro with M3 chip and Liquid Retina XDR", "MacBook Pro 14-inch M3", 630000m, 14, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, "Mac", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Large MacBook Air with M2 chip and all-day battery", "MacBook Air 15-inch M2", 410000m, 20, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, "iPad", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Professional iPad with M2 chip and Liquid Retina XDR", "iPad Pro 12.9-inch M2", 270000m, 12, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 8, "iPad", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Compact iPad Pro with M2 chip and ProMotion", "iPad Pro 11-inch M2", 210000m, 18, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 9, "iPad", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Powerful iPad Air with M1 chip and Touch ID", "iPad Air 10.9-inch M1", 145000m, 25, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10, "Apple Watch", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Latest Apple Watch with S9 chip and advanced health features", "Apple Watch Series 9", 82000m, 35, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 11, "Apple Watch", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Affordable Apple Watch with essential features", "Apple Watch SE 2nd Gen", 43000m, 40, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 12, "Apple Watch", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Rugged Apple Watch for extreme sports and adventures", "Apple Watch Ultra 2", 115000m, 15, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 13, "AirPods", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Premium AirPods with Active Noise Cancellation", "AirPods Pro 2nd Gen", 63000m, 45, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 14, "AirPods", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Latest AirPods with spatial audio and adaptive EQ", "AirPods 3rd Gen", 33000m, 55, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 15, "AirPods", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Over-ear headphones with Active Noise Cancellation", "AirPods Max", 92000m, 18, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 16, "Apple TV", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Latest Apple TV with A15 Bionic chip and Siri Remote", "Apple TV 4K 3rd Gen", 43000m, 30, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 17, "Apple TV", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Apple TV with A8 chip and Siri Remote", "Apple TV HD", 23000m, 35, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 18, "Apple TV", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Apple TV 4K with A12 Bionic chip", "Apple TV 4K 2nd Gen", 33000m, 25, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 19, "Accessories", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Premium keyboard with trackpad for iPad Pro", "Magic Keyboard for iPad Pro", 82000m, 18, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 20, "Accessories", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Precision stylus for iPad Pro and iPad Air", "Apple Pencil 2nd Gen", 23000m, 30, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 21, "Accessories", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Wireless charger with magnetic alignment", "MagSafe Charger", 14000m, 50, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
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

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Products");

            migrationBuilder.AddColumn<int>(
                name: "DeliveryTime",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GadgetHubProductId",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
