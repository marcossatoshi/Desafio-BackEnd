using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mottu.Rentals.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "couriers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    identifier = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cnpj = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    birth_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cnh_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    cnh_type = table.Column<int>(type: "integer", nullable: false),
                    cnh_image_path = table.Column<string>(type: "text", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_couriers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "motorcycle_created_notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    motorcycle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    published_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_motorcycle_created_notifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "motorcycles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    identifier = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    model = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    plate = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_motorcycles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rentals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    identifier = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    motorcycle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    courier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    expected_end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    daily_price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    total_price = table.Column<decimal>(type: "numeric(10,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rentals", x => x.id);
                    table.ForeignKey(
                        name: "FK_rentals_couriers_courier_id",
                        column: x => x.courier_id,
                        principalTable: "couriers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_rentals_motorcycles_motorcycle_id",
                        column: x => x.motorcycle_id,
                        principalTable: "motorcycles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_couriers_cnh_number",
                table: "couriers",
                column: "cnh_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_couriers_cnpj",
                table: "couriers",
                column: "cnpj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_couriers_identifier",
                table: "couriers",
                column: "identifier");

            migrationBuilder.CreateIndex(
                name: "IX_motorcycles_identifier",
                table: "motorcycles",
                column: "identifier");

            migrationBuilder.CreateIndex(
                name: "IX_motorcycles_plate",
                table: "motorcycles",
                column: "plate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rentals_courier_id",
                table: "rentals",
                column: "courier_id");

            migrationBuilder.CreateIndex(
                name: "IX_rentals_identifier",
                table: "rentals",
                column: "identifier");

            migrationBuilder.CreateIndex(
                name: "IX_rentals_motorcycle_id",
                table: "rentals",
                column: "motorcycle_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "motorcycle_created_notifications");

            migrationBuilder.DropTable(
                name: "rentals");

            migrationBuilder.DropTable(
                name: "couriers");

            migrationBuilder.DropTable(
                name: "motorcycles");
        }
    }
}
