using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestSyncProg.Migrations
{
    public partial class AddUniqueConstraint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UniqueUserId",
                table: "EntityTracker",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_EntityTracker_UniqueUserId_LocalId",
                table: "EntityTracker",
                columns: new[] { "UniqueUserId", "LocalId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EntityTracker_UniqueUserId_LocalId",
                table: "EntityTracker");

            migrationBuilder.AlterColumn<string>(
                name: "UniqueUserId",
                table: "EntityTracker",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
