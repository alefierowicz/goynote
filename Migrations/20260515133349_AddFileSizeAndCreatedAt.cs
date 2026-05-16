using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace goynote.Migrations
{
    /// <inheritdoc />
    public partial class AddFileSizeAndCreatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Attachments",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "FileSize",
                table: "Attachments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "Attachments");
        }
    }
}
