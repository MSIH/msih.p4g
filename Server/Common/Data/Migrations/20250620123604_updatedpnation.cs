using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace msih.p4g.Server.Common.Data.Migrations
{
    /// <inheritdoc />
    public partial class updatedpnation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalAmountCharged",
                table: "Donations");

            migrationBuilder.RenameColumn(
                name: "TransactionFeeAmount",
                table: "Donations",
                newName: "PayTransactionFeeAmount");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Donations",
                newName: "DonationAmount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PayTransactionFeeAmount",
                table: "Donations",
                newName: "TransactionFeeAmount");

            migrationBuilder.RenameColumn(
                name: "DonationAmount",
                table: "Donations",
                newName: "Amount");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmountCharged",
                table: "Donations",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
