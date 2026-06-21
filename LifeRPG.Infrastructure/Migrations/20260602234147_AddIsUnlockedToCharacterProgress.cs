using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeRPG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsUnlockedToCharacterProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ActiveCharacterId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnlockLevel",
                table: "Characters",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveCharacterId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UnlockLevel",
                table: "Characters");
        }
    }
}
