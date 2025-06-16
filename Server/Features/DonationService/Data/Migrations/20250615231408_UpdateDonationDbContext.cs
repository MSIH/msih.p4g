using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace msih.p4g.Server.Features.DonationService.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDonationDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Donations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    DonorId = table.Column<int>(type: "INTEGER", nullable: false),
                    PaymentTransactionId = table.Column<int>(type: "INTEGER", nullable: true),
                    PayTransactionFee = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsMonthly = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsAnnual = table.Column<bool>(type: "INTEGER", nullable: false),
                    DonationMessage = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ReferralCode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CampaignCode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Donations", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Donations");
        }
    }
}
