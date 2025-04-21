using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class AddShipmentDisplayFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: new Guid("26549bc6-c007-403b-aa00-0b7574b8047c"));

            migrationBuilder.DropColumn(
                name: "Carrier",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ShipmentDate",
                table: "Shipments");

            migrationBuilder.RenameColumn(
                name: "Items",
                table: "Shipments",
                newName: "ItemCount");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "CreatedAt", "Email", "HashedPassword", "ManagerID", "Role", "UserName" },
                values: new object[] { new Guid("d473c74f-c281-492b-950f-1be962ed064f"), new DateTime(2025, 4, 20, 18, 7, 40, 963, DateTimeKind.Utc).AddTicks(5655), "admin@gmail.com", "$2a$11$a.Xm6sDahJyFXApcNwRxzOW0ukxD5UNjc07Osi1tLr/9cyVBv.pva", null, "Admin", "Admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: new Guid("d473c74f-c281-492b-950f-1be962ed064f"));

            migrationBuilder.RenameColumn(
                name: "ItemCount",
                table: "Shipments",
                newName: "Items");

            migrationBuilder.AddColumn<string>(
                name: "Carrier",
                table: "Shipments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ShipmentDate",
                table: "Shipments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "CreatedAt", "Email", "HashedPassword", "ManagerID", "Role", "UserName" },
                values: new object[] { new Guid("26549bc6-c007-403b-aa00-0b7574b8047c"), new DateTime(2025, 4, 20, 16, 33, 18, 712, DateTimeKind.Utc).AddTicks(3344), "admin@gmail.com", "$2a$11$yV2yDZoNpmJdYQDiiwDiCe1gAWSVqJ3Rf06hSQjHREpUMTAPDx2Ua", null, "Admin", "Admin" });
        }
    }
}
