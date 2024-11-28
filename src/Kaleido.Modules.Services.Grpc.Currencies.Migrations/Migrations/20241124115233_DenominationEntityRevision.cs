﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kaleido.Modules.Services.Grpc.Currencies.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class DenominationEntityRevision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DenominationRevisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "varchar(36)", nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Revision = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "varchar(8)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DenominationRevisions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DenominationRevisions_Key",
                table: "DenominationRevisions",
                column: "Key");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DenominationRevisions");
        }
    }
}
