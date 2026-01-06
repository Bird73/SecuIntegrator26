using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecuIntegrator26.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HolidayConfigs",
                columns: table => new
                {
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    IsHoliday = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HolidayConfigs", x => x.Date);
                });

            migrationBuilder.CreateTable(
                name: "StockSymbols",
                columns: table => new
                {
                    StockCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MarketType = table.Column<string>(type: "TEXT", nullable: false),
                    Industry = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockSymbols", x => x.StockCode);
                });

            migrationBuilder.CreateTable(
                name: "DailyClosingQuotes",
                columns: table => new
                {
                    StockCode = table.Column<string>(type: "TEXT", nullable: false),
                    TradeDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TradeVolume = table.Column<long>(type: "INTEGER", nullable: false),
                    TradeValue = table.Column<long>(type: "INTEGER", nullable: false),
                    OpeningPrice = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    HighestPrice = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    LowestPrice = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ClosingPrice = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Change = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TransactionCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyClosingQuotes", x => new { x.StockCode, x.TradeDate });
                    table.ForeignKey(
                        name: "FK_DailyClosingQuotes_StockSymbols_StockCode",
                        column: x => x.StockCode,
                        principalTable: "StockSymbols",
                        principalColumn: "StockCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MonthlyRevenues",
                columns: table => new
                {
                    StockCode = table.Column<string>(type: "TEXT", nullable: false),
                    YearMonth = table.Column<string>(type: "TEXT", nullable: false),
                    RevenueCurrent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RevenueLastYear = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MomChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    YoyChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyRevenues", x => new { x.StockCode, x.YearMonth });
                    table.ForeignKey(
                        name: "FK_MonthlyRevenues_StockSymbols_StockCode",
                        column: x => x.StockCode,
                        principalTable: "StockSymbols",
                        principalColumn: "StockCode",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyClosingQuotes");

            migrationBuilder.DropTable(
                name: "HolidayConfigs");

            migrationBuilder.DropTable(
                name: "MonthlyRevenues");

            migrationBuilder.DropTable(
                name: "StockSymbols");
        }
    }
}
