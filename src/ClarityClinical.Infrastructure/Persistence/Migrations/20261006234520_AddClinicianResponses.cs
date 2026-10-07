using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClarityClinical.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicianResponses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clinician_response",
                schema: "clinical",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    consultation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recommendation_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    response_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    rationale = table.Column<string>(type: "text", nullable: true),
                    modified_action = table.Column<string>(type: "text", nullable: true),
                    responded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    actor_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    actor_role = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clinician_response", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_clinician_response_consultation_id_recommendation_key_respo~",
                schema: "clinical",
                table: "clinician_response",
                columns: new[] { "consultation_id", "recommendation_key", "responded_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clinician_response",
                schema: "clinical");
        }
    }
}
