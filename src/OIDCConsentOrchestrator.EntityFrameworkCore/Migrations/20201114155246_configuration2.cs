using Microsoft.EntityFrameworkCore.Migrations;

namespace OIDCConsentOrchestrator.EntityFrameworkCore.Migrations
{
    public partial class configuration2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Endpoint",
                table: "ExternalServices",
                newName: "Authority");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Authority",
                table: "ExternalServices",
                newName: "Endpoint");
        }
    }
}
