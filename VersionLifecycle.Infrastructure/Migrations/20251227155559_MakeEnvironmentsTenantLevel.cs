using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VersionLifecycle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeEnvironmentsTenantLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Environments_Applications_ApplicationId",
                table: "Environments");

            migrationBuilder.DropIndex(
                name: "IX_Environments_ApplicationId_Name",
                table: "Environments");

            migrationBuilder.DropColumn(
                name: "ApplicationId",
                table: "Environments");

            migrationBuilder.CreateIndex(
                name: "IX_Environments_TenantId_Name",
                table: "Environments",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Environments_Tenants_TenantId",
                table: "Environments",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Environments_Tenants_TenantId",
                table: "Environments");

            migrationBuilder.DropIndex(
                name: "IX_Environments_TenantId_Name",
                table: "Environments");

            migrationBuilder.AddColumn<int>(
                name: "ApplicationId",
                table: "Environments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Environments_ApplicationId_Name",
                table: "Environments",
                columns: new[] { "ApplicationId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Environments_Applications_ApplicationId",
                table: "Environments",
                column: "ApplicationId",
                principalTable: "Applications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
