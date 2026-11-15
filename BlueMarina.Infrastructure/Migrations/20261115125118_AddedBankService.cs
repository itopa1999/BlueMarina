using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlueMarina.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedBankService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_banks_code",
                table: "banks");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "banks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Nigeria");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "banks",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "NGN");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "banks",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "LongCode",
                table: "banks",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "banks",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "SupportsDirectDebit",
                table: "banks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SupportsTransfer",
                table: "banks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "ix_banks_code_currency",
                table: "banks",
                columns: new[] { "Code", "Currency" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_banks_slug",
                table: "banks",
                column: "Slug");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_banks_code_currency",
                table: "banks");

            migrationBuilder.DropIndex(
                name: "ix_banks_slug",
                table: "banks");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "banks");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "banks");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "banks");

            migrationBuilder.DropColumn(
                name: "LongCode",
                table: "banks");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "banks");

            migrationBuilder.DropColumn(
                name: "SupportsDirectDebit",
                table: "banks");

            migrationBuilder.DropColumn(
                name: "SupportsTransfer",
                table: "banks");

            migrationBuilder.CreateIndex(
                name: "ix_banks_code",
                table: "banks",
                column: "Code",
                unique: true);
        }
    }
}
