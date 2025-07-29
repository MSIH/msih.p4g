using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace msih.p4g.Server.Common.Data.Migrations
{
    /// <inheritdoc />
    public partial class RecurringDonationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NextProcessDate",
                table: "Donations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParentRecurringDonationId",
                table: "Donations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecurringPaymentToken",
                table: "Donations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Donations_ParentRecurringDonationId",
                table: "Donations",
                column: "ParentRecurringDonationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Donations_Donations_ParentRecurringDonationId",
                table: "Donations",
                column: "ParentRecurringDonationId",
                principalTable: "Donations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Donations_Donations_ParentRecurringDonationId",
                table: "Donations");

            migrationBuilder.DropIndex(
                name: "IX_Donations_ParentRecurringDonationId",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "NextProcessDate",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "ParentRecurringDonationId",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "RecurringPaymentToken",
                table: "Donations");
        }
    }
}
