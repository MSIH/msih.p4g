using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace msih.p4g.Server.Common.Data.Migrations
{
    /// <inheritdoc />
    public partial class MoveSuspensionFieldsToFundraiser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add suspension fields to Fundraisers table
            migrationBuilder.AddColumn<bool>(
                name: "IsSuspended",
                table: "Fundraisers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SuspendedDate",
                table: "Fundraisers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuspensionReason",
                table: "Fundraisers",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            // Migrate data from Profiles to Fundraisers
            // Note: This assumes that all profiles with suspension data have corresponding fundraiser records
            migrationBuilder.Sql(@"
                UPDATE Fundraisers 
                SET IsSuspended = p.IsSuspended,
                    SuspendedDate = p.SuspendedDate,
                    SuspensionReason = p.SuspensionReason
                FROM Profiles p
                WHERE Fundraisers.UserId = p.UserId 
                  AND (p.IsSuspended = 1 OR p.SuspendedDate IS NOT NULL OR p.SuspensionReason IS NOT NULL)
            ");

            // Remove suspension fields from Profiles table
            migrationBuilder.DropColumn(
                name: "IsSuspended",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "SuspendedDate",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "SuspensionReason",
                table: "Profiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Add suspension fields back to Profiles table
            migrationBuilder.AddColumn<bool>(
                name: "IsSuspended",
                table: "Profiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SuspendedDate",
                table: "Profiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuspensionReason",
                table: "Profiles",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            // Migrate data back from Fundraisers to Profiles
            migrationBuilder.Sql(@"
                UPDATE Profiles 
                SET IsSuspended = f.IsSuspended,
                    SuspendedDate = f.SuspendedDate,
                    SuspensionReason = f.SuspensionReason
                FROM Fundraisers f
                WHERE Profiles.UserId = f.UserId 
                  AND (f.IsSuspended = 1 OR f.SuspendedDate IS NOT NULL OR f.SuspensionReason IS NOT NULL)
            ");

            // Remove suspension fields from Fundraisers table
            migrationBuilder.DropColumn(
                name: "IsSuspended",
                table: "Fundraisers");

            migrationBuilder.DropColumn(
                name: "SuspendedDate",
                table: "Fundraisers");

            migrationBuilder.DropColumn(
                name: "SuspensionReason",
                table: "Fundraisers");
        }
    }
}