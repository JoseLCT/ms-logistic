using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MsLogistic.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GeneralMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "products",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "products",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "order_deliveries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "delivery_zones",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "delivery_zones",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_at",
                table: "products");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "products");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "order_deliveries");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "delivery_zones");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "delivery_zones");
        }
    }
}
