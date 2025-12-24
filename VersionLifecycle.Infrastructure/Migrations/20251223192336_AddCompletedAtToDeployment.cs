using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VersionLifecycle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompletedAtToDeployment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "Deployments",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "Deployments");
        }
    }
}
