using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory_Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddingWarehouse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Wearhouses",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "citext", nullable: true),
                    Address = table.Column<string>(type: "citext", nullable: true),
                    ContactPerson = table.Column<string>(type: "citext", nullable: true),
                    Company = table.Column<string>(type: "citext", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wearhouses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wearhouses_products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "public",
                        principalTable: "products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Wearhouses_ProductId",
                schema: "public",
                table: "Wearhouses",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Wearhouses",
                schema: "public");
        }
    }
}
