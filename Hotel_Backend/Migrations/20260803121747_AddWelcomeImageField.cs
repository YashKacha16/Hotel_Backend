using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hotel_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddWelcomeImageField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WelcomeImageUrl",
                table: "HotelSettings",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WelcomeImageUrl",
                table: "HotelSettings");
        }
    }
}
