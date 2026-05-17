using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebActionResults.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderRequestFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancelAdminNote",
                table: "Orders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CancelApproved",
                table: "Orders",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelReason",
                table: "Orders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelRequestedAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CancelRequestedFromStatus",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelReviewedAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnAdminNote",
                table: "Orders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ReturnApproved",
                table: "Orders",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnImageUrl",
                table: "Orders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnReason",
                table: "Orders",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnRequestedAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReturnRequestedFromStatus",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnReviewedAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelAdminNote",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CancelApproved",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CancelReason",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CancelRequestedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CancelRequestedFromStatus",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CancelReviewedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReturnAdminNote",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReturnApproved",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReturnImageUrl",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReturnReason",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReturnRequestedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReturnRequestedFromStatus",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReturnReviewedAt",
                table: "Orders");
        }
    }
}
