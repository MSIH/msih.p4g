using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace msih.p4g.Server.Features.Base.ProfileService.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Age = table.Column<int>(type: "INTEGER", nullable: true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Address_Street = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Address_City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Address_State = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Address_PostalCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Address_Country = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    MobileNumber = table.Column<string>(type: "TEXT", nullable: true),
                    ConsentReceiveText = table.Column<bool>(type: "INTEGER", nullable: false),
                    UnsubscribeMobile = table.Column<bool>(type: "INTEGER", nullable: false),
                    ConsentReceiveEmail = table.Column<bool>(type: "INTEGER", nullable: false),
                    UnsubscribeEmail = table.Column<bool>(type: "INTEGER", nullable: false),
                    ConsentReceiveMail = table.Column<bool>(type: "INTEGER", nullable: false),
                    UnsubscribeMail = table.Column<bool>(type: "INTEGER", nullable: false),
                    ReferralCode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_ReferralCode",
                table: "Profiles",
                column: "ReferralCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Profiles");
        }
    }
}
