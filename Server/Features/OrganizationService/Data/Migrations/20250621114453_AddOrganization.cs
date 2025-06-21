using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace msih.p4g.Server.Features.OrganizationService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Donations",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Campaigns",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LegalName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    TaxId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Website = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    EmailAddress = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    MissionStatement = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    ShortDescription = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    OrganizationType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LogoUrl = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Street = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PostalCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Country = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Donations_OrganizationId",
                table: "Donations",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_OrganizationId",
                table: "Campaigns",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_TaxId",
                table: "Organizations",
                column: "TaxId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_Organizations_OrganizationId",
                table: "Campaigns",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Donations_Organizations_OrganizationId",
                table: "Donations",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_Organizations_OrganizationId",
                table: "Campaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_Donations_Organizations_OrganizationId",
                table: "Donations");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Donations_OrganizationId",
                table: "Donations");

            migrationBuilder.DropIndex(
                name: "IX_Campaigns_OrganizationId",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Campaigns");
        }
    }
}
