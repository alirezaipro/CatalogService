using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogItemTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Category",
                schema: "catalog",
                table: "CatalogCategories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Brand",
                schema: "catalog",
                table: "CatalogBrands",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "CatalogItems",
                schema: "catalog",
                columns: table => new
                {
                    Slug = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    AvailableStock = table.Column<int>(type: "int", nullable: false),
                    MaxStockThreshold = table.Column<int>(type: "int", nullable: false),
                    CatalogBrandId = table.Column<int>(type: "int", nullable: false),
                    CatalogCategoryId = table.Column<int>(type: "int", nullable: false),
                    Medias = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogItems", x => x.Slug);
                    table.ForeignKey(
                        name: "FK_CatalogItems_CatalogBrands_CatalogBrandId",
                        column: x => x.CatalogBrandId,
                        principalSchema: "catalog",
                        principalTable: "CatalogBrands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CatalogItems_CatalogCategories_CatalogCategoryId",
                        column: x => x.CatalogCategoryId,
                        principalSchema: "catalog",
                        principalTable: "CatalogCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItems_CatalogBrandId",
                schema: "catalog",
                table: "CatalogItems",
                column: "CatalogBrandId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItems_CatalogCategoryId",
                schema: "catalog",
                table: "CatalogItems",
                column: "CatalogCategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogItems",
                schema: "catalog");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                schema: "catalog",
                table: "CatalogCategories",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Brand",
                schema: "catalog",
                table: "CatalogBrands",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
