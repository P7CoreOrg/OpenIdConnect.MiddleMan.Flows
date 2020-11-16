using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OIDCConsentOrchestrator.EntityFrameworkCore.Migrations
{
    public partial class configuration2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DownstreamOIDCConfigurations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownstreamOIDCConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OIDCClientConfigurationEntity",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DownstreamOIDCConfigurationFK = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ClientId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientSecret = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OIDCClientConfigurationEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OIDCClientConfigurationEntity_DownstreamOIDCConfigurations_DownstreamOIDCConfigurationFK",
                        column: x => x.DownstreamOIDCConfigurationFK,
                        principalTable: "DownstreamOIDCConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RedirectUriEntity",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OIDCClientConfigurationFK = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RedirectUri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedirectUriEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RedirectUriEntity_OIDCClientConfigurationEntity_OIDCClientConfigurationFK",
                        column: x => x.OIDCClientConfigurationFK,
                        principalTable: "OIDCClientConfigurationEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OIDCClientConfigurationEntity_DownstreamOIDCConfigurationFK",
                table: "OIDCClientConfigurationEntity",
                column: "DownstreamOIDCConfigurationFK");

            migrationBuilder.CreateIndex(
                name: "IX_RedirectUriEntity_OIDCClientConfigurationFK",
                table: "RedirectUriEntity",
                column: "OIDCClientConfigurationFK");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RedirectUriEntity");

            migrationBuilder.DropTable(
                name: "OIDCClientConfigurationEntity");

            migrationBuilder.DropTable(
                name: "DownstreamOIDCConfigurations");
        }
    }
}
