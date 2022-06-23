using Microsoft.EntityFrameworkCore.Migrations;

namespace Api.Migrations
{
    public partial class AddLocationDetectorNavigation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MacAddress",
                table: "Detectors",
                type: "varchar(20)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "Detectors",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Detectors_LocationId",
                table: "Detectors",
                column: "LocationId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Detectors_Locations_LocationId",
                table: "Detectors",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Detectors_Locations_LocationId",
                table: "Detectors");

            migrationBuilder.DropIndex(
                name: "IX_Detectors_LocationId",
                table: "Detectors");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "Detectors");

            migrationBuilder.AlterColumn<string>(
                name: "MacAddress",
                table: "Detectors",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
