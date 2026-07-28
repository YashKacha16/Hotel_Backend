using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hotel_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddMinimumAdvancePercent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MinimumAdvancePercent",
                table: "HotelSettings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinimumAdvancePercent",
                table: "HotelSettings");
        }
    }
}
