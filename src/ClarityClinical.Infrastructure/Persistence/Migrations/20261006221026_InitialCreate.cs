using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClarityClinical.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "clinical");

            migrationBuilder.EnsureSchema(
                name: "consultations");

            migrationBuilder.EnsureSchema(
                name: "patients");

            migrationBuilder.CreateTable(
                name: "clinical_assessment",
                schema: "clinical",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    consultation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    evaluated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clinical_assessment", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "consultation",
                schema: "consultations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    patient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consultation", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "patient",
                schema: "patients",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    given_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    family_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patient", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "assessment_candidate",
                schema: "clinical",
                columns: table => new
                {
                    key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    clinical_assessment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    current_score = table.Column<int>(type: "integer", nullable: false),
                    previous_score = table.Column<int>(type: "integer", nullable: true),
                    score_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assessment_candidate", x => new { x.clinical_assessment_id, x.key });
                    table.ForeignKey(
                        name: "FK_assessment_candidate_clinical_assessment_clinical_assessmen~",
                        column: x => x.clinical_assessment_id,
                        principalSchema: "clinical",
                        principalTable: "clinical_assessment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "clinical_event",
                schema: "consultations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    display_text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    source_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    include_in_reasoning = table.Column<bool>(type: "boolean", nullable: false),
                    consultation_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clinical_event", x => x.id);
                    table.ForeignKey(
                        name: "FK_clinical_event_consultation_consultation_id",
                        column: x => x.consultation_id,
                        principalSchema: "consultations",
                        principalTable: "consultation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "patient_allergy",
                schema: "patients",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    patient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patient_allergy", x => new { x.patient_id, x.code });
                    table.ForeignKey(
                        name: "FK_patient_allergy_patient_patient_id",
                        column: x => x.patient_id,
                        principalSchema: "patients",
                        principalTable: "patient",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "patient_condition",
                schema: "patients",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    patient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patient_condition", x => new { x.patient_id, x.code });
                    table.ForeignKey(
                        name: "FK_patient_condition_patient_patient_id",
                        column: x => x.patient_id,
                        principalSchema: "patients",
                        principalTable: "patient",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "patient_medication",
                schema: "patients",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    patient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patient_medication", x => new { x.patient_id, x.code });
                    table.ForeignKey(
                        name: "FK_patient_medication_patient_patient_id",
                        column: x => x.patient_id,
                        principalSchema: "patients",
                        principalTable: "patient",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_clinical_assessment_consultation_id_evaluated_at",
                schema: "clinical",
                table: "clinical_assessment",
                columns: new[] { "consultation_id", "evaluated_at" });

            migrationBuilder.CreateIndex(
                name: "IX_clinical_event_consultation_id_occurred_at",
                schema: "consultations",
                table: "clinical_event",
                columns: new[] { "consultation_id", "occurred_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assessment_candidate",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "clinical_event",
                schema: "consultations");

            migrationBuilder.DropTable(
                name: "patient_allergy",
                schema: "patients");

            migrationBuilder.DropTable(
                name: "patient_condition",
                schema: "patients");

            migrationBuilder.DropTable(
                name: "patient_medication",
                schema: "patients");

            migrationBuilder.DropTable(
                name: "clinical_assessment",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "consultation",
                schema: "consultations");

            migrationBuilder.DropTable(
                name: "patient",
                schema: "patients");
        }
    }
}
