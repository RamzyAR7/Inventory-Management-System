using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class InitReset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: new Guid("bca52738-b7ee-449b-84a8-bbdb74776a3b"));

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            /*migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "CreatedAt", "Email", "HashedPassword", "ManagerID", "Role", "UserName" },
                values: new object[] { new Guid("26549bc6-c007-403b-aa00-0b7574b8047c"), new DateTime(2025, 4, 20, 16, 33, 18, 712, DateTimeKind.Utc).AddTicks(3344), "admin@gmail.com", "$2a$11$yV2yDZoNpmJdYQDiiwDiCe1gAWSVqJ3Rf06hSQjHREpUMTAPDx2Ua", null, "Admin", "Admin" });*/
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: new Guid("26549bc6-c007-403b-aa00-0b7574b8047c"));

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "Customers");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "CreatedAt", "Email", "HashedPassword", "ManagerID", "Role", "UserName" },
                values: new object[] { new Guid("bca52738-b7ee-449b-84a8-bbdb74776a3b"), new DateTime(2025, 4, 20, 15, 22, 39, 226, DateTimeKind.Utc).AddTicks(7963), "admin@gmail.com", "$2a$11$yiHsGAV083zJ3zowg/GfFulI7u0sgERJjx8O7adyHCzFYhEU0F1Tu", null, "Admin", "Admin" });
        }
    }
}
