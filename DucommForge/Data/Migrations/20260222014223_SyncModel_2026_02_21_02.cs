using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DucommForge.Data.Migrations
{
    /// <inheritdoc />
    public partial class SyncModel_2026_02_21_02 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Units_Stations_StationKey1",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_Units_StationKey_UnitId",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_Units_StationKey1",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_Stations_AgencyId_StationId",
                table: "Stations");

            migrationBuilder.DropIndex(
                name: "IX_Agencies_DispatchCenterId_Short",
                table: "Agencies");

            migrationBuilder.DropColumn(
                name: "StationKey1",
                table: "Units");

            migrationBuilder.CreateIndex(
                name: "IX_Units_StationKey",
                table: "Units",
                column: "StationKey");

            migrationBuilder.CreateIndex(
                name: "IX_Stations_AgencyId",
                table: "Stations",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Agencies_DispatchCenterId",
                table: "Agencies",
                column: "DispatchCenterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Units_Stations_StationKey",
                table: "Units",
                column: "StationKey",
                principalTable: "Stations",
                principalColumn: "StationKey",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropIndex(
                name: "IX_Agencies_DispatchCenterId",
                table: "Agencies");

            migrationBuilder.AddColumn<int>(
                name: "StationKey1",
                table: "Units",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Units_StationKey_UnitId",
                table: "Units",
                columns: new[] { "StationKey", "UnitId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Units_StationKey1",
                table: "Units",
                column: "StationKey1");

            migrationBuilder.CreateIndex(
                name: "IX_Stations_AgencyId_StationId",
                table: "Stations",
                columns: new[] { "AgencyId", "StationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Agencies_DispatchCenterId_Short",
                table: "Agencies",
                columns: new[] { "DispatchCenterId", "Short" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Units_Stations_StationKey1",
                table: "Units",
                column: "StationKey1",
                principalTable: "Stations",
                principalColumn: "StationKey");
        }
    }
}
