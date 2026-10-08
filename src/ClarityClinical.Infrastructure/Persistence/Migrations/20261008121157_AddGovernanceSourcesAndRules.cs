using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClarityClinical.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGovernanceSourcesAndRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "governance");

            migrationBuilder.CreateTable(
                name: "guideline_source",
                schema: "governance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Organisation = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Url = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    PublishedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    LastUpdatedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    LastReviewedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ReviewStatus = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guideline_source", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "clinical_rule_version",
                schema: "governance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Version = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ScoreType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IsIllustrative = table.Column<bool>(type: "boolean", nullable: false),
                    ProvenanceNote = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    GuidelineSourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clinical_rule_version", x => x.Id);
                    table.ForeignKey(
                        name: "FK_clinical_rule_version_guideline_source_GuidelineSourceId",
                        column: x => x.GuidelineSourceId,
                        principalSchema: "governance",
                        principalTable: "guideline_source",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_clinical_rule_version_GuidelineSourceId",
                schema: "governance",
                table: "clinical_rule_version",
                column: "GuidelineSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_clinical_rule_version_RuleKey_Version",
                schema: "governance",
                table: "clinical_rule_version",
                columns: new[] { "RuleKey", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_guideline_source_ReferenceCode",
                schema: "governance",
                table: "guideline_source",
                column: "ReferenceCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clinical_rule_version",
                schema: "governance");

            migrationBuilder.DropTable(
                name: "guideline_source",
                schema: "governance");
        }
    }
}
