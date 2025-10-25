using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GadgetCentralAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddGadgetHubProductId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GadgetHubProductId",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GadgetHubProductId",
                table: "Products");
        }
    }
}
