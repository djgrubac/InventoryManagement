using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory_Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovedProductWarehouseRealtionship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Wearhouses_products_ProductId",
                schema: "public",
                table: "Wearhouses");

            migrationBuilder.DropIndex(
                name: "IX_Wearhouses_ProductId",
                schema: "public",
                table: "Wearhouses");

            migrationBuilder.DropColumn(
                name: "ProductId",
                schema: "public",
                table: "Wearhouses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProductId",
                schema: "public",
                table: "Wearhouses",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wearhouses_ProductId",
                schema: "public",
                table: "Wearhouses",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Wearhouses_products_ProductId",
                schema: "public",
                table: "Wearhouses",
                column: "ProductId",
                principalSchema: "public",
                principalTable: "products",
                principalColumn: "Id");
        }
    }
}
