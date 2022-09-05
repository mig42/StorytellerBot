using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StorytellerBot.Migrations
{
    public partial class SavedGames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SavedStatusJson",
                table: "SavedStatus",
                newName: "StoryState");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Adventure",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CommandProgresses",
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
                    table.PrimaryKey("PK_CommandProgresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CurrentGames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    SavedStatusId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrentGames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurrentGames_SavedStatus_SavedStatusId",
                        column: x => x.SavedStatusId,
                        principalTable: "SavedStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    CurrentGameId = table.Column<int>(type: "INTEGER", nullable: true),
                    CommandProgressId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_CommandProgresses_CommandProgressId",
                        column: x => x.CommandProgressId,
                        principalTable: "CommandProgresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_User_CurrentGames_CurrentGameId",
                        column: x => x.CurrentGameId,
                        principalTable: "CurrentGames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SavedStatus_UserId",
                table: "SavedStatus",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrentGames_SavedStatusId",
                table: "CurrentGames",
                column: "SavedStatusId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_CommandProgressId",
                table: "User",
                column: "CommandProgressId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_CurrentGameId",
                table: "User",
                column: "CurrentGameId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SavedStatus_User_UserId",
                table: "SavedStatus",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SavedStatus_User_UserId",
                table: "SavedStatus");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "CommandProgresses");

            migrationBuilder.DropTable(
                name: "CurrentGames");

            migrationBuilder.DropIndex(
                name: "IX_SavedStatus_UserId",
                table: "SavedStatus");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Adventure");

            migrationBuilder.RenameColumn(
                name: "StoryState",
                table: "SavedStatus",
                newName: "SavedStatusJson");
        }
    }
}
