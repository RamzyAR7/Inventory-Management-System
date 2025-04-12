using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class AddPKForCustomerOrderTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomerOrders",
                table: "CustomerOrders");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: new Guid("160ade4a-1e3c-40b0-9eed-e6d56e125739"));

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerOrderID",
                table: "CustomerOrders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomerOrders",
                table: "CustomerOrders",
                column: "CustomerOrderID");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "CreatedAt", "Email", "FullName", "PasswordHash", "Role" },
                values: new object[] { new Guid("556fa4bd-2343-485c-8644-0bb9fe61e424"), new DateTime(2025, 4, 12, 2, 0, 10, 831, DateTimeKind.Utc).AddTicks(2187), "admin@gmail.com", "Admin", "$2a$11$TsoAxEL1JVGf42YbyYU3tOTdB1VvzKwsof7THTXTMtU1BCrsEVeya", "Admin" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrders_CustomerID_OrderID",
                table: "CustomerOrders",
                columns: new[] { "CustomerID", "OrderID" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomerOrders",
                table: "CustomerOrders");

            migrationBuilder.DropIndex(
                name: "IX_CustomerOrders_CustomerID_OrderID",
                table: "CustomerOrders");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: new Guid("556fa4bd-2343-485c-8644-0bb9fe61e424"));

            migrationBuilder.DropColumn(
                name: "CustomerOrderID",
                table: "CustomerOrders");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomerOrders",
                table: "CustomerOrders",
                columns: new[] { "CustomerID", "OrderID" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "CreatedAt", "Email", "FullName", "PasswordHash", "Role" },
                values: new object[] { new Guid("160ade4a-1e3c-40b0-9eed-e6d56e125739"), new DateTime(2025, 4, 6, 21, 46, 0, 701, DateTimeKind.Utc).AddTicks(6141), "admin@gmail.com", "Admin", "$2a$11$hdS5IWB9il2h1qpEmlPTNuB553EnPXgPCks..kqZ4H4cIsLD/Zg92", "Admin" });
        }
    }
}
