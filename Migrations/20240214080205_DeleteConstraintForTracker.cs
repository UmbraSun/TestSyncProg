using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestSyncProg.Migrations
{
    public partial class DeleteConstraintForTracker : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrackerConstraints");

            migrationBuilder.AddColumn<long>(
                name: "LocalId",
                table: "EntityTracker",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocalId",
                table: "EntityTracker");

            migrationBuilder.CreateTable(
                name: "TrackerConstraints",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrackerId = table.Column<long>(type: "bigint", nullable: false),
                    LocalId = table.Column<long>(type: "bigint", nullable: false),
                    UniqueUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackerConstraints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackerConstraints_EntityTracker_TrackerId",
                        column: x => x.TrackerId,
                        principalTable: "EntityTracker",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
    }
}
