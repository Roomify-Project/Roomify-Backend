using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Roomify.GP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Updates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AIResultHistories_AspNetUsers_ApplicationUserId1",
                table: "AIResultHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_SavedDesigns_AspNetUsers_ApplicationUserId1",
                table: "SavedDesigns");

            migrationBuilder.DropIndex(
                name: "IX_SavedDesigns_ApplicationUserId1",
                table: "SavedDesigns");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId1",
                table: "SavedDesigns");

            migrationBuilder.RenameColumn(
                name: "GeneratedImageUrl",
                table: "AIResultHistories",
                newName: "generatedImageUrl");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId1",
                table: "AIResultHistories",
                newName: "AIResultId");

            migrationBuilder.RenameIndex(
                name: "IX_AIResultHistories_ApplicationUserId1",
                table: "AIResultHistories",
                newName: "IX_AIResultHistories_AIResultId");

            migrationBuilder.AlterColumn<Guid>(
                name: "ApplicationUserId",
                table: "SavedDesigns",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "AIResults",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<Guid>(
                name: "ApplicationUserId",
                table: "AIResultHistories",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_SavedDesigns_ApplicationUserId",
                table: "SavedDesigns",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AIResultHistories_ApplicationUserId",
                table: "AIResultHistories",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AIResultHistories_AIResults_AIResultId",
                table: "AIResultHistories",
                column: "AIResultId",
                principalTable: "AIResults",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AIResultHistories_AspNetUsers_ApplicationUserId",
                table: "AIResultHistories",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SavedDesigns_AspNetUsers_ApplicationUserId",
                table: "SavedDesigns",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AIResultHistories_AIResults_AIResultId",
                table: "AIResultHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_AIResultHistories_AspNetUsers_ApplicationUserId",
                table: "AIResultHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_SavedDesigns_AspNetUsers_ApplicationUserId",
                table: "SavedDesigns");

            migrationBuilder.DropIndex(
                name: "IX_SavedDesigns_ApplicationUserId",
                table: "SavedDesigns");

            migrationBuilder.DropIndex(
                name: "IX_AIResultHistories_ApplicationUserId",
                table: "AIResultHistories");

            migrationBuilder.RenameColumn(
                name: "generatedImageUrl",
                table: "AIResultHistories",
                newName: "GeneratedImageUrl");

            migrationBuilder.RenameColumn(
                name: "AIResultId",
                table: "AIResultHistories",
                newName: "ApplicationUserId1");

            migrationBuilder.RenameIndex(
                name: "IX_AIResultHistories_AIResultId",
                table: "AIResultHistories",
                newName: "IX_AIResultHistories_ApplicationUserId1");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "SavedDesigns",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationUserId1",
                table: "SavedDesigns",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "AIResults",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "AIResultHistories",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_SavedDesigns_ApplicationUserId1",
                table: "SavedDesigns",
                column: "ApplicationUserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AIResultHistories_AspNetUsers_ApplicationUserId1",
                table: "AIResultHistories",
                column: "ApplicationUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SavedDesigns_AspNetUsers_ApplicationUserId1",
                table: "SavedDesigns",
                column: "ApplicationUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
