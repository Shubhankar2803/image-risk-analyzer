using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RiskAnalyzer.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRiskCategoriesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RiskCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CategoryName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SeverityWeight = table.Column<int>(type: "int", nullable: false),
                    ActionThreshold = table.Column<float>(type: "float", nullable: false),
                    RecommendedAction = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, defaultValue: "Review")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskCategories", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            // Create unique index on CategoryName
            migrationBuilder.CreateIndex(
                name: "IX_RiskCategories_CategoryName",
                table: "RiskCategories",
                column: "CategoryName",
                unique: true);

            // Create index on IsEnabled for filtering
            migrationBuilder.CreateIndex(
                name: "IX_RiskCategories_IsEnabled",
                table: "RiskCategories",
                column: "IsEnabled");

            // Seed initial risk categories
            migrationBuilder.InsertData(
                table: "RiskCategories",
                columns: new[] { "CategoryName", "SeverityWeight", "ActionThreshold", "RecommendedAction", "Description", "IsEnabled", "UpdatedAt" },
                values: new object[,]
                {
                    { "Safe", 0, 1.0f, "No Action", "Control group - safe content", true, DateTime.UtcNow },
                    { "Explicit_Porn", 10, 0.6f, "Auto-Block", "High legal/policy risk - sexually explicit content", true, DateTime.UtcNow },
                    { "Violence_gore", 10, 0.7f, "Auto-Block", "Triggers harm policies - graphic violence and gore", true, DateTime.UtcNow },
                    { "Hate_Symbols", 10, 0.65f, "Auto-Block", "High platform de-platforming risk - hate symbols", true, DateTime.UtcNow },
                    { "Softporn", 9, 0.7f, "Block + Review", "Sexually suggestive content - moderate risk", true, DateTime.UtcNow },
                    { "Weapons", 7, 0.75f, "Review", "Could be legitimate (movie poster) or threat - nuanced", true, DateTime.UtcNow },
                    { "Hentai", 6, 0.8f, "Review", "Animated explicit content - platform dependent", true, DateTime.UtcNow },
                    { "Sensitive_Documents", 6, 0.5f, "Review", "May contain PII or confidential information", true, DateTime.UtcNow },
                    { "VIolence_gore", 4, 0.85f, "Warning", "Realistic but non-graphic violence - lower risk", true, DateTime.UtcNow }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RiskCategories");
        }
    }
}
