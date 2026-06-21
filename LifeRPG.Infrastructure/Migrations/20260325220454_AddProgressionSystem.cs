using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeRPG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProgressionSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LifePoints",
                table: "Users",
                newName: "TotalLifePoints");

            migrationBuilder.RenameColumn(
                name: "Level",
                table: "Users",
                newName: "LifeLevel");

            migrationBuilder.RenameColumn(
                name: "XP",
                table: "UserCharacterProgress",
                newName: "TotalXP");

            migrationBuilder.AddColumn<int>(
                name: "CurrentLifePoints",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentXP",
                table: "UserCharacterProgress",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "UserCharacterProgress",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentLifePoints",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CurrentXP",
                table: "UserCharacterProgress");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "UserCharacterProgress");

            migrationBuilder.RenameColumn(
                name: "TotalLifePoints",
                table: "Users",
                newName: "LifePoints");

            migrationBuilder.RenameColumn(
                name: "LifeLevel",
                table: "Users",
                newName: "Level");

            migrationBuilder.RenameColumn(
                name: "TotalXP",
                table: "UserCharacterProgress",
                newName: "XP");
        }
    }
}
