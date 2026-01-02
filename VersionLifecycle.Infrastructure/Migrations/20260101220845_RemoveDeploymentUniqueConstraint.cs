using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VersionLifecycle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDeploymentUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Deployments_ApplicationId_VersionId_EnvironmentId",
                table: "Deployments");

            migrationBuilder.CreateIndex(
                name: "IX_Deployments_ApplicationId_VersionId_EnvironmentId",
                table: "Deployments",
                columns: new[] { "ApplicationId", "VersionId", "EnvironmentId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Deployments_ApplicationId_VersionId_EnvironmentId",
                table: "Deployments");

            migrationBuilder.CreateIndex(
                name: "IX_Deployments_ApplicationId_VersionId_EnvironmentId",
                table: "Deployments",
                columns: new[] { "ApplicationId", "VersionId", "EnvironmentId" },
                unique: true);
        }
    }
}
