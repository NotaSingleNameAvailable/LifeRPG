using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeRPG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLastCompletedDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastCompletedDate",
                table: "Tasks",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastCompletedDate",
                table: "Tasks");
        }
    }
}
