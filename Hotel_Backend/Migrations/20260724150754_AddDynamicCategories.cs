using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hotel_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddDynamicCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Create Categories table
            migrationBuilder.CreateTable(
                name: "Categories",
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
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            // 2. Add CategoryId column to MenuItems (temporarily nullable so we can map it)
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "MenuItems",
                type: "int",
                nullable: true);

            // 3. Migrate unique string Category values from MenuItems into the new Categories table
            migrationBuilder.Sql("INSERT INTO Categories (Name, Position, IsActive) SELECT DISTINCT Category, 0, 1 FROM MenuItems WHERE Category IS NOT NULL AND Category <> ''");

            // 4. Ensure a default 'General' category exists in case table was empty or has no categories
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM Categories) INSERT INTO Categories (Name, Position, IsActive) VALUES ('General', 0, 1)");

            // 5. Update MenuItems to map CategoryId using the Category name matching
            migrationBuilder.Sql("UPDATE m SET m.CategoryId = c.Id FROM MenuItems m INNER JOIN Categories c ON m.Category = c.Name");

            // 6. Map any remaining unmapped items (null or empty categories) to the default category
            migrationBuilder.Sql("DECLARE @DefaultId INT; SELECT TOP 1 @DefaultId = Id FROM Categories ORDER BY Id; UPDATE MenuItems SET CategoryId = @DefaultId WHERE CategoryId IS NULL");

            // 7. Make CategoryId non-nullable now that all rows are mapped
            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "MenuItems",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // 8. Drop the old string Category column
            migrationBuilder.DropColumn(
                name: "Category",
                table: "MenuItems");

            // 9. Add other columns to MenuItems
            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "MenuItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Position",
                table: "MenuItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_CategoryId",
                table: "MenuItems",
                column: "CategoryId");

            // 10. Add the Foreign Key constraint
            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_Categories_CategoryId",
                table: "MenuItems",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuItems_Categories_CategoryId",
                table: "MenuItems");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_MenuItems_CategoryId",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "Image",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "MenuItems");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "MenuItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
