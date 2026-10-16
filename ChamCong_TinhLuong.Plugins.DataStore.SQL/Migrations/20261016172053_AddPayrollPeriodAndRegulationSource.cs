using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChamCong_TinhLuong.Plugins.DataStore.SQL.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollPeriodAndRegulationSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RegulationSource",
                table: "DeductionRates",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PayrollPeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    StandardWorkDays = table.Column<decimal>(type: "decimal(4,1)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    LockedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LockedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollPeriods", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPeriods_Month_Year",
                table: "PayrollPeriods",
                columns: new[] { "Month", "Year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayrollPeriods");

            migrationBuilder.DropColumn(
                name: "RegulationSource",
                table: "DeductionRates");
        }
    }
}
