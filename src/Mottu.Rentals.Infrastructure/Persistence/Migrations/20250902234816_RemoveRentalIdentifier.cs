using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mottu.Rentals.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRentalIdentifier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_rentals_identifier",
                table: "rentals");

            migrationBuilder.DropColumn(
                name: "identifier",
                table: "rentals");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "identifier",
                table: "rentals",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_rentals_identifier",
                table: "rentals",
                column: "identifier");
        }
    }
}
