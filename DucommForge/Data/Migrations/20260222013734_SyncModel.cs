using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DucommForge.Data.Migrations
{
    /// <inheritdoc />
    public partial class SyncModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Units_Stations_StationKey",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_Units_StationKey",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_Stations_AgencyId",
                table: "Stations");

            migrationBuilder.AlterColumn<string>(
                name: "UnitId",
                table: "Units",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<int>(
                name: "StationKey1",
                table: "Units",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Units_StationKey1",
                table: "Units",
                column: "StationKey1");

            migrationBuilder.AddForeignKey(
                name: "FK_Units_Stations_StationKey1",
                table: "Units",
                column: "StationKey1",
                principalTable: "Stations",
                principalColumn: "StationKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Units_Stations_StationKey1",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_Units_StationKey1",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "StationKey1",
                table: "Units");

            migrationBuilder.AlterColumn<string>(
                name: "UnitId",
                table: "Units",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Units_StationKey",
                table: "Units",
                column: "StationKey");

            migrationBuilder.CreateIndex(
                name: "IX_Stations_AgencyId",
                table: "Stations",
                column: "AgencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Units_Stations_StationKey",
                table: "Units",
                column: "StationKey",
                principalTable: "Stations",
                principalColumn: "StationKey",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
