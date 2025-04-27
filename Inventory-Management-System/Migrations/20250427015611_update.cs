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
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_ManagerID",
                table: "Users");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_ManagerID",
                table: "Users",
                column: "ManagerID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: new Guid("4c5132b9-0af3-4af8-9945-abd39c3a1db5"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "CreatedAt", "Email", "HashedPassword", "ManagerID", "Role", "UserName" },
                values: new object[] { new Guid("b19f2e7d-e001-49ad-a56f-e1061366d389"), new DateTime(2025, 4, 27, 1, 56, 10, 71, DateTimeKind.Utc).AddTicks(9641), "admin@gmail.com", "$2a$11$zhRJJ1aDAtClpNPs.Aici.AWRk8wrbEcduNnQkBaMMvn8mAZgFE5K", null, "Admin", "Admin" });
        }

        /// <inheritdoc />


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_ManagerID",
                table: "Users");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_ManagerID",
                table: "Users",
                column: "ManagerID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: new Guid("b19f2e7d-e001-49ad-a56f-e1061366d389"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "CreatedAt", "Email", "HashedPassword", "ManagerID", "Role", "UserName" },
                values: new object[] { new Guid("4c5132b9-0af3-4af8-9945-abd39c3a1db5"), new DateTime(2025, 4, 26, 17, 3, 22, 717, DateTimeKind.Utc).AddTicks(5486), "admin@gmail.com", "$2a$11$gwVA017LnrYw6uNTswArpu23w8Jy951gr.ZDyln8WDEc/nhiuSHly", null, "Admin", "Admin" });
        }
    }
}
