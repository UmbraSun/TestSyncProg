using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestSyncProg.Migrations
{
    public partial class ChangeConstraintForTracker : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TrackerConstraints",
                table: "TrackerConstraints");

            migrationBuilder.DropIndex(
                name: "IX_TrackerConstraints_TrackerId",
                table: "TrackerConstraints");

            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "TrackerConstraints",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TrackerConstraints",
                table: "TrackerConstraints",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_TrackerConstraints_LocalId_UniqueUserId",
                table: "TrackerConstraints",
                columns: new[] { "LocalId", "UniqueUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrackerConstraints_TrackerId",
                table: "TrackerConstraints",
                column: "TrackerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TrackerConstraints",
                table: "TrackerConstraints");

            migrationBuilder.DropIndex(
                name: "IX_TrackerConstraints_LocalId_UniqueUserId",
                table: "TrackerConstraints");

            migrationBuilder.DropIndex(
                name: "IX_TrackerConstraints_TrackerId",
                table: "TrackerConstraints");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "TrackerConstraints");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TrackerConstraints",
                table: "TrackerConstraints",
                columns: new[] { "LocalId", "UniqueUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_TrackerConstraints_TrackerId",
                table: "TrackerConstraints",
                column: "TrackerId",
                unique: true);
        }
    }
}
