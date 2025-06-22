using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace msih.p4g.Server.Features.OrganizationService.Data.Migrations
{
    /// <inheritdoc />
    public partial class updateFundrasierPayOut3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PayPalAccount",
                table: "Fundraisers",
                newName: "PayoutAccount");

            migrationBuilder.AddColumn<int>(
                name: "PayoutAccountFormat",
                table: "Fundraisers",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PayoutAccountType",
                table: "Fundraisers",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayoutAccountFormat",
                table: "Fundraisers");

            migrationBuilder.DropColumn(
                name: "PayoutAccountType",
                table: "Fundraisers");

            migrationBuilder.RenameColumn(
                name: "PayoutAccount",
                table: "Fundraisers",
                newName: "PayPalAccount");
        }
    }
}
