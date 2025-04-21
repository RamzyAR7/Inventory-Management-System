using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class EditeAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customers_FullName",
                table: "Customers");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: new Guid("05a935e4-b3e3-47eb-97bd-8d57a82e98a8"));

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Customers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "CreatedAt", "Email", "HashedPassword", "ManagerID", "Role", "UserName" },
                values: new object[] { new Guid("b24a9a56-546c-47b7-aefd-1f58faa5df6b"), new DateTime(2025, 4, 20, 13, 11, 21, 949, DateTimeKind.Utc).AddTicks(6037), "admin@gmail.com", "$2a$11$gOIliDBTgbCm2BpP3oIeOOmw5vG8gVoKi8h6vYpPEqzfHE2skEU9O", null, "Admin", "Admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: new Guid("b24a9a56-546c-47b7-aefd-1f58faa5df6b"));

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Customers",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "CreatedAt", "Email", "HashedPassword", "ManagerID", "Role", "UserName" },
                values: new object[] { new Guid("05a935e4-b3e3-47eb-97bd-8d57a82e98a8"), new DateTime(2025, 4, 19, 14, 44, 0, 175, DateTimeKind.Utc).AddTicks(7597), "admin@gmail.com", "$2a$11$hfw/HV5k20wJv3l1LguCnOmBYkILy5Krq.01auPyHIljGGtL308z6", null, "Admin", "Admin" });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_FullName",
                table: "Customers",
                column: "FullName",
                unique: true);
        }
    }
}
