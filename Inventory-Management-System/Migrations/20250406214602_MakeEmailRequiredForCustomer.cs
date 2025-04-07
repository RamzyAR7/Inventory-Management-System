using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class MakeEmailRequiredForCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: new Guid("d22766c8-63cf-4727-9f6a-2ffbe7d3fd5b"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "CreatedAt", "Email", "FullName", "PasswordHash", "Role" },
                values: new object[] { new Guid("160ade4a-1e3c-40b0-9eed-e6d56e125739"), new DateTime(2025, 4, 6, 21, 46, 0, 701, DateTimeKind.Utc).AddTicks(6141), "admin@gmail.com", "Admin", "$2a$11$hdS5IWB9il2h1qpEmlPTNuB553EnPXgPCks..kqZ4H4cIsLD/Zg92", "Admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: new Guid("160ade4a-1e3c-40b0-9eed-e6d56e125739"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "CreatedAt", "Email", "FullName", "PasswordHash", "Role" },
                values: new object[] { new Guid("d22766c8-63cf-4727-9f6a-2ffbe7d3fd5b"), new DateTime(2025, 4, 5, 2, 4, 48, 110, DateTimeKind.Utc).AddTicks(8892), "admin@example.com", "Admin", "$2a$11$1ZxuzuX0/PgVv4D6DMq6Qe/PohrgbHF5orrxZ34YuCc42x8Rs/Eiq", "Admin" });
        }
    }
}
