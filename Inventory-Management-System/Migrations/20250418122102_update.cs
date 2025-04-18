using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: new Guid("2b8af87b-c765-410e-bd14-2ebbc030053a"));

            migrationBuilder.RenameColumn(
                name: "EncryptedPassword",
                table: "Users",
                newName: "HashedPassword");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "CreatedAt", "Email", "HashedPassword", "ManagerID", "Role", "UserName" },
                values: new object[] { new Guid("1fd389ff-7287-4969-bbf7-27ef27fb8320"), new DateTime(2025, 4, 18, 12, 21, 0, 777, DateTimeKind.Utc).AddTicks(9716), "admin@gmail.com", "$2a$11$qFhkJKqm1Yfca5t4mDEVfe75.4OUHgjkcz3rLnfuoX03OK/wx5ziW", null, "Admin", "Admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: new Guid("1fd389ff-7287-4969-bbf7-27ef27fb8320"));

            migrationBuilder.RenameColumn(
                name: "HashedPassword",
                table: "Users",
                newName: "EncryptedPassword");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "CreatedAt", "Email", "EncryptedPassword", "ManagerID", "Role", "UserName" },
                values: new object[] { new Guid("2b8af87b-c765-410e-bd14-2ebbc030053a"), new DateTime(2025, 4, 12, 13, 21, 25, 739, DateTimeKind.Utc).AddTicks(6784), "admin@gmail.com", "OHQCCSALwuReYqVzEhwlBw==", null, "Admin", "Admin" });
        }
    }
}
