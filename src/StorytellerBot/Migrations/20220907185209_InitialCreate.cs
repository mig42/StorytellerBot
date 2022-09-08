using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StorytellerBot.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Adventure",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    ScriptFileName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adventure", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommandProgress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Command = table.Column<string>(type: "TEXT", nullable: false),
                    Argument = table.Column<string>(type: "TEXT", nullable: true),
                    Step = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommandProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommandProgress_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SavedStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    AdventureId = table.Column<int>(type: "INTEGER", nullable: false),
                    StoryState = table.Column<string>(type: "TEXT", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedStatus_Adventure_AdventureId",
                        column: x => x.AdventureId,
                        principalTable: "Adventure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SavedStatus_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CurrentGame",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    SavedStatusId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrentGame", x => new { x.UserId, x.SavedStatusId });
                    table.ForeignKey(
                        name: "FK_CurrentGame_SavedStatus_SavedStatusId",
                        column: x => x.SavedStatusId,
                        principalTable: "SavedStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CurrentGame_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommandProgress_UserId",
                table: "CommandProgress",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrentGame_SavedStatusId",
                table: "CurrentGame",
                column: "SavedStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedStatus_AdventureId",
                table: "SavedStatus",
                column: "AdventureId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedStatus_UserId_AdventureId",
                table: "SavedStatus",
                columns: new[] { "UserId", "AdventureId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommandProgress");

            migrationBuilder.DropTable(
                name: "CurrentGame");

            migrationBuilder.DropTable(
                name: "SavedStatus");

            migrationBuilder.DropTable(
                name: "Adventure");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
