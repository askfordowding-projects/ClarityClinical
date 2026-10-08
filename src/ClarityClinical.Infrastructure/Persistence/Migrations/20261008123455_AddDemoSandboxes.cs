using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClarityClinical.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDemoSandboxes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "demo_sandbox",
                schema: "admin",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    visitor_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    source_scenario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sandbox_scenario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sandbox_patient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_demo_sandbox", x => x.id);
                    table.ForeignKey(
                        name: "FK_demo_sandbox_demo_scenario_sandbox_scenario_id",
                        column: x => x.sandbox_scenario_id,
                        principalSchema: "admin",
                        principalTable: "demo_scenario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_demo_sandbox_demo_scenario_source_scenario_id",
                        column: x => x.source_scenario_id,
                        principalSchema: "admin",
                        principalTable: "demo_scenario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_demo_sandbox_patient_sandbox_patient_id",
                        column: x => x.sandbox_patient_id,
                        principalSchema: "patients",
                        principalTable: "patient",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_demo_sandbox_sandbox_patient_id",
                schema: "admin",
                table: "demo_sandbox",
                column: "sandbox_patient_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_demo_sandbox_sandbox_scenario_id",
                schema: "admin",
                table: "demo_sandbox",
                column: "sandbox_scenario_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_demo_sandbox_source_scenario_id",
                schema: "admin",
                table: "demo_sandbox",
                column: "source_scenario_id");

            migrationBuilder.CreateIndex(
                name: "IX_demo_sandbox_visitor_id_source_scenario_id",
                schema: "admin",
                table: "demo_sandbox",
                columns: new[] { "visitor_id", "source_scenario_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "demo_sandbox",
                schema: "admin");
        }
    }
}
