using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeRPG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HEREREPLACEWITHNEWNAMEV5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AwardedCharacterId",
                table: "Tasks",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AwardedCharacterId",
                table: "Tasks");
        }
    }
}
