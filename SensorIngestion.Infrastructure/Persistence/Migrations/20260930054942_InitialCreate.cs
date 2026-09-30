using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorIngestion.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alert",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RuleId = table.Column<string>(type: "TEXT", nullable: false),
                    DeviceId = table.Column<string>(type: "TEXT", nullable: false),
                    Metric = table.Column<string>(type: "TEXT", nullable: false),
                    StartTs = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    EndTs = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    PeakValue = table.Column<double>(type: "REAL", nullable: false),
                    CreationDateTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    DbEntryDateTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alert", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rule",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RuleId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    Metric = table.Column<string>(type: "TEXT", nullable: false),
                    DeviceId = table.Column<string>(type: "TEXT", nullable: true),
                    Operator = table.Column<byte>(type: "INTEGER", nullable: false),
                    Threshold = table.Column<double>(type: "REAL", nullable: true),
                    MinValue = table.Column<double>(type: "REAL", nullable: true),
                    MaxValue = table.Column<double>(type: "REAL", nullable: true),
                    DurationSeconds = table.Column<int>(type: "INTEGER", nullable: true),
                    CreationDateTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    DbEntryDateTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rule", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SensorReading",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceId = table.Column<string>(type: "TEXT", nullable: false),
                    Metric = table.Column<string>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Sequence = table.Column<long>(type: "INTEGER", nullable: false),
                    Value = table.Column<double>(type: "REAL", nullable: false),
                    Classification = table.Column<byte>(type: "INTEGER", nullable: false),
                    CreationDateTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    DbEntryDateTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorReading", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReadingViolation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReadingId = table.Column<long>(type: "INTEGER", nullable: false),
                    RuleId = table.Column<string>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: false),
                    CreationDateTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    DbEntryDateTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReadingViolation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReadingViolation_SensorReading_ReadingId",
                        column: x => x.ReadingId,
                        principalTable: "SensorReading",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alert_NaturalKey",
                table: "Alert",
                columns: new[] { "RuleId", "DeviceId", "Metric", "StartTs" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReadingViolation_Reading_Rule",
                table: "ReadingViolation",
                columns: new[] { "ReadingId", "RuleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReadingViolation_RuleId",
                table: "ReadingViolation",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Rule_RuleId",
                table: "Rule",
                column: "RuleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reading_NaturalKey",
                table: "SensorReading",
                columns: new[] { "DeviceId", "Metric", "Timestamp", "Sequence" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alert");

            migrationBuilder.DropTable(
                name: "ReadingViolation");

            migrationBuilder.DropTable(
                name: "Rule");

            migrationBuilder.DropTable(
                name: "SensorReading");
        }
    }
}
