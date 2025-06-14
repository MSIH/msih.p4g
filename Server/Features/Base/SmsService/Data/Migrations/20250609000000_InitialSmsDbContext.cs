/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace msih.p4g.Server.Features.Base.SmsService.Data.Migrations
{
    /// <summary>
    /// Initial migration for the SMS service database context
    /// </summary>
    public partial class InitialSmsDbContext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ValidatedPhoneNumbers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    PhoneNumber = table.Column<string>(maxLength: 20, nullable: false),
                    IsMobile = table.Column<bool>(nullable: false),
                    Carrier = table.Column<string>(maxLength: 100, nullable: true),
                    CountryCode = table.Column<string>(maxLength: 5, nullable: true),
                    ValidatedOn = table.Column<DateTime>(nullable: false),
                    IsValid = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValidatedPhoneNumbers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ValidatedPhoneNumbers_PhoneNumber",
                table: "ValidatedPhoneNumbers",
                column: "PhoneNumber",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ValidatedPhoneNumbers");
        }
    }
}
