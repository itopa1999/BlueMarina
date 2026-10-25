using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlueMarina.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedUserOnboardingProgressShit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tier1_IdDone",
                table: "user_onboarding_progress");

            migrationBuilder.DropColumn(
                name: "Tier1_SelfieDone",
                table: "user_onboarding_progress");

            migrationBuilder.RenameColumn(
                name: "Tier3_SignatureDone",
                table: "user_onboarding_progress",
                newName: "Tier3_TaxResidencyDone");

            migrationBuilder.RenameColumn(
                name: "Tier3_SelfieDone",
                table: "user_onboarding_progress",
                newName: "Tier3_PersonalDetailsDone");

            migrationBuilder.RenameColumn(
                name: "Tier3_IdDone",
                table: "user_onboarding_progress",
                newName: "Tier3_OccupationDone");

            migrationBuilder.RenameColumn(
                name: "Tier3_AccountDone",
                table: "user_onboarding_progress",
                newName: "Tier3_NextOfKinDone");

            migrationBuilder.RenameColumn(
                name: "Tier2_AccountDone",
                table: "user_onboarding_progress",
                newName: "Tier3_BankAccountDone");

            migrationBuilder.RenameColumn(
                name: "Tier1_SignatureDone",
                table: "user_onboarding_progress",
                newName: "Tier3_AddressDone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Tier3_TaxResidencyDone",
                table: "user_onboarding_progress",
                newName: "Tier3_SignatureDone");

            migrationBuilder.RenameColumn(
                name: "Tier3_PersonalDetailsDone",
                table: "user_onboarding_progress",
                newName: "Tier3_SelfieDone");

            migrationBuilder.RenameColumn(
                name: "Tier3_OccupationDone",
                table: "user_onboarding_progress",
                newName: "Tier3_IdDone");

            migrationBuilder.RenameColumn(
                name: "Tier3_NextOfKinDone",
                table: "user_onboarding_progress",
                newName: "Tier3_AccountDone");

            migrationBuilder.RenameColumn(
                name: "Tier3_BankAccountDone",
                table: "user_onboarding_progress",
                newName: "Tier2_AccountDone");

            migrationBuilder.RenameColumn(
                name: "Tier3_AddressDone",
                table: "user_onboarding_progress",
                newName: "Tier1_SignatureDone");

            migrationBuilder.AddColumn<bool>(
                name: "Tier1_IdDone",
                table: "user_onboarding_progress",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Tier1_SelfieDone",
                table: "user_onboarding_progress",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
