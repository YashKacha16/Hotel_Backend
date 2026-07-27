using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hotel_Backend.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyTablesAddQrAutoPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RestaurantTables_Categories_CategoryId",
                table: "RestaurantTables");

            migrationBuilder.DropColumn(
                name: "Number",
                table: "RestaurantTables");

            migrationBuilder.DropColumn(
                name: "Shape",
                table: "RestaurantTables");

            migrationBuilder.DropColumn(
                name: "X",
                table: "RestaurantTables");

            migrationBuilder.RenameColumn(
                name: "Y",
                table: "RestaurantTables",
                newName: "Position");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastStatusChangedAt",
                table: "RestaurantTables",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastStatusChangedBy",
                table: "RestaurantTables",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "RestaurantTables",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "QrToken",
                table: "RestaurantTables",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "TableCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableCategories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantTables_Name",
                table: "RestaurantTables",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantTables_QrToken",
                table: "RestaurantTables",
                column: "QrToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TableCategories_Name",
                table: "TableCategories",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RestaurantTables_TableCategories_CategoryId",
                table: "RestaurantTables",
                column: "CategoryId",
                principalTable: "TableCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RestaurantTables_TableCategories_CategoryId",
                table: "RestaurantTables");

            migrationBuilder.DropTable(
                name: "TableCategories");

            migrationBuilder.DropIndex(
                name: "IX_RestaurantTables_Name",
                table: "RestaurantTables");

            migrationBuilder.DropIndex(
                name: "IX_RestaurantTables_QrToken",
                table: "RestaurantTables");

            migrationBuilder.DropColumn(
                name: "LastStatusChangedAt",
                table: "RestaurantTables");

            migrationBuilder.DropColumn(
                name: "LastStatusChangedBy",
                table: "RestaurantTables");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "RestaurantTables");

            migrationBuilder.DropColumn(
                name: "QrToken",
                table: "RestaurantTables");

            migrationBuilder.RenameColumn(
                name: "Position",
                table: "RestaurantTables",
                newName: "Y");

            migrationBuilder.AddColumn<string>(
                name: "Number",
                table: "RestaurantTables",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Shape",
                table: "RestaurantTables",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "X",
                table: "RestaurantTables",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_RestaurantTables_Categories_CategoryId",
                table: "RestaurantTables",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
