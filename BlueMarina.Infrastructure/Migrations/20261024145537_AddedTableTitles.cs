using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlueMarina.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedTableTitles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_AspNetUsers_UserId",
                table: "Addresses");

            migrationBuilder.DropForeignKey(
                name: "FK_AgreementAcceptances_AgreementVersions_AgreementVersionId",
                table: "AgreementAcceptances");

            migrationBuilder.DropForeignKey(
                name: "FK_AgreementAcceptances_AspNetUsers_UserId",
                table: "AgreementAcceptances");

            migrationBuilder.DropForeignKey(
                name: "FK_Companies_AspNetUsers_CreatedByUserId",
                table: "Companies");

            migrationBuilder.DropForeignKey(
                name: "FK_CompanyDocuments_Companies_CompanyId",
                table: "CompanyDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_CompanyInvitations_Companies_CompanyId",
                table: "CompanyInvitations");

            migrationBuilder.DropForeignKey(
                name: "FK_CompanyMembers_AspNetUsers_UserId",
                table: "CompanyMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_CompanyMembers_Companies_CompanyId",
                table: "CompanyMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_Consents_AspNetUsers_UserId",
                table: "Consents");

            migrationBuilder.DropForeignKey(
                name: "FK_Credentials_AspNetUsers_UserId",
                table: "Credentials");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceApprovalRequests_AspNetUsers_UserId",
                table: "DeviceApprovalRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Devices_AspNetUsers_UserId",
                table: "Devices");

            migrationBuilder.DropForeignKey(
                name: "FK_IdDocuments_AspNetUsers_UserId",
                table: "IdDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_IdentityChecks_AspNetUsers_UserId",
                table: "IdentityChecks");

            migrationBuilder.DropForeignKey(
                name: "FK_NextOfKins_AspNetUsers_UserId",
                table: "NextOfKins");

            migrationBuilder.DropForeignKey(
                name: "FK_SelfieChecks_AspNetUsers_UserId",
                table: "SelfieChecks");

            migrationBuilder.DropForeignKey(
                name: "FK_Signatures_AspNetUsers_UserId",
                table: "Signatures");

            migrationBuilder.DropForeignKey(
                name: "FK_TaxResidencies_AspNetUsers_UserId",
                table: "TaxResidencies");

            migrationBuilder.DropForeignKey(
                name: "FK_TierUpgradeItems_TierUpgradeRequests_TierUpgradeRequestId",
                table: "TierUpgradeItems");

            migrationBuilder.DropForeignKey(
                name: "FK_TierUpgradeRequests_AspNetUsers_UserId",
                table: "TierUpgradeRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_TwoFactorMethods_AspNetUsers_UserId",
                table: "TwoFactorMethods");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Signatures",
                table: "Signatures");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Credentials",
                table: "Credentials");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Consents",
                table: "Consents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Companies",
                table: "Companies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Addresses",
                table: "Addresses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TwoFactorMethods",
                table: "TwoFactorMethods");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TierUpgradeRequests",
                table: "TierUpgradeRequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TierUpgradeItems",
                table: "TierUpgradeItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TierLimits",
                table: "TierLimits");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaxResidencies",
                table: "TaxResidencies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SignInAttempts",
                table: "SignInAttempts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SelfieChecks",
                table: "SelfieChecks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NextOfKins",
                table: "NextOfKins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IdentityChecks",
                table: "IdentityChecks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IdDocuments",
                table: "IdDocuments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Devices",
                table: "Devices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeviceApprovalRequests",
                table: "DeviceApprovalRequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompanyMembers",
                table: "CompanyMembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompanyInvitations",
                table: "CompanyInvitations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompanyDocuments",
                table: "CompanyDocuments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AgreementVersions",
                table: "AgreementVersions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AgreementAcceptances",
                table: "AgreementAcceptances");

            migrationBuilder.RenameTable(
                name: "Signatures",
                newName: "signatures");

            migrationBuilder.RenameTable(
                name: "Credentials",
                newName: "credentials");

            migrationBuilder.RenameTable(
                name: "Consents",
                newName: "consents");

            migrationBuilder.RenameTable(
                name: "Companies",
                newName: "companies");

            migrationBuilder.RenameTable(
                name: "Addresses",
                newName: "addresses");

            migrationBuilder.RenameTable(
                name: "TwoFactorMethods",
                newName: "two_factor_methods");

            migrationBuilder.RenameTable(
                name: "TierUpgradeRequests",
                newName: "tier_upgrade_requests");

            migrationBuilder.RenameTable(
                name: "TierUpgradeItems",
                newName: "tier_upgrade_items");

            migrationBuilder.RenameTable(
                name: "TierLimits",
                newName: "tier_limits");

            migrationBuilder.RenameTable(
                name: "TaxResidencies",
                newName: "tax_residencies");

            migrationBuilder.RenameTable(
                name: "SignInAttempts",
                newName: "signIn_attempts");

            migrationBuilder.RenameTable(
                name: "SelfieChecks",
                newName: "selfie_checks");

            migrationBuilder.RenameTable(
                name: "NextOfKins",
                newName: "next_of_kins");

            migrationBuilder.RenameTable(
                name: "IdentityChecks",
                newName: "identity_checks");

            migrationBuilder.RenameTable(
                name: "IdDocuments",
                newName: "id_documents");

            migrationBuilder.RenameTable(
                name: "Devices",
                newName: "device");

            migrationBuilder.RenameTable(
                name: "DeviceApprovalRequests",
                newName: "device_approval_requests");

            migrationBuilder.RenameTable(
                name: "CompanyMembers",
                newName: "company_members");

            migrationBuilder.RenameTable(
                name: "CompanyInvitations",
                newName: "company_invitations");

            migrationBuilder.RenameTable(
                name: "CompanyDocuments",
                newName: "company_documents");

            migrationBuilder.RenameTable(
                name: "AgreementVersions",
                newName: "agreement_versions");

            migrationBuilder.RenameTable(
                name: "AgreementAcceptances",
                newName: "agreement_acceptances");

            migrationBuilder.RenameIndex(
                name: "IX_Signatures_UserId",
                table: "signatures",
                newName: "IX_signatures_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Credentials_UserId",
                table: "credentials",
                newName: "IX_credentials_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Consents_UserId",
                table: "consents",
                newName: "IX_consents_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Companies_RcNumber",
                table: "companies",
                newName: "IX_companies_RcNumber");

            migrationBuilder.RenameIndex(
                name: "IX_Companies_CreatedByUserId",
                table: "companies",
                newName: "IX_companies_CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Addresses_UserId",
                table: "addresses",
                newName: "IX_addresses_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_TwoFactorMethods_UserId",
                table: "two_factor_methods",
                newName: "IX_two_factor_methods_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_TierUpgradeRequests_UserId",
                table: "tier_upgrade_requests",
                newName: "IX_tier_upgrade_requests_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_TierUpgradeItems_TierUpgradeRequestId",
                table: "tier_upgrade_items",
                newName: "IX_tier_upgrade_items_TierUpgradeRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_TierLimits_Tier",
                table: "tier_limits",
                newName: "IX_tier_limits_Tier");

            migrationBuilder.RenameIndex(
                name: "IX_TaxResidencies_UserId",
                table: "tax_residencies",
                newName: "IX_tax_residencies_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_SelfieChecks_UserId",
                table: "selfie_checks",
                newName: "IX_selfie_checks_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_NextOfKins_UserId",
                table: "next_of_kins",
                newName: "IX_next_of_kins_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_IdentityChecks_UserId",
                table: "identity_checks",
                newName: "IX_identity_checks_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_IdDocuments_UserId",
                table: "id_documents",
                newName: "IX_id_documents_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Devices_UserId",
                table: "device",
                newName: "IX_device_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_DeviceApprovalRequests_UserId",
                table: "device_approval_requests",
                newName: "IX_device_approval_requests_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_CompanyMembers_UserId",
                table: "company_members",
                newName: "IX_company_members_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_CompanyMembers_CompanyId",
                table: "company_members",
                newName: "IX_company_members_CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_CompanyInvitations_TokenHash",
                table: "company_invitations",
                newName: "IX_company_invitations_TokenHash");

            migrationBuilder.RenameIndex(
                name: "IX_CompanyInvitations_CompanyId",
                table: "company_invitations",
                newName: "IX_company_invitations_CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_CompanyDocuments_CompanyId",
                table: "company_documents",
                newName: "IX_company_documents_CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_AgreementVersions_Type_Version",
                table: "agreement_versions",
                newName: "IX_agreement_versions_Type_Version");

            migrationBuilder.RenameIndex(
                name: "IX_AgreementAcceptances_UserId",
                table: "agreement_acceptances",
                newName: "IX_agreement_acceptances_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_AgreementAcceptances_AgreementVersionId",
                table: "agreement_acceptances",
                newName: "IX_agreement_acceptances_AgreementVersionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_signatures",
                table: "signatures",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_credentials",
                table: "credentials",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_consents",
                table: "consents",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_companies",
                table: "companies",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_addresses",
                table: "addresses",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_two_factor_methods",
                table: "two_factor_methods",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tier_upgrade_requests",
                table: "tier_upgrade_requests",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tier_upgrade_items",
                table: "tier_upgrade_items",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tier_limits",
                table: "tier_limits",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tax_residencies",
                table: "tax_residencies",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_signIn_attempts",
                table: "signIn_attempts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_selfie_checks",
                table: "selfie_checks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_next_of_kins",
                table: "next_of_kins",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_identity_checks",
                table: "identity_checks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_id_documents",
                table: "id_documents",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_device",
                table: "device",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_device_approval_requests",
                table: "device_approval_requests",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_company_members",
                table: "company_members",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_company_invitations",
                table: "company_invitations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_company_documents",
                table: "company_documents",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_agreement_versions",
                table: "agreement_versions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_agreement_acceptances",
                table: "agreement_acceptances",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_addresses_AspNetUsers_UserId",
                table: "addresses",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "PublicId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_agreement_acceptances_AspNetUsers_UserId",
                table: "agreement_acceptances",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "PublicId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_agreement_acceptances_agreement_versions_AgreementVersionId",
                table: "agreement_acceptances",
                column: "AgreementVersionId",
                principalTable: "agreement_versions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_companies_AspNetUsers_CreatedByUserId",
                table: "companies",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "PublicId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_company_documents_companies_CompanyId",
                table: "company_documents",
                column: "CompanyId",
                principalTable: "companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_company_invitations_companies_CompanyId",
                table: "company_invitations",
                column: "CompanyId",
                principalTable: "companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_company_members_AspNetUsers_UserId",
                table: "company_members",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "PublicId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_company_members_companies_CompanyId",
                table: "company_members",
                column: "CompanyId",
                principalTable: "companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_consents_AspNetUsers_UserId",
                table: "consents",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "PublicId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_credentials_AspNetUsers_UserId",
                table: "credentials",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "PublicId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_device_AspNetUsers_UserId",
                table: "device",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "PublicId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_device_approval_requests_AspNetUsers_UserId",
                table: "device_approval_requests",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "PublicId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_id_documents_AspNetUsers_UserId",
                table: "id_documents",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "PublicId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_identity_checks_AspNetUsers_UserId",
                table: "identity_checks",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "PublicId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_next_of_kins_AspNetUsers_UserId",
                table: "next_of_kins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "PublicId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_selfie_checks_AspNetUsers_UserId",
                table: "selfie_checks",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "PublicId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_signatures_AspNetUsers_UserId",
                table: "signatures",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "PublicId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tax_residencies_AspNetUsers_UserId",
                table: "tax_residencies",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "PublicId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tier_upgrade_items_tier_upgrade_requests_TierUpgradeRequest~",
                table: "tier_upgrade_items",
                column: "TierUpgradeRequestId",
                principalTable: "tier_upgrade_requests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tier_upgrade_requests_AspNetUsers_UserId",
                table: "tier_upgrade_requests",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "PublicId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_two_factor_methods_AspNetUsers_UserId",
                table: "two_factor_methods",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "PublicId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_addresses_AspNetUsers_UserId",
                table: "addresses");

            migrationBuilder.DropForeignKey(
                name: "FK_agreement_acceptances_AspNetUsers_UserId",
                table: "agreement_acceptances");

            migrationBuilder.DropForeignKey(
                name: "FK_agreement_acceptances_agreement_versions_AgreementVersionId",
                table: "agreement_acceptances");

            migrationBuilder.DropForeignKey(
                name: "FK_companies_AspNetUsers_CreatedByUserId",
                table: "companies");

            migrationBuilder.DropForeignKey(
                name: "FK_company_documents_companies_CompanyId",
                table: "company_documents");

            migrationBuilder.DropForeignKey(
                name: "FK_company_invitations_companies_CompanyId",
                table: "company_invitations");

            migrationBuilder.DropForeignKey(
                name: "FK_company_members_AspNetUsers_UserId",
                table: "company_members");

            migrationBuilder.DropForeignKey(
                name: "FK_company_members_companies_CompanyId",
                table: "company_members");

            migrationBuilder.DropForeignKey(
                name: "FK_consents_AspNetUsers_UserId",
                table: "consents");

            migrationBuilder.DropForeignKey(
                name: "FK_credentials_AspNetUsers_UserId",
                table: "credentials");

            migrationBuilder.DropForeignKey(
                name: "FK_device_AspNetUsers_UserId",
                table: "device");

            migrationBuilder.DropForeignKey(
                name: "FK_device_approval_requests_AspNetUsers_UserId",
                table: "device_approval_requests");

            migrationBuilder.DropForeignKey(
                name: "FK_id_documents_AspNetUsers_UserId",
                table: "id_documents");

            migrationBuilder.DropForeignKey(
                name: "FK_identity_checks_AspNetUsers_UserId",
                table: "identity_checks");

            migrationBuilder.DropForeignKey(
                name: "FK_next_of_kins_AspNetUsers_UserId",
                table: "next_of_kins");

            migrationBuilder.DropForeignKey(
                name: "FK_selfie_checks_AspNetUsers_UserId",
                table: "selfie_checks");

            migrationBuilder.DropForeignKey(
                name: "FK_signatures_AspNetUsers_UserId",
                table: "signatures");

            migrationBuilder.DropForeignKey(
                name: "FK_tax_residencies_AspNetUsers_UserId",
                table: "tax_residencies");

            migrationBuilder.DropForeignKey(
                name: "FK_tier_upgrade_items_tier_upgrade_requests_TierUpgradeRequest~",
                table: "tier_upgrade_items");

            migrationBuilder.DropForeignKey(
                name: "FK_tier_upgrade_requests_AspNetUsers_UserId",
                table: "tier_upgrade_requests");

            migrationBuilder.DropForeignKey(
                name: "FK_two_factor_methods_AspNetUsers_UserId",
                table: "two_factor_methods");

            migrationBuilder.DropPrimaryKey(
                name: "PK_signatures",
                table: "signatures");

            migrationBuilder.DropPrimaryKey(
                name: "PK_credentials",
                table: "credentials");

            migrationBuilder.DropPrimaryKey(
                name: "PK_consents",
                table: "consents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_companies",
                table: "companies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_addresses",
                table: "addresses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_two_factor_methods",
                table: "two_factor_methods");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tier_upgrade_requests",
                table: "tier_upgrade_requests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tier_upgrade_items",
                table: "tier_upgrade_items");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tier_limits",
                table: "tier_limits");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tax_residencies",
                table: "tax_residencies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_signIn_attempts",
                table: "signIn_attempts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_selfie_checks",
                table: "selfie_checks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_next_of_kins",
                table: "next_of_kins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_identity_checks",
                table: "identity_checks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_id_documents",
                table: "id_documents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_device_approval_requests",
                table: "device_approval_requests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_device",
                table: "device");

            migrationBuilder.DropPrimaryKey(
                name: "PK_company_members",
                table: "company_members");

            migrationBuilder.DropPrimaryKey(
                name: "PK_company_invitations",
                table: "company_invitations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_company_documents",
                table: "company_documents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_agreement_versions",
                table: "agreement_versions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_agreement_acceptances",
                table: "agreement_acceptances");

            migrationBuilder.RenameTable(
                name: "signatures",
                newName: "Signatures");

            migrationBuilder.RenameTable(
                name: "credentials",
                newName: "Credentials");

            migrationBuilder.RenameTable(
                name: "consents",
                newName: "Consents");

            migrationBuilder.RenameTable(
                name: "companies",
                newName: "Companies");

            migrationBuilder.RenameTable(
                name: "addresses",
                newName: "Addresses");

            migrationBuilder.RenameTable(
                name: "two_factor_methods",
                newName: "TwoFactorMethods");

            migrationBuilder.RenameTable(
                name: "tier_upgrade_requests",
                newName: "TierUpgradeRequests");

            migrationBuilder.RenameTable(
                name: "tier_upgrade_items",
                newName: "TierUpgradeItems");

            migrationBuilder.RenameTable(
                name: "tier_limits",
                newName: "TierLimits");

            migrationBuilder.RenameTable(
                name: "tax_residencies",
                newName: "TaxResidencies");

            migrationBuilder.RenameTable(
                name: "signIn_attempts",
                newName: "SignInAttempts");

            migrationBuilder.RenameTable(
                name: "selfie_checks",
                newName: "SelfieChecks");

            migrationBuilder.RenameTable(
                name: "next_of_kins",
                newName: "NextOfKins");

            migrationBuilder.RenameTable(
                name: "identity_checks",
                newName: "IdentityChecks");

            migrationBuilder.RenameTable(
                name: "id_documents",
                newName: "IdDocuments");

            migrationBuilder.RenameTable(
                name: "device_approval_requests",
                newName: "DeviceApprovalRequests");

            migrationBuilder.RenameTable(
                name: "device",
                newName: "Devices");

            migrationBuilder.RenameTable(
                name: "company_members",
                newName: "CompanyMembers");

            migrationBuilder.RenameTable(
                name: "company_invitations",
                newName: "CompanyInvitations");

            migrationBuilder.RenameTable(
                name: "company_documents",
                newName: "CompanyDocuments");

            migrationBuilder.RenameTable(
                name: "agreement_versions",
                newName: "AgreementVersions");

            migrationBuilder.RenameTable(
                name: "agreement_acceptances",
                newName: "AgreementAcceptances");

            migrationBuilder.RenameIndex(
                name: "IX_signatures_UserId",
                table: "Signatures",
                newName: "IX_Signatures_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_credentials_UserId",
                table: "Credentials",
                newName: "IX_Credentials_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_consents_UserId",
                table: "Consents",
                newName: "IX_Consents_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_companies_RcNumber",
                table: "Companies",
                newName: "IX_Companies_RcNumber");

            migrationBuilder.RenameIndex(
                name: "IX_companies_CreatedByUserId",
                table: "Companies",
                newName: "IX_Companies_CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_addresses_UserId",
                table: "Addresses",
                newName: "IX_Addresses_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_two_factor_methods_UserId",
                table: "TwoFactorMethods",
                newName: "IX_TwoFactorMethods_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_tier_upgrade_requests_UserId",
                table: "TierUpgradeRequests",
                newName: "IX_TierUpgradeRequests_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_tier_upgrade_items_TierUpgradeRequestId",
                table: "TierUpgradeItems",
                newName: "IX_TierUpgradeItems_TierUpgradeRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_tier_limits_Tier",
                table: "TierLimits",
                newName: "IX_TierLimits_Tier");

            migrationBuilder.RenameIndex(
                name: "IX_tax_residencies_UserId",
                table: "TaxResidencies",
                newName: "IX_TaxResidencies_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_selfie_checks_UserId",
                table: "SelfieChecks",
                newName: "IX_SelfieChecks_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_next_of_kins_UserId",
                table: "NextOfKins",
                newName: "IX_NextOfKins_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_identity_checks_UserId",
                table: "IdentityChecks",
                newName: "IX_IdentityChecks_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_id_documents_UserId",
                table: "IdDocuments",
                newName: "IX_IdDocuments_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_device_approval_requests_UserId",
                table: "DeviceApprovalRequests",
                newName: "IX_DeviceApprovalRequests_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_device_UserId",
                table: "Devices",
                newName: "IX_Devices_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_company_members_UserId",
                table: "CompanyMembers",
                newName: "IX_CompanyMembers_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_company_members_CompanyId",
                table: "CompanyMembers",
                newName: "IX_CompanyMembers_CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_company_invitations_TokenHash",
                table: "CompanyInvitations",
                newName: "IX_CompanyInvitations_TokenHash");

            migrationBuilder.RenameIndex(
                name: "IX_company_invitations_CompanyId",
                table: "CompanyInvitations",
                newName: "IX_CompanyInvitations_CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_company_documents_CompanyId",
                table: "CompanyDocuments",
                newName: "IX_CompanyDocuments_CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_agreement_versions_Type_Version",
                table: "AgreementVersions",
                newName: "IX_AgreementVersions_Type_Version");

            migrationBuilder.RenameIndex(
                name: "IX_agreement_acceptances_UserId",
                table: "AgreementAcceptances",
                newName: "IX_AgreementAcceptances_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_agreement_acceptances_AgreementVersionId",
                table: "AgreementAcceptances",
                newName: "IX_AgreementAcceptances_AgreementVersionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Signatures",
                table: "Signatures",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Credentials",
                table: "Credentials",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Consents",
                table: "Consents",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Companies",
                table: "Companies",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Addresses",
                table: "Addresses",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TwoFactorMethods",
                table: "TwoFactorMethods",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TierUpgradeRequests",
                table: "TierUpgradeRequests",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TierUpgradeItems",
                table: "TierUpgradeItems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TierLimits",
                table: "TierLimits",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaxResidencies",
                table: "TaxResidencies",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SignInAttempts",
                table: "SignInAttempts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SelfieChecks",
                table: "SelfieChecks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NextOfKins",
                table: "NextOfKins",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IdentityChecks",
                table: "IdentityChecks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IdDocuments",
                table: "IdDocuments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeviceApprovalRequests",
                table: "DeviceApprovalRequests",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Devices",
                table: "Devices",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompanyMembers",
                table: "CompanyMembers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompanyInvitations",
                table: "CompanyInvitations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompanyDocuments",
                table: "CompanyDocuments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AgreementVersions",
                table: "AgreementVersions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AgreementAcceptances",
                table: "AgreementAcceptances",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_AspNetUsers_UserId",
                table: "Addresses",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "PublicId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AgreementAcceptances_AgreementVersions_AgreementVersionId",
                table: "AgreementAcceptances",
                column: "AgreementVersionId",
                principalTable: "AgreementVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AgreementAcceptances_AspNetUsers_UserId",
                table: "AgreementAcceptances",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "PublicId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_AspNetUsers_CreatedByUserId",
                table: "Companies",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "PublicId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyDocuments_Companies_CompanyId",
                table: "CompanyDocuments",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyInvitations_Companies_CompanyId",
                table: "CompanyInvitations",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyMembers_AspNetUsers_UserId",
                table: "CompanyMembers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "PublicId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyMembers_Companies_CompanyId",
                table: "CompanyMembers",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Consents_AspNetUsers_UserId",
                table: "Consents",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "PublicId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Credentials_AspNetUsers_UserId",
                table: "Credentials",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "PublicId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceApprovalRequests_AspNetUsers_UserId",
                table: "DeviceApprovalRequests",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "PublicId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_AspNetUsers_UserId",
                table: "Devices",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "PublicId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IdDocuments_AspNetUsers_UserId",
                table: "IdDocuments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "PublicId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IdentityChecks_AspNetUsers_UserId",
                table: "IdentityChecks",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "PublicId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NextOfKins_AspNetUsers_UserId",
                table: "NextOfKins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "PublicId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SelfieChecks_AspNetUsers_UserId",
                table: "SelfieChecks",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "PublicId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Signatures_AspNetUsers_UserId",
                table: "Signatures",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "PublicId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaxResidencies_AspNetUsers_UserId",
                table: "TaxResidencies",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "PublicId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TierUpgradeItems_TierUpgradeRequests_TierUpgradeRequestId",
                table: "TierUpgradeItems",
                column: "TierUpgradeRequestId",
                principalTable: "TierUpgradeRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TierUpgradeRequests_AspNetUsers_UserId",
                table: "TierUpgradeRequests",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "PublicId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TwoFactorMethods_AspNetUsers_UserId",
                table: "TwoFactorMethods",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "PublicId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
