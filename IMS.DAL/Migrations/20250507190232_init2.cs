using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class init2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: new Guid("f73f60de-498c-4bdd-bb3e-bfb8d2d386ba"));

            migrationBuilder.AlterColumn<string>(
                name: "DeliveryPhoneNumber",
                table: "Shipments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeliveryName",
                table: "Shipments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "CreatedAt", "Email", "HashedPassword", "IsActive", "ManagerID", "Role", "UserName" },
                values: new object[] { new Guid("e2855c6c-a199-4c49-a312-d38486d42eaf"), new DateTime(2025, 5, 7, 19, 2, 31, 280, DateTimeKind.Utc).AddTicks(569), "admin@gmail.com", "$2a$11$Jzw4V87jTBcLLVSbQ0AdKu474sdR9ofOtXKCa8ikQJUkAvcTidhri", true, null, "Admin", "Admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: new Guid("e2855c6c-a199-4c49-a312-d38486d42eaf"));

            migrationBuilder.AlterColumn<string>(
                name: "DeliveryPhoneNumber",
                table: "Shipments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "DeliveryName",
                table: "Shipments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "CreatedAt", "Email", "HashedPassword", "IsActive", "ManagerID", "Role", "UserName" },
                values: new object[] { new Guid("f73f60de-498c-4bdd-bb3e-bfb8d2d386ba"), new DateTime(2025, 5, 3, 3, 1, 45, 525, DateTimeKind.Utc).AddTicks(4933), "admin@gmail.com", "$2a$11$UhWrBq3nHRfWfBeK9D43OuM8O1bzeit7WdsNKNdUJYA4lEpktp4.K", true, null, "Admin", "Admin" });
        }
    }
}
