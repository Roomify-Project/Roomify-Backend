using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Roomify.GP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class historyFKfix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Prompts_AIResultHistoryId",
                table: "Prompts");

            migrationBuilder.AddColumn<Guid>(
                name: "PromptId",
                table: "AIResultHistories",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Prompts_AIResultHistoryId",
                table: "Prompts",
                column: "AIResultHistoryId",
                unique: true,
                filter: "[AIResultHistoryId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Prompts_AIResultHistoryId",
                table: "Prompts");

            migrationBuilder.DropColumn(
                name: "PromptId",
                table: "AIResultHistories");

            migrationBuilder.CreateIndex(
                name: "IX_Prompts_AIResultHistoryId",
                table: "Prompts",
                column: "AIResultHistoryId");
        }
    }
}
