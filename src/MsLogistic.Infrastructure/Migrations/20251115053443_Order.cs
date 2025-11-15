using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MsLogistic.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Order : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_order_deliveries_order_id",
                table: "order_deliveries");

            migrationBuilder.CreateIndex(
                name: "IX_order_deliveries_order_id",
                table: "order_deliveries",
                column: "order_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_order_deliveries_order_id",
                table: "order_deliveries");

            migrationBuilder.CreateIndex(
                name: "IX_order_deliveries_order_id",
                table: "order_deliveries",
                column: "order_id");
        }
    }
}
