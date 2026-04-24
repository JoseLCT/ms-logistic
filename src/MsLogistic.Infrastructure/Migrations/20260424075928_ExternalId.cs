using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MsLogistic.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExternalId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExternalId",
                table: "products",
                newName: "external_id");

            migrationBuilder.RenameColumn(
                name: "ExternalId",
                table: "customers",
                newName: "external_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "external_id",
                table: "products",
                newName: "ExternalId");

            migrationBuilder.RenameColumn(
                name: "external_id",
                table: "customers",
                newName: "ExternalId");
        }
    }
}
