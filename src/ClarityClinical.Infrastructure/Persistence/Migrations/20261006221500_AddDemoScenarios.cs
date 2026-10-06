using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClarityClinical.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDemoScenarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "admin");

            migrationBuilder.CreateTable(
                name: "demo_scenario",
                schema: "admin",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    scenario_key = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    patient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    setting = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    source_language = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    clinician_language = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    is_canonical = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_demo_scenario", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "demo_scenario_step",
                schema: "admin",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    scenario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequence = table.Column<int>(type: "integer", nullable: false),
                    step_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    clinical_fact_code = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    clinical_fact_display_text = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    original_text = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    source_language = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_demo_scenario_step", x => x.id);
                    table.ForeignKey(
                        name: "FK_demo_scenario_step_demo_scenario_scenario_id",
                        column: x => x.scenario_id,
                        principalSchema: "admin",
                        principalTable: "demo_scenario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_demo_scenario_scenario_key",
                schema: "admin",
                table: "demo_scenario",
                column: "scenario_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_demo_scenario_step_scenario_id_sequence",
                schema: "admin",
                table: "demo_scenario_step",
                columns: new[] { "scenario_id", "sequence" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "demo_scenario_step",
                schema: "admin");

            migrationBuilder.DropTable(
                name: "demo_scenario",
                schema: "admin");
        }
    }
}
