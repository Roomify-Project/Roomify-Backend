using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Roomify.GP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class PendingRegistrationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailConfirmationTokens_AspNetUsers_UserId",
                table: "EmailConfirmationTokens");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "EmailConfirmationTokens",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "PendingRegistrationId",
                table: "EmailConfirmationTokens",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PendingRegistrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Roles = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingRegistrations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailConfirmationTokens_PendingRegistrationId",
                table: "EmailConfirmationTokens",
                column: "PendingRegistrationId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailConfirmationTokens_AspNetUsers_UserId",
                table: "EmailConfirmationTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailConfirmationTokens_PendingRegistrations_PendingRegistrationId",
                table: "EmailConfirmationTokens",
                column: "PendingRegistrationId",
                principalTable: "PendingRegistrations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailConfirmationTokens_AspNetUsers_UserId",
                table: "EmailConfirmationTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailConfirmationTokens_PendingRegistrations_PendingRegistrationId",
                table: "EmailConfirmationTokens");

            migrationBuilder.DropTable(
                name: "PendingRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_EmailConfirmationTokens_PendingRegistrationId",
                table: "EmailConfirmationTokens");

            migrationBuilder.DropColumn(
                name: "PendingRegistrationId",
                table: "EmailConfirmationTokens");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "EmailConfirmationTokens",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailConfirmationTokens_AspNetUsers_UserId",
                table: "EmailConfirmationTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
