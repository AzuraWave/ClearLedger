using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfrastructureLayer.Migrations
{
    /// <inheritdoc />
    public partial class fixedRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientPaymentAllocations_ClientPaymentHeaders_ClientPaymentHeaderId",
                table: "ClientPaymentAllocations");

            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Organizations_OrganizationId",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_LedgerEntries_ProjectId",
                table: "LedgerEntries");

            migrationBuilder.DropIndex(
                name: "IX_Clients_OrganizationId",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_ClientPaymentAllocations_ClientPaymentHeaderId",
                table: "ClientPaymentAllocations");

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Projects",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Organizations",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<Guid>(
                name: "BatchId",
                table: "LedgerEntries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVoided",
                table: "LedgerEntries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "VoidedByEntryId",
                table: "LedgerEntries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Clients",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "ClientPaymentHeaders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Projects_OrganizationId_ClientId",
                table: "Projects",
                columns: new[] { "OrganizationId", "ClientId" });

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Name",
                table: "Organizations",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_BatchId",
                table: "LedgerEntries",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_OrganizationId_ClientId",
                table: "LedgerEntries",
                columns: new[] { "OrganizationId", "ClientId" });

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_ProjectId_Date",
                table: "LedgerEntries",
                columns: new[] { "ProjectId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Name",
                table: "Clients",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_OrganizationId_Status",
                table: "Clients",
                columns: new[] { "OrganizationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientPaymentHeaders_Date",
                table: "ClientPaymentHeaders",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_ClientPaymentHeaders_OrganizationId_ClientId",
                table: "ClientPaymentHeaders",
                columns: new[] { "OrganizationId", "ClientId" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientPaymentAllocations_ClientPaymentHeaderId_ProjectId",
                table: "ClientPaymentAllocations",
                columns: new[] { "ClientPaymentHeaderId", "ProjectId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ClientPaymentAllocations_ClientPaymentHeaders_ClientPaymentHeaderId",
                table: "ClientPaymentAllocations",
                column: "ClientPaymentHeaderId",
                principalTable: "ClientPaymentHeaders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientPaymentHeaders_Organizations_OrganizationId",
                table: "ClientPaymentHeaders",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Organizations_OrganizationId",
                table: "Clients",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LedgerEntries_Organizations_OrganizationId",
                table: "LedgerEntries",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Organizations_OrganizationId",
                table: "Projects",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientPaymentAllocations_ClientPaymentHeaders_ClientPaymentHeaderId",
                table: "ClientPaymentAllocations");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientPaymentHeaders_Organizations_OrganizationId",
                table: "ClientPaymentHeaders");

            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Organizations_OrganizationId",
                table: "Clients");

            migrationBuilder.DropForeignKey(
                name: "FK_LedgerEntries_Organizations_OrganizationId",
                table: "LedgerEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Organizations_OrganizationId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_OrganizationId_ClientId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_Name",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_LedgerEntries_BatchId",
                table: "LedgerEntries");

            migrationBuilder.DropIndex(
                name: "IX_LedgerEntries_OrganizationId_ClientId",
                table: "LedgerEntries");

            migrationBuilder.DropIndex(
                name: "IX_LedgerEntries_ProjectId_Date",
                table: "LedgerEntries");

            migrationBuilder.DropIndex(
                name: "IX_Clients_Name",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_OrganizationId_Status",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_ClientPaymentHeaders_Date",
                table: "ClientPaymentHeaders");

            migrationBuilder.DropIndex(
                name: "IX_ClientPaymentHeaders_OrganizationId_ClientId",
                table: "ClientPaymentHeaders");

            migrationBuilder.DropIndex(
                name: "IX_ClientPaymentAllocations_ClientPaymentHeaderId_ProjectId",
                table: "ClientPaymentAllocations");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "LedgerEntries");

            migrationBuilder.DropColumn(
                name: "IsVoided",
                table: "LedgerEntries");

            migrationBuilder.DropColumn(
                name: "VoidedByEntryId",
                table: "LedgerEntries");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "ClientPaymentHeaders");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Organizations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_ProjectId",
                table: "LedgerEntries",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_OrganizationId",
                table: "Clients",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientPaymentAllocations_ClientPaymentHeaderId",
                table: "ClientPaymentAllocations",
                column: "ClientPaymentHeaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientPaymentAllocations_ClientPaymentHeaders_ClientPaymentHeaderId",
                table: "ClientPaymentAllocations",
                column: "ClientPaymentHeaderId",
                principalTable: "ClientPaymentHeaders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Organizations_OrganizationId",
                table: "Clients",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
