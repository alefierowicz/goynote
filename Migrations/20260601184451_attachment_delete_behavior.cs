using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace goynote.Migrations
{
    /// <inheritdoc />
    public partial class attachment_delete_behavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Notes_NoteID",
                table: "Attachments");

            migrationBuilder.AlterColumn<Guid>(
                name: "NoteID",
                table: "Attachments",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Notes_NoteID",
                table: "Attachments",
                column: "NoteID",
                principalTable: "Notes",
                principalColumn: "ID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Notes_NoteID",
                table: "Attachments");

            migrationBuilder.AlterColumn<Guid>(
                name: "NoteID",
                table: "Attachments",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Notes_NoteID",
                table: "Attachments",
                column: "NoteID",
                principalTable: "Notes",
                principalColumn: "ID");
        }
    }
}
