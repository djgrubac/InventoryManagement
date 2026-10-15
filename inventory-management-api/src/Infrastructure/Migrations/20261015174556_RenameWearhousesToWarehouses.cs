using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory_Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameWearhousesToWarehouses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Wearhouses_WarehouseId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Stocks_Wearhouses_WarehouseId",
                table: "Stocks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Wearhouses",
                table: "Wearhouses");

            migrationBuilder.RenameTable(
                name: "Wearhouses",
                newName: "Warehouses");

            // RenameTable does not rename the identity sequence behind Id
            migrationBuilder.Sql("ALTER SEQUENCE \"Wearhouses_Id_seq\" RENAME TO \"Warehouses_Id_seq\";");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Warehouses",
                table: "Warehouses",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Warehouses_WarehouseId",
                table: "Orders",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Stocks_Warehouses_WarehouseId",
                table: "Stocks",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Warehouses_WarehouseId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Stocks_Warehouses_WarehouseId",
                table: "Stocks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Warehouses",
                table: "Warehouses");

            migrationBuilder.RenameTable(
                name: "Warehouses",
                newName: "Wearhouses");

            migrationBuilder.Sql("ALTER SEQUENCE \"Warehouses_Id_seq\" RENAME TO \"Wearhouses_Id_seq\";");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Wearhouses",
                table: "Wearhouses",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Wearhouses_WarehouseId",
                table: "Orders",
                column: "WarehouseId",
                principalTable: "Wearhouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Stocks_Wearhouses_WarehouseId",
                table: "Stocks",
                column: "WarehouseId",
                principalTable: "Wearhouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
