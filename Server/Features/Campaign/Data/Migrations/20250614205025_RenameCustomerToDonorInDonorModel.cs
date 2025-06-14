using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace msih.p4g.Server.Features.Campaign.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameCustomerToDonorInDonorModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PaymentProcessorCustomerId",
                table: "Donors",
                newName: "PaymentProcessorDonorId");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "Donors",
                newName: "DonorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PaymentProcessorDonorId",
                table: "Donors",
                newName: "PaymentProcessorCustomerId");

            migrationBuilder.RenameColumn(
                name: "DonorId",
                table: "Donors",
                newName: "CustomerId");
        }
    }
}
