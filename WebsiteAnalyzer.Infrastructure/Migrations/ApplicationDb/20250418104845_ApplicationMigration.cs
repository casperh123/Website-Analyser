using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebsiteAnalyzer.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class ApplicationMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BrokenLinkCrawls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    LinksChecked = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrokenLinkCrawls", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CrawlSchedules",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    Action = table.Column<int>(type: "INTEGER", nullable: false),
                    LastCrawlDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Frequency = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrawlSchedules", x => new { x.UserId, x.Url, x.Action });
                });

            migrationBuilder.CreateTable(
                name: "Websites",
                columns: table => new
                {
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Websites", x => new { x.Url, x.UserId });
                });

            migrationBuilder.CreateTable(
                name: "BrokenLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BrokenLinkCrawlId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TargetPage = table.Column<string>(type: "TEXT", nullable: false),
                    ReferringPage = table.Column<string>(type: "TEXT", nullable: false),
                    AnchorText = table.Column<string>(type: "TEXT", nullable: false),
                    Line = table.Column<int>(type: "INTEGER", nullable: false),
                    StatusCode = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrokenLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BrokenLinks_BrokenLinkCrawls_BrokenLinkCrawlId",
                        column: x => x.BrokenLinkCrawlId,
                        principalTable: "BrokenLinkCrawls",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CacheWarms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    WebsiteUrl = table.Column<string>(type: "TEXT", nullable: false),
                    VisitedPages = table.Column<int>(type: "INTEGER", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ScheduleUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ScheduleUrl = table.Column<string>(type: "TEXT", nullable: true),
                    ScheduleAction = table.Column<int>(type: "INTEGER", nullable: true),
                    WebsiteUserId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CacheWarms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CacheWarms_CrawlSchedules_ScheduleUserId_ScheduleUrl_ScheduleAction",
                        columns: x => new { x.ScheduleUserId, x.ScheduleUrl, x.ScheduleAction },
                        principalTable: "CrawlSchedules",
                        principalColumns: new[] { "UserId", "Url", "Action" });
                    table.ForeignKey(
                        name: "FK_CacheWarms_Websites_WebsiteUrl_WebsiteUserId",
                        columns: x => new { x.WebsiteUrl, x.WebsiteUserId },
                        principalTable: "Websites",
                        principalColumns: new[] { "Url", "UserId" });
                });

            migrationBuilder.CreateIndex(
                name: "IX_BrokenLinks_BrokenLinkCrawlId",
                table: "BrokenLinks",
                column: "BrokenLinkCrawlId");

            migrationBuilder.CreateIndex(
                name: "IX_CacheWarms_ScheduleUserId_ScheduleUrl_ScheduleAction",
                table: "CacheWarms",
                columns: new[] { "ScheduleUserId", "ScheduleUrl", "ScheduleAction" });

            migrationBuilder.CreateIndex(
                name: "IX_CacheWarms_WebsiteUrl_WebsiteUserId",
                table: "CacheWarms",
                columns: new[] { "WebsiteUrl", "WebsiteUserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BrokenLinks");

            migrationBuilder.DropTable(
                name: "CacheWarms");

            migrationBuilder.DropTable(
                name: "BrokenLinkCrawls");

            migrationBuilder.DropTable(
                name: "CrawlSchedules");

            migrationBuilder.DropTable(
                name: "Websites");
        }
    }
}
