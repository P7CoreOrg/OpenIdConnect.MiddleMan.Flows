using Microsoft.EntityFrameworkCore.Migrations;

namespace OIDCConsentOrchestrator.EntityFrameworkCore.Migrations
{
    public partial class configuration3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OIDCClientConfigurationEntity_DownstreamOIDCConfigurations_DownstreamOIDCConfigurationFK",
                table: "OIDCClientConfigurationEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_RedirectUriEntity_OIDCClientConfigurationEntity_OIDCClientConfigurationFK",
                table: "RedirectUriEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RedirectUriEntity",
                table: "RedirectUriEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OIDCClientConfigurationEntity",
                table: "OIDCClientConfigurationEntity");

            migrationBuilder.RenameTable(
                name: "RedirectUriEntity",
                newName: "RedirectUris");

            migrationBuilder.RenameTable(
                name: "OIDCClientConfigurationEntity",
                newName: "OIDCClientConfigurations");

            migrationBuilder.RenameIndex(
                name: "IX_RedirectUriEntity_OIDCClientConfigurationFK",
                table: "RedirectUris",
                newName: "IX_RedirectUris_OIDCClientConfigurationFK");

            migrationBuilder.RenameIndex(
                name: "IX_OIDCClientConfigurationEntity_DownstreamOIDCConfigurationFK",
                table: "OIDCClientConfigurations",
                newName: "IX_OIDCClientConfigurations_DownstreamOIDCConfigurationFK");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RedirectUris",
                table: "RedirectUris",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OIDCClientConfigurations",
                table: "OIDCClientConfigurations",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OIDCClientConfigurations_DownstreamOIDCConfigurations_DownstreamOIDCConfigurationFK",
                table: "OIDCClientConfigurations",
                column: "DownstreamOIDCConfigurationFK",
                principalTable: "DownstreamOIDCConfigurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RedirectUris_OIDCClientConfigurations_OIDCClientConfigurationFK",
                table: "RedirectUris",
                column: "OIDCClientConfigurationFK",
                principalTable: "OIDCClientConfigurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OIDCClientConfigurations_DownstreamOIDCConfigurations_DownstreamOIDCConfigurationFK",
                table: "OIDCClientConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_RedirectUris_OIDCClientConfigurations_OIDCClientConfigurationFK",
                table: "RedirectUris");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RedirectUris",
                table: "RedirectUris");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OIDCClientConfigurations",
                table: "OIDCClientConfigurations");

            migrationBuilder.RenameTable(
                name: "RedirectUris",
                newName: "RedirectUriEntity");

            migrationBuilder.RenameTable(
                name: "OIDCClientConfigurations",
                newName: "OIDCClientConfigurationEntity");

            migrationBuilder.RenameIndex(
                name: "IX_RedirectUris_OIDCClientConfigurationFK",
                table: "RedirectUriEntity",
                newName: "IX_RedirectUriEntity_OIDCClientConfigurationFK");

            migrationBuilder.RenameIndex(
                name: "IX_OIDCClientConfigurations_DownstreamOIDCConfigurationFK",
                table: "OIDCClientConfigurationEntity",
                newName: "IX_OIDCClientConfigurationEntity_DownstreamOIDCConfigurationFK");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RedirectUriEntity",
                table: "RedirectUriEntity",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OIDCClientConfigurationEntity",
                table: "OIDCClientConfigurationEntity",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OIDCClientConfigurationEntity_DownstreamOIDCConfigurations_DownstreamOIDCConfigurationFK",
                table: "OIDCClientConfigurationEntity",
                column: "DownstreamOIDCConfigurationFK",
                principalTable: "DownstreamOIDCConfigurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RedirectUriEntity_OIDCClientConfigurationEntity_OIDCClientConfigurationFK",
                table: "RedirectUriEntity",
                column: "OIDCClientConfigurationFK",
                principalTable: "OIDCClientConfigurationEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
