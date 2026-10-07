using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClarityClinical.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTranscriptSegments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "transcript_segment",
                schema: "consultations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    consultation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    speaker_role = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    source_language = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    machine_original_text = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    corrected_text = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    translated_text = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    recognition_confidence = table.Column<double>(type: "double precision", nullable: true),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    include_in_reasoning = table.Column<bool>(type: "boolean", nullable: false),
                    is_redacted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transcript_segment", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_transcript_segment_consultation_id_occurred_at",
                schema: "consultations",
                table: "transcript_segment",
                columns: new[] { "consultation_id", "occurred_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transcript_segment",
                schema: "consultations");
        }
    }
}
