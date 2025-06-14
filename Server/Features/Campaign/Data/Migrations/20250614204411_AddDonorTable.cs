using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace msih.p4g.Server.Features.Campaign.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDonorTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Donors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CustomerId = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    PaymentProcessorCustomerId = table.Column<string>(type: "TEXT", nullable: false),
                    MobileNumber = table.Column<string>(type: "TEXT", nullable: false),
                    ConsentReceiveText = table.Column<bool>(type: "INTEGER", nullable: false),
                    UnsubscribeMobile = table.Column<bool>(type: "INTEGER", nullable: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: false),
                    ConsentReceivePhone = table.Column<bool>(type: "INTEGER", nullable: false),
                    UnsubscribePhone = table.Column<bool>(type: "INTEGER", nullable: false),
                    EmailAddress = table.Column<string>(type: "TEXT", nullable: false),
                    ConsentReceiveEmail = table.Column<bool>(type: "INTEGER", nullable: false),
                    UnsubscribeEmail = table.Column<bool>(type: "INTEGER", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    ConsentReceiveMail = table.Column<bool>(type: "INTEGER", nullable: false),
                    UnsubscribeMail = table.Column<bool>(type: "INTEGER", nullable: false),
                    Address_Street = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Address_City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Address_State = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Address_PostalCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Address_Country = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Donors", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Donors");
        }
    }
}
