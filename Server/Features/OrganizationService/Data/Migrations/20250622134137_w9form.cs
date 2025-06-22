using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace msih.p4g.Server.Features.OrganizationService.Data.Migrations
{
    /// <inheritdoc />
    public partial class w9form : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "W9Forms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    BusinessName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    FederalTaxClassification = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LLCTaxClassification = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true),
                    OtherClassificationInstructions = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    PartnershipTrustInfo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ExemptPayeeCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    FATCAExemptionCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CityStateZip = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    AccountNumbers = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    SocialSecurityNumber = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    EmployerIdentificationNumber = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    SignedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SignatureVerification = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    FundraiserId = table.Column<int>(type: "INTEGER", nullable: true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_W9Forms", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_W9Forms_FundraiserId",
                table: "W9Forms",
                column: "FundraiserId");

            migrationBuilder.CreateIndex(
                name: "IX_W9Forms_UserId",
                table: "W9Forms",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "W9Forms");
        }
    }
}
