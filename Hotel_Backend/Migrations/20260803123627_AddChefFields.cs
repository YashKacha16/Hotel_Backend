using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hotel_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddChefFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChefDescription",
                table: "HotelSettings",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChefImageUrl",
                table: "HotelSettings",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChefName",
                table: "HotelSettings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChefDescription",
                table: "HotelSettings");

            migrationBuilder.DropColumn(
                name: "ChefImageUrl",
                table: "HotelSettings");

            migrationBuilder.DropColumn(
                name: "ChefName",
                table: "HotelSettings");
        }
    }
}
