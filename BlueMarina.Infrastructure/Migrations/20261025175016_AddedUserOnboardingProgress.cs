using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BlueMarina.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedUserOnboardingProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_onboarding_progress",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(100)", nullable: false),
                    Tier1_AccountDone = table.Column<bool>(type: "boolean", nullable: false),
                    Tier1_SelfieDone = table.Column<bool>(type: "boolean", nullable: false),
                    Tier1_IdDone = table.Column<bool>(type: "boolean", nullable: false),
                    Tier1_SignatureDone = table.Column<bool>(type: "boolean", nullable: false),
                    Tier1_IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    Tier2_AccountDone = table.Column<bool>(type: "boolean", nullable: false),
                    Tier2_SelfieDone = table.Column<bool>(type: "boolean", nullable: false),
                    Tier2_IdDone = table.Column<bool>(type: "boolean", nullable: false),
                    Tier2_SignatureDone = table.Column<bool>(type: "boolean", nullable: false),
                    Tier2_IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    Tier3_AccountDone = table.Column<bool>(type: "boolean", nullable: false),
                    Tier3_SelfieDone = table.Column<bool>(type: "boolean", nullable: false),
                    Tier3_IdDone = table.Column<bool>(type: "boolean", nullable: false),
                    Tier3_SignatureDone = table.Column<bool>(type: "boolean", nullable: false),
                    Tier3_IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_onboarding_progress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_onboarding_progress_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "PublicId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_onboarding_progress_UserId",
                table: "user_onboarding_progress",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_onboarding_progress");
        }
    }
}
