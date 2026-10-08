using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClarityClinical.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicalFactSourceEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "source_event_id",
                schema: "consultations",
                table: "clinical_event",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_clinical_event_source_event_id",
                schema: "consultations",
                table: "clinical_event",
                column: "source_event_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_clinical_event_source_event_id",
                schema: "consultations",
                table: "clinical_event");

            migrationBuilder.DropColumn(
                name: "source_event_id",
                schema: "consultations",
                table: "clinical_event");
        }
    }
}
