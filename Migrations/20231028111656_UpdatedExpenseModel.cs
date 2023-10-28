using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Impensa.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedExpenseModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExpenseCategories_Expenses_ExpenseId",
                table: "ExpenseCategories");

            migrationBuilder.DropIndex(
                name: "IX_ExpenseCategories_ExpenseId",
                table: "ExpenseCategories");

            migrationBuilder.DropColumn(
                name: "ExpenseId",
                table: "ExpenseCategories");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseCategories_UserId",
                table: "ExpenseCategories",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseCategories_Users_UserId",
                table: "ExpenseCategories",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExpenseCategories_Users_UserId",
                table: "ExpenseCategories");

            migrationBuilder.DropIndex(
                name: "IX_ExpenseCategories_UserId",
                table: "ExpenseCategories");

            migrationBuilder.AddColumn<Guid>(
                name: "ExpenseId",
                table: "ExpenseCategories",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseCategories_ExpenseId",
                table: "ExpenseCategories",
                column: "ExpenseId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseCategories_Expenses_ExpenseId",
                table: "ExpenseCategories",
                column: "ExpenseId",
                principalTable: "Expenses",
                principalColumn: "Id");
        }
    }
}
