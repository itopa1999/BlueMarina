using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlueMarina.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDBDatabse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReviewedByStaffId",
                table: "selfie_checks");

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "selfie_checks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReviewedAt",
                table: "selfie_checks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewedByStaffUserId",
                table: "selfie_checks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReviewedAt",
                table: "id_documents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewedByStaffUserId",
                table: "id_documents",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "selfie_checks");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "selfie_checks");

            migrationBuilder.DropColumn(
                name: "ReviewedByStaffUserId",
                table: "selfie_checks");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "id_documents");

            migrationBuilder.DropColumn(
                name: "ReviewedByStaffUserId",
                table: "id_documents");

            migrationBuilder.AddColumn<long>(
                name: "ReviewedByStaffId",
                table: "selfie_checks",
                type: "bigint",
                nullable: true);
        }
    }
}
