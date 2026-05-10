using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebActionResults.Migrations
{
    /// <inheritdoc />
    public partial class AddCartItemPriceColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BasePrice",
                table: "CartItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceAdjustment",
                table: "CartItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PriceBreakdown",
                table: "CartItems",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BasePrice",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "PriceAdjustment",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "PriceBreakdown",
                table: "CartItems");
        }
    }
}
