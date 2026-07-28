using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hotel_Backend.Migrations
{
    /// <inheritdoc />
    public partial class updatechanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WaitlistEstimatedWaitMinutes",
                table: "HotelSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "WaitlistMessage",
                table: "HotelSettings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WaitlistEstimatedWaitMinutes",
                table: "HotelSettings");

            migrationBuilder.DropColumn(
                name: "WaitlistMessage",
                table: "HotelSettings");
        }
    }
}
