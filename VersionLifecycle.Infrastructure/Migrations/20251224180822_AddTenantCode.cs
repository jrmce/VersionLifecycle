using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VersionLifecycle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Tenants",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE \"Tenants\" SET \"Code\" = 'DEMO-CODE' WHERE COALESCE(\"Code\", '') = '';");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Code",
                table: "Tenants",
                column: "Code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tenants_Code",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Tenants");
        }
    }
}
