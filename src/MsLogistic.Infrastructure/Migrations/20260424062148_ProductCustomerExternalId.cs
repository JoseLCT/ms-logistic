using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MsLogistic.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProductCustomerExternalId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ExternalId",
                table: "products",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ExternalId",
                table: "customers",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "products");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "customers");
        }
    }
}
