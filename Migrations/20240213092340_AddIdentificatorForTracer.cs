using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestSyncProg.Migrations
{
    public partial class AddIdentificatorForTracer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UniqueUserId",
                table: "EntityTracker",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UniqueUserId",
                table: "EntityTracker");
        }
    }
}
