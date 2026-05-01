using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RiskAnalyzer.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMLSeverityWeightAndRiskMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SeverityWeight",
                table: "ImageAnalyses",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "Severity weight for risk category (0-10 scale)");

            migrationBuilder.AddColumn<string>(
                name: "RiskAction",
                table: "ImageAnalyses",
                type: "longtext",
                nullable: false,
                defaultValue: "Review",
                comment: "Action to take: Auto-Block, Review, Allow");

            migrationBuilder.AddColumn<string>(
                name: "RiskColor",
                table: "ImageAnalyses",
                type: "longtext",
                nullable: false,
                defaultValue: "yellow",
                comment: "UI color code: green (safe), yellow (warning), red (danger)");

            // Convert existing int RiskScore and ConfidenceScore to float
            migrationBuilder.AlterColumn<float>(
                name: "ConfidenceScore",
                table: "ImageAnalyses",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<float>(
                name: "RiskScore",
                table: "ImageAnalyses",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SeverityWeight",
                table: "ImageAnalyses");

            migrationBuilder.DropColumn(
                name: "RiskAction",
                table: "ImageAnalyses");

            migrationBuilder.DropColumn(
                name: "RiskColor",
                table: "ImageAnalyses");

            migrationBuilder.AlterColumn<int>(
                name: "ConfidenceScore",
                table: "ImageAnalyses",
                type: "int",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<int>(
                name: "RiskScore",
                table: "ImageAnalyses",
                type: "int",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");
        }
    }
}
