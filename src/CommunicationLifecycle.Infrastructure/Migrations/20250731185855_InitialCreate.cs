using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CommunicationLifecycle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Communications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TypeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CurrentStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastUpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SourceFileUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Communications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommunicationTypes",
                columns: table => new
                {
                    TypeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunicationTypes", x => x.TypeCode);
                });

            migrationBuilder.CreateTable(
                name: "CommunicationStatusHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommunicationId = table.Column<int>(type: "int", nullable: false),
                    StatusCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OccurredUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunicationStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommunicationStatusHistory_Communications_CommunicationId",
                        column: x => x.CommunicationId,
                        principalTable: "Communications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommunicationTypeStatuses",
                columns: table => new
                {
                    TypeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StatusCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunicationTypeStatuses", x => new { x.TypeCode, x.StatusCode });
                    table.ForeignKey(
                        name: "FK_CommunicationTypeStatuses_CommunicationTypes_TypeCode",
                        column: x => x.TypeCode,
                        principalTable: "CommunicationTypes",
                        principalColumn: "TypeCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "CommunicationTypes",
                columns: new[] { "TypeCode", "Description", "DisplayName", "IsActive" },
                values: new object[,]
                {
                    { "CLAIM_STATEMENT", "Claim statements", "Claim Statement", true },
                    { "EOB", "Explanation of Benefits documents", "Explanation of Benefits", true },
                    { "EOP", "Explanation of Payment documents", "Explanation of Payment", true },
                    { "ID_CARD", "Member identification cards", "Member ID Card", true },
                    { "PROVIDER_STATEMENT", "Provider statements", "Provider Statement", true },
                    { "WELCOME_PACKET", "New member welcome packets", "Welcome Packet", true }
                });

            migrationBuilder.InsertData(
                table: "CommunicationTypeStatuses",
                columns: new[] { "StatusCode", "TypeCode", "Description", "DisplayOrder" },
                values: new object[,]
                {
                    { "Archived", "CLAIM_STATEMENT", "CLAIM_STATEMENT Archived status", 8 },
                    { "Cancelled", "CLAIM_STATEMENT", "CLAIM_STATEMENT Cancelled status", 7 },
                    { "Delivered", "CLAIM_STATEMENT", "CLAIM_STATEMENT Delivered status", 5 },
                    { "Failed", "CLAIM_STATEMENT", "CLAIM_STATEMENT Failed status", 6 },
                    { "Printed", "CLAIM_STATEMENT", "CLAIM_STATEMENT Printed status", 3 },
                    { "ReadyForRelease", "CLAIM_STATEMENT", "CLAIM_STATEMENT ReadyForRelease status", 1 },
                    { "Released", "CLAIM_STATEMENT", "CLAIM_STATEMENT Released status", 2 },
                    { "Shipped", "CLAIM_STATEMENT", "CLAIM_STATEMENT Shipped status", 4 },
                    { "Archived", "EOB", "EOB Archived status", 13 },
                    { "Cancelled", "EOB", "EOB Cancelled status", 12 },
                    { "Delivered", "EOB", "EOB Delivered status", 9 },
                    { "Failed", "EOB", "EOB Failed status", 11 },
                    { "Inserted", "EOB", "EOB Inserted status", 5 },
                    { "InTransit", "EOB", "EOB InTransit status", 8 },
                    { "Printed", "EOB", "EOB Printed status", 4 },
                    { "QueuedForPrinting", "EOB", "EOB QueuedForPrinting status", 3 },
                    { "ReadyForRelease", "EOB", "EOB ReadyForRelease status", 1 },
                    { "Released", "EOB", "EOB Released status", 2 },
                    { "Returned", "EOB", "EOB Returned status", 10 },
                    { "Shipped", "EOB", "EOB Shipped status", 7 },
                    { "WarehouseReady", "EOB", "EOB WarehouseReady status", 6 },
                    { "Archived", "EOP", "EOP Archived status", 13 },
                    { "Cancelled", "EOP", "EOP Cancelled status", 12 },
                    { "Delivered", "EOP", "EOP Delivered status", 9 },
                    { "Failed", "EOP", "EOP Failed status", 11 },
                    { "Inserted", "EOP", "EOP Inserted status", 5 },
                    { "InTransit", "EOP", "EOP InTransit status", 8 },
                    { "Printed", "EOP", "EOP Printed status", 4 },
                    { "QueuedForPrinting", "EOP", "EOP QueuedForPrinting status", 3 },
                    { "ReadyForRelease", "EOP", "EOP ReadyForRelease status", 1 },
                    { "Released", "EOP", "EOP Released status", 2 },
                    { "Returned", "EOP", "EOP Returned status", 10 },
                    { "Shipped", "EOP", "EOP Shipped status", 7 },
                    { "WarehouseReady", "EOP", "EOP WarehouseReady status", 6 },
                    { "Archived", "ID_CARD", "ID Card Archived status", 13 },
                    { "Cancelled", "ID_CARD", "ID Card Cancelled status", 11 },
                    { "Delivered", "ID_CARD", "ID Card Delivered status", 8 },
                    { "Expired", "ID_CARD", "ID Card Expired status", 12 },
                    { "Failed", "ID_CARD", "ID Card Failed status", 10 },
                    { "InTransit", "ID_CARD", "ID Card InTransit status", 7 },
                    { "Printed", "ID_CARD", "ID Card Printed status", 4 },
                    { "QueuedForPrinting", "ID_CARD", "ID Card QueuedForPrinting status", 3 },
                    { "ReadyForRelease", "ID_CARD", "ID Card ReadyForRelease status", 1 },
                    { "Released", "ID_CARD", "ID Card Released status", 2 },
                    { "Returned", "ID_CARD", "ID Card Returned status", 9 },
                    { "Shipped", "ID_CARD", "ID Card Shipped status", 6 },
                    { "WarehouseReady", "ID_CARD", "ID Card WarehouseReady status", 5 },
                    { "Archived", "PROVIDER_STATEMENT", "PROVIDER_STATEMENT Archived status", 8 },
                    { "Cancelled", "PROVIDER_STATEMENT", "PROVIDER_STATEMENT Cancelled status", 7 },
                    { "Delivered", "PROVIDER_STATEMENT", "PROVIDER_STATEMENT Delivered status", 5 },
                    { "Failed", "PROVIDER_STATEMENT", "PROVIDER_STATEMENT Failed status", 6 },
                    { "Printed", "PROVIDER_STATEMENT", "PROVIDER_STATEMENT Printed status", 3 },
                    { "ReadyForRelease", "PROVIDER_STATEMENT", "PROVIDER_STATEMENT ReadyForRelease status", 1 },
                    { "Released", "PROVIDER_STATEMENT", "PROVIDER_STATEMENT Released status", 2 },
                    { "Shipped", "PROVIDER_STATEMENT", "PROVIDER_STATEMENT Shipped status", 4 },
                    { "Archived", "WELCOME_PACKET", "WELCOME_PACKET Archived status", 8 },
                    { "Cancelled", "WELCOME_PACKET", "WELCOME_PACKET Cancelled status", 7 },
                    { "Delivered", "WELCOME_PACKET", "WELCOME_PACKET Delivered status", 5 },
                    { "Failed", "WELCOME_PACKET", "WELCOME_PACKET Failed status", 6 },
                    { "Printed", "WELCOME_PACKET", "WELCOME_PACKET Printed status", 3 },
                    { "ReadyForRelease", "WELCOME_PACKET", "WELCOME_PACKET ReadyForRelease status", 1 },
                    { "Released", "WELCOME_PACKET", "WELCOME_PACKET Released status", 2 },
                    { "Shipped", "WELCOME_PACKET", "WELCOME_PACKET Shipped status", 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Communications_CurrentStatus",
                table: "Communications",
                column: "CurrentStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Communications_LastUpdatedUtc",
                table: "Communications",
                column: "LastUpdatedUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Communications_TypeCode",
                table: "Communications",
                column: "TypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationStatusHistory_CommunicationId",
                table: "CommunicationStatusHistory",
                column: "CommunicationId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationStatusHistory_OccurredUtc",
                table: "CommunicationStatusHistory",
                column: "OccurredUtc");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationStatusHistory_StatusCode",
                table: "CommunicationStatusHistory",
                column: "StatusCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommunicationStatusHistory");

            migrationBuilder.DropTable(
                name: "CommunicationTypeStatuses");

            migrationBuilder.DropTable(
                name: "Communications");

            migrationBuilder.DropTable(
                name: "CommunicationTypes");
        }
    }
}
