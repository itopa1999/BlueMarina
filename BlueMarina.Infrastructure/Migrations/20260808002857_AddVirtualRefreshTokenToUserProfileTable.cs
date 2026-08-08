using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlueMarina.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVirtualRefreshTokenToUserProfileTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_refresh_tokens_AspNetUsers_UserProfileId",
                table: "refresh_tokens");

            migrationBuilder.DropForeignKey(
                name: "FK_refresh_tokens_user_profiles_UserProfileId1",
                table: "refresh_tokens");

            migrationBuilder.DropIndex(
                name: "IX_refresh_tokens_UserProfileId1",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "UserProfileId1",
                table: "refresh_tokens");

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "refresh_tokens",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_UserId",
                table: "refresh_tokens",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_refresh_tokens_AspNetUsers_UserId",
                table: "refresh_tokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_refresh_tokens_user_profiles_UserProfileId",
                table: "refresh_tokens",
                column: "UserProfileId",
                principalTable: "user_profiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_refresh_tokens_AspNetUsers_UserId",
                table: "refresh_tokens");

            migrationBuilder.DropForeignKey(
                name: "FK_refresh_tokens_user_profiles_UserProfileId",
                table: "refresh_tokens");

            migrationBuilder.DropIndex(
                name: "IX_refresh_tokens_UserId",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "refresh_tokens");

            migrationBuilder.AddColumn<long>(
                name: "UserProfileId1",
                table: "refresh_tokens",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_UserProfileId1",
                table: "refresh_tokens",
                column: "UserProfileId1");

            migrationBuilder.AddForeignKey(
                name: "FK_refresh_tokens_AspNetUsers_UserProfileId",
                table: "refresh_tokens",
                column: "UserProfileId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_refresh_tokens_user_profiles_UserProfileId1",
                table: "refresh_tokens",
                column: "UserProfileId1",
                principalTable: "user_profiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
