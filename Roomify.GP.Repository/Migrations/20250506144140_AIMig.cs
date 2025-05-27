using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Roomify.GP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AIMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Descriptions");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "RoomImages");

            migrationBuilder.DropColumn(
                name: "Length",
                table: "RoomImages");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "RoomImages");

            migrationBuilder.CreateTable(
                name: "AIResultHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GeneratedImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIResultHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIResultHistories_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AIResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GeneratedImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SavedDesigns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GeneratedImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SavedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedDesigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedDesigns_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prompts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DescriptionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoomStyle = table.Column<int>(type: "int", nullable: false),
                    RoomType = table.Column<int>(type: "int", nullable: false),
                    RoomImageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AIResultHistoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prompts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prompts_AIResultHistories_AIResultHistoryId",
                        column: x => x.AIResultHistoryId,
                        principalTable: "AIResultHistories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Prompts_RoomImages_RoomImageId",
                        column: x => x.RoomImageId,
                        principalTable: "RoomImages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIResultHistories_ApplicationUserId",
                table: "AIResultHistories",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Prompts_AIResultHistoryId",
                table: "Prompts",
                column: "AIResultHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Prompts_RoomImageId",
                table: "Prompts",
                column: "RoomImageId",
                unique: true,
                filter: "[RoomImageId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SavedDesigns_ApplicationUserId",
                table: "SavedDesigns",
                column: "ApplicationUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIResults");

            migrationBuilder.DropTable(
                name: "Prompts");

            migrationBuilder.DropTable(
                name: "SavedDesigns");

            migrationBuilder.DropTable(
                name: "AIResultHistories");

            migrationBuilder.AddColumn<double>(
                name: "Height",
                table: "RoomImages",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Length",
                table: "RoomImages",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Width",
                table: "RoomImages",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "Descriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomImageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DescriptionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoomStyle = table.Column<int>(type: "int", nullable: false),
                    RoomType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Descriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Descriptions_RoomImages_RoomImageId",
                        column: x => x.RoomImageId,
                        principalTable: "RoomImages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Descriptions_RoomImageId",
                table: "Descriptions",
                column: "RoomImageId",
                unique: true);
        }
    }
}
