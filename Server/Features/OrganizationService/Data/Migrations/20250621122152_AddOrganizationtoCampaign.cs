using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace msih.p4g.Server.Features.OrganizationService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationtoCampaign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrganizationId1",
                table: "Campaigns",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_OrganizationId1",
                table: "Campaigns",
                column: "OrganizationId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_Organizations_OrganizationId1",
                table: "Campaigns",
                column: "OrganizationId1",
                principalTable: "Organizations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_Organizations_OrganizationId1",
                table: "Campaigns");

            migrationBuilder.DropIndex(
                name: "IX_Campaigns_OrganizationId1",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "OrganizationId1",
                table: "Campaigns");
        }
    }
}
