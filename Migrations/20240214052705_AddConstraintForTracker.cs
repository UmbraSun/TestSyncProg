using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestSyncProg.Migrations
{
    public partial class AddConstraintForTracker : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrackerConstraints",
                columns: table => new
                {
                    LocalId = table.Column<long>(type: "bigint", nullable: false),
                    UniqueUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TrackerId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackerConstraints", x => new { x.LocalId, x.UniqueUserId });
                    table.ForeignKey(
                        name: "FK_TrackerConstraints_EntityTracker_TrackerId",
                        column: x => x.TrackerId,
                        principalTable: "EntityTracker",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrackerConstraints_TrackerId",
                table: "TrackerConstraints",
                column: "TrackerId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrackerConstraints");
        }
    }
}
