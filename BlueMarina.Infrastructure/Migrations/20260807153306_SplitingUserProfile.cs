using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlueMarina.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SplitingUserProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User Profile_Bvn",
                table: "User Profile");

            migrationBuilder.DropIndex(
                name: "IX_User Profile_KycLevel",
                table: "User Profile");

            migrationBuilder.DropIndex(
                name: "IX_User Profile_Nin",
                table: "User Profile");

            migrationBuilder.DropColumn(
                name: "AnnualIncome",
                table: "User Profile");

            migrationBuilder.DropColumn(
                name: "Bvn",
                table: "User Profile");

            migrationBuilder.DropColumn(
                name: "Employer",
                table: "User Profile");

            migrationBuilder.DropColumn(
                name: "FinancialGoalsJson",
                table: "User Profile");

            migrationBuilder.DropColumn(
                name: "KycLevel",
                table: "User Profile");

            migrationBuilder.DropColumn(
                name: "KycVerifiedAt",
                table: "User Profile");

            migrationBuilder.DropColumn(
                name: "NextOfKinName",
                table: "User Profile");

            migrationBuilder.DropColumn(
                name: "NextOfKinPhone",
                table: "User Profile");

            migrationBuilder.DropColumn(
                name: "NextOfKinRelationship",
                table: "User Profile");

            migrationBuilder.DropColumn(
                name: "Nin",
                table: "User Profile");

            migrationBuilder.DropColumn(
                name: "Occupation",
                table: "User Profile");

            migrationBuilder.DropColumn(
                name: "RiskAppetite",
                table: "User Profile");

            migrationBuilder.CreateTable(
                name: "Employment Information",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Occupation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Employer = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employment Information", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employment Information_User Profile_UserId",
                        column: x => x.UserId,
                        principalTable: "User Profile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Income Profile",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnnualIncome = table.Column<decimal>(type: "numeric", nullable: true),
                    RiskAppetite = table.Column<int>(type: "integer", nullable: false),
                    FinancialGoalsJson = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Income Profile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Income Profile_User Profile_UserId",
                        column: x => x.UserId,
                        principalTable: "User Profile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Kyc Profile",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Bvn = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    Nin = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    KycLevel = table.Column<int>(type: "integer", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kyc Profile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kyc Profile_User Profile_UserId",
                        column: x => x.UserId,
                        principalTable: "User Profile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Next Of Kin",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Relationship = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Next Of Kin", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Next Of Kin_User Profile_UserId",
                        column: x => x.UserId,
                        principalTable: "User Profile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employment Information_UserId",
                table: "Employment Information",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Income Profile_UserId",
                table: "Income Profile",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kyc Profile_Bvn",
                table: "Kyc Profile",
                column: "Bvn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kyc Profile_KycLevel",
                table: "Kyc Profile",
                column: "KycLevel");

            migrationBuilder.CreateIndex(
                name: "IX_Kyc Profile_Nin",
                table: "Kyc Profile",
                column: "Nin",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kyc Profile_UserId",
                table: "Kyc Profile",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Next Of Kin_UserId",
                table: "Next Of Kin",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Employment Information");

            migrationBuilder.DropTable(
                name: "Income Profile");

            migrationBuilder.DropTable(
                name: "Kyc Profile");

            migrationBuilder.DropTable(
                name: "Next Of Kin");

            migrationBuilder.AddColumn<decimal>(
                name: "AnnualIncome",
                table: "User Profile",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bvn",
                table: "User Profile",
                type: "character varying(11)",
                maxLength: 11,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Employer",
                table: "User Profile",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinancialGoalsJson",
                table: "User Profile",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KycLevel",
                table: "User Profile",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "KycVerifiedAt",
                table: "User Profile",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NextOfKinName",
                table: "User Profile",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NextOfKinPhone",
                table: "User Profile",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NextOfKinRelationship",
                table: "User Profile",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nin",
                table: "User Profile",
                type: "character varying(11)",
                maxLength: 11,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Occupation",
                table: "User Profile",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RiskAppetite",
                table: "User Profile",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_User Profile_Bvn",
                table: "User Profile",
                column: "Bvn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User Profile_KycLevel",
                table: "User Profile",
                column: "KycLevel");

            migrationBuilder.CreateIndex(
                name: "IX_User Profile_Nin",
                table: "User Profile",
                column: "Nin",
                unique: true);
        }
    }
}
