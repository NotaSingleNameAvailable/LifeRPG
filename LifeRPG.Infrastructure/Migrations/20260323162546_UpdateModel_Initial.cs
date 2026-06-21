using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeRPG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModel_Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalXP",
                table: "Users",
                newName: "LifePoints");

            migrationBuilder.CreateTable(
                name: "Characters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Cid = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Emoji = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserCharacterProgress",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    XP = table.Column<int>(type: "integer", nullable: false),
                    IsUnlocked = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCharacterProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCharacterProgress_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCharacterProgress_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserCharacterProgress_CharacterId",
                table: "UserCharacterProgress",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCharacterProgress_UserId",
                table: "UserCharacterProgress",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserCharacterProgress");

            migrationBuilder.DropTable(
                name: "Characters");

            migrationBuilder.RenameColumn(
                name: "LifePoints",
                table: "Users",
                newName: "TotalXP");
        }
    }
}
