using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DucommForge.Migrations
{
    /// <inheritdoc />
    public partial class InitSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "DispatchCenters",
                columns: table => new
                {
                    DispatchCenterId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispatchCenters", x => x.DispatchCenterId);
                });

            migrationBuilder.CreateTable(
                name: "Agencies",
                columns: table => new
                {
                    AgencyId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DispatchCenterId = table.Column<int>(type: "INTEGER", nullable: false),
                    Short = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Owned = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agencies", x => x.AgencyId);
                    table.ForeignKey(
                        name: "FK_Agencies_DispatchCenters_DispatchCenterId",
                        column: x => x.DispatchCenterId,
                        principalTable: "DispatchCenters",
                        principalColumn: "DispatchCenterId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Stations",
                columns: table => new
                {
                    StationKey = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AgencyId = table.Column<int>(type: "INTEGER", nullable: false),
                    StationId = table.Column<string>(type: "TEXT", nullable: false),
                    Esz = table.Column<string>(type: "TEXT", nullable: true),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stations", x => x.StationKey);
                    table.ForeignKey(
                        name: "FK_Stations_Agencies_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "Agencies",
                        principalColumn: "AgencyId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Units",
                columns: table => new
                {
                    UnitKey = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StationKey = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitId = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Jump = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Units", x => x.UnitKey);
                    table.ForeignKey(
                        name: "FK_Units_Stations_StationKey",
                        column: x => x.StationKey,
                        principalTable: "Stations",
                        principalColumn: "StationKey",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agencies_DispatchCenterId",
                table: "Agencies",
                column: "DispatchCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_Agencies_DispatchCenterId_Short",
                table: "Agencies",
                columns: new[] { "DispatchCenterId", "Short" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DispatchCenters_Code",
                table: "DispatchCenters",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stations_AgencyId",
                table: "Stations",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Stations_AgencyId_StationId",
                table: "Stations",
                columns: new[] { "AgencyId", "StationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Units_StationKey",
                table: "Units",
                column: "StationKey");

            migrationBuilder.CreateIndex(
                name: "IX_Units_StationKey_UnitId",
                table: "Units",
                columns: new[] { "StationKey", "UnitId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "Units");

            migrationBuilder.DropTable(
                name: "Stations");

            migrationBuilder.DropTable(
                name: "Agencies");

            migrationBuilder.DropTable(
                name: "DispatchCenters");
        }
    }
}
