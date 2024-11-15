using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscountGenerator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUsedAtColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UsedAt",
                table: "DiscountCodes",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsedAt",
                table: "DiscountCodes");
        }
    }
}
