using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace MsLogistic.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "delivery_persons",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delivery_persons", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "delivery_zones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    delivery_person_id = table.Column<Guid>(type: "uuid", nullable: true),
                    code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    boundaries = table.Column<Polygon>(type: "geometry", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delivery_zones", x => x.id);
                    table.ForeignKey(
                        name: "FK_delivery_zones_delivery_persons_delivery_person_id",
                        column: x => x.delivery_person_id,
                        principalTable: "delivery_persons",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "routes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    delivery_zone_id = table.Column<Guid>(type: "uuid", nullable: false),
                    delivery_person_id = table.Column<Guid>(type: "uuid", nullable: true),
                    scheduled_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_location = table.Column<Point>(type: "geography", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routes", x => x.id);
                    table.ForeignKey(
                        name: "FK_routes_delivery_persons_delivery_person_id",
                        column: x => x.delivery_person_id,
                        principalTable: "delivery_persons",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_routes_delivery_zones_delivery_zone_id",
                        column: x => x.delivery_zone_id,
                        principalTable: "delivery_zones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    route_id = table.Column<Guid>(type: "uuid", nullable: true),
                    delivery_sequence = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    scheduled_delivery_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    delivery_address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    delivery_location = table.Column<Point>(type: "geography", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.id);
                    table.ForeignKey(
                        name: "FK_orders_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_orders_routes_route_id",
                        column: x => x.route_id,
                        principalTable: "routes",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "order_deliveries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    delivery_person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    location = table.Column<Point>(type: "geography", nullable: false),
                    delivered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    comments = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    image_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_deliveries", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_deliveries_delivery_persons_delivery_person_id",
                        column: x => x.delivery_person_id,
                        principalTable: "delivery_persons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_order_deliveries_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_items_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_order_items_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_delivery_zones_delivery_person_id",
                table: "delivery_zones",
                column: "delivery_person_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_deliveries_delivery_person_id",
                table: "order_deliveries",
                column: "delivery_person_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_deliveries_order_id",
                table: "order_deliveries",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_order_id",
                table: "order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_product_id",
                table: "order_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_customer_id",
                table: "orders",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_route_id",
                table: "orders",
                column: "route_id");

            migrationBuilder.CreateIndex(
                name: "IX_routes_delivery_person_id",
                table: "routes",
                column: "delivery_person_id");

            migrationBuilder.CreateIndex(
                name: "IX_routes_delivery_zone_id",
                table: "routes",
                column: "delivery_zone_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "order_deliveries");

            migrationBuilder.DropTable(
                name: "order_items");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "routes");

            migrationBuilder.DropTable(
                name: "delivery_zones");

            migrationBuilder.DropTable(
                name: "delivery_persons");
        }
    }
}
