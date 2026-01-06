using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace X39.Software.ExamMaker.Api.Storage.Exam.Migrations
{
    /// <inheritdoc />
    public partial class Things : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamAnswers_ExamQuestions_ExamQuestionId",
                table: "ExamAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamAnswers_Organizations_OrganizationId",
                table: "ExamAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamQuestions_ExamTopics_ExamTopicId",
                table: "ExamQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamQuestions_Organizations_OrganizationId",
                table: "ExamQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_Exams_Organizations_OrganizationId",
                table: "Exams");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamTopics_Exams_ExamId",
                table: "ExamTopics");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamTopics_Organizations_OrganizationId",
                table: "ExamTopics");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationRegistrationTokens_Organizations_OrganizationId",
                table: "OrganizationRegistrationTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationRegistrationTokens_Users_CreatedById",
                table: "OrganizationRegistrationTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationRegistrationTokens_Users_UsedById",
                table: "OrganizationRegistrationTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Organizations_OrganizationId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTokens_Users_UserId",
                table: "UserTokens");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UserTokens",
                newName: "UserFk");

            migrationBuilder.RenameColumn(
                name: "ExpiresAt",
                table: "UserTokens",
                newName: "RefreshTokenExpiresAt");

            migrationBuilder.RenameIndex(
                name: "IX_UserTokens_UserId",
                table: "UserTokens",
                newName: "IX_UserTokens_UserFk");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "Users",
                newName: "OrganizationFk");

            migrationBuilder.RenameIndex(
                name: "IX_Users_OrganizationId",
                table: "Users",
                newName: "IX_Users_OrganizationFk");

            migrationBuilder.RenameColumn(
                name: "UsedById",
                table: "OrganizationRegistrationTokens",
                newName: "UsedByFk");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "OrganizationRegistrationTokens",
                newName: "OrganizationFk");

            migrationBuilder.RenameColumn(
                name: "CreatedById",
                table: "OrganizationRegistrationTokens",
                newName: "CreatedByFk");

            migrationBuilder.RenameIndex(
                name: "IX_OrganizationRegistrationTokens_UsedById",
                table: "OrganizationRegistrationTokens",
                newName: "IX_OrganizationRegistrationTokens_UsedByFk");

            migrationBuilder.RenameIndex(
                name: "IX_OrganizationRegistrationTokens_OrganizationId",
                table: "OrganizationRegistrationTokens",
                newName: "IX_OrganizationRegistrationTokens_OrganizationFk");

            migrationBuilder.RenameIndex(
                name: "IX_OrganizationRegistrationTokens_CreatedById",
                table: "OrganizationRegistrationTokens",
                newName: "IX_OrganizationRegistrationTokens_CreatedByFk");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "ExamTopics",
                newName: "OrganizationFk");

            migrationBuilder.RenameColumn(
                name: "ExamId",
                table: "ExamTopics",
                newName: "ExamFk");

            migrationBuilder.RenameIndex(
                name: "IX_ExamTopics_OrganizationId",
                table: "ExamTopics",
                newName: "IX_ExamTopics_OrganizationFk");

            migrationBuilder.RenameIndex(
                name: "IX_ExamTopics_ExamId",
                table: "ExamTopics",
                newName: "IX_ExamTopics_ExamFk");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "Exams",
                newName: "OrganizationFk");

            migrationBuilder.RenameIndex(
                name: "IX_Exams_OrganizationId",
                table: "Exams",
                newName: "IX_Exams_OrganizationFk");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "ExamQuestions",
                newName: "OrganizationFk");

            migrationBuilder.RenameColumn(
                name: "ExamTopicId",
                table: "ExamQuestions",
                newName: "ExamTopicFk");

            migrationBuilder.RenameIndex(
                name: "IX_ExamQuestions_OrganizationId",
                table: "ExamQuestions",
                newName: "IX_ExamQuestions_OrganizationFk");

            migrationBuilder.RenameIndex(
                name: "IX_ExamQuestions_ExamTopicId",
                table: "ExamQuestions",
                newName: "IX_ExamQuestions_ExamTopicFk");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "ExamAnswers",
                newName: "OrganizationFk");

            migrationBuilder.RenameColumn(
                name: "ExamQuestionId",
                table: "ExamAnswers",
                newName: "ExamQuestionFk");

            migrationBuilder.RenameIndex(
                name: "IX_ExamAnswers_OrganizationId",
                table: "ExamAnswers",
                newName: "IX_ExamAnswers_OrganizationFk");

            migrationBuilder.RenameIndex(
                name: "IX_ExamAnswers_ExamQuestionId",
                table: "ExamAnswers",
                newName: "IX_ExamAnswers_ExamQuestionFk");

            migrationBuilder.AlterColumn<string>(
                name: "RefreshToken",
                table: "UserTokens",
                type: "character varying(88)",
                maxLength: 88,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Organizations",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Identifier",
                table: "Organizations",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "OrganizationRegistrationTokens",
                type: "character varying(340)",
                maxLength: 340,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Identifier = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsInternalRole = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleUser",
                columns: table => new
                {
                    RolesId = table.Column<long>(type: "bigint", nullable: false),
                    UsersId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleUser", x => new { x.RolesId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_RoleUser_Roles_RolesId",
                        column: x => x.RolesId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleUser_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Identifier", "IsInternalRole", "Title" },
                values: new object[] { 1L, "Administrators", true, "Administrators" });

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Identifier",
                table: "Roles",
                column: "Identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleUser_UsersId",
                table: "RoleUser",
                column: "UsersId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamAnswers_ExamQuestions_ExamQuestionFk",
                table: "ExamAnswers",
                column: "ExamQuestionFk",
                principalTable: "ExamQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamAnswers_Organizations_OrganizationFk",
                table: "ExamAnswers",
                column: "OrganizationFk",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamQuestions_ExamTopics_ExamTopicFk",
                table: "ExamQuestions",
                column: "ExamTopicFk",
                principalTable: "ExamTopics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamQuestions_Organizations_OrganizationFk",
                table: "ExamQuestions",
                column: "OrganizationFk",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Exams_Organizations_OrganizationFk",
                table: "Exams",
                column: "OrganizationFk",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamTopics_Exams_ExamFk",
                table: "ExamTopics",
                column: "ExamFk",
                principalTable: "Exams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamTopics_Organizations_OrganizationFk",
                table: "ExamTopics",
                column: "OrganizationFk",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationRegistrationTokens_Organizations_OrganizationFk",
                table: "OrganizationRegistrationTokens",
                column: "OrganizationFk",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationRegistrationTokens_Users_CreatedByFk",
                table: "OrganizationRegistrationTokens",
                column: "CreatedByFk",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationRegistrationTokens_Users_UsedByFk",
                table: "OrganizationRegistrationTokens",
                column: "UsedByFk",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Organizations_OrganizationFk",
                table: "Users",
                column: "OrganizationFk",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTokens_Users_UserFk",
                table: "UserTokens",
                column: "UserFk",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamAnswers_ExamQuestions_ExamQuestionFk",
                table: "ExamAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamAnswers_Organizations_OrganizationFk",
                table: "ExamAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamQuestions_ExamTopics_ExamTopicFk",
                table: "ExamQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamQuestions_Organizations_OrganizationFk",
                table: "ExamQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_Exams_Organizations_OrganizationFk",
                table: "Exams");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamTopics_Exams_ExamFk",
                table: "ExamTopics");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamTopics_Organizations_OrganizationFk",
                table: "ExamTopics");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationRegistrationTokens_Organizations_OrganizationFk",
                table: "OrganizationRegistrationTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationRegistrationTokens_Users_CreatedByFk",
                table: "OrganizationRegistrationTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationRegistrationTokens_Users_UsedByFk",
                table: "OrganizationRegistrationTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Organizations_OrganizationFk",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTokens_Users_UserFk",
                table: "UserTokens");

            migrationBuilder.DropTable(
                name: "RoleUser");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.RenameColumn(
                name: "UserFk",
                table: "UserTokens",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "RefreshTokenExpiresAt",
                table: "UserTokens",
                newName: "ExpiresAt");

            migrationBuilder.RenameIndex(
                name: "IX_UserTokens_UserFk",
                table: "UserTokens",
                newName: "IX_UserTokens_UserId");

            migrationBuilder.RenameColumn(
                name: "OrganizationFk",
                table: "Users",
                newName: "OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_OrganizationFk",
                table: "Users",
                newName: "IX_Users_OrganizationId");

            migrationBuilder.RenameColumn(
                name: "UsedByFk",
                table: "OrganizationRegistrationTokens",
                newName: "UsedById");

            migrationBuilder.RenameColumn(
                name: "OrganizationFk",
                table: "OrganizationRegistrationTokens",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "CreatedByFk",
                table: "OrganizationRegistrationTokens",
                newName: "CreatedById");

            migrationBuilder.RenameIndex(
                name: "IX_OrganizationRegistrationTokens_UsedByFk",
                table: "OrganizationRegistrationTokens",
                newName: "IX_OrganizationRegistrationTokens_UsedById");

            migrationBuilder.RenameIndex(
                name: "IX_OrganizationRegistrationTokens_OrganizationFk",
                table: "OrganizationRegistrationTokens",
                newName: "IX_OrganizationRegistrationTokens_OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_OrganizationRegistrationTokens_CreatedByFk",
                table: "OrganizationRegistrationTokens",
                newName: "IX_OrganizationRegistrationTokens_CreatedById");

            migrationBuilder.RenameColumn(
                name: "OrganizationFk",
                table: "ExamTopics",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "ExamFk",
                table: "ExamTopics",
                newName: "ExamId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamTopics_OrganizationFk",
                table: "ExamTopics",
                newName: "IX_ExamTopics_OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamTopics_ExamFk",
                table: "ExamTopics",
                newName: "IX_ExamTopics_ExamId");

            migrationBuilder.RenameColumn(
                name: "OrganizationFk",
                table: "Exams",
                newName: "OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_Exams_OrganizationFk",
                table: "Exams",
                newName: "IX_Exams_OrganizationId");

            migrationBuilder.RenameColumn(
                name: "OrganizationFk",
                table: "ExamQuestions",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "ExamTopicFk",
                table: "ExamQuestions",
                newName: "ExamTopicId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamQuestions_OrganizationFk",
                table: "ExamQuestions",
                newName: "IX_ExamQuestions_OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamQuestions_ExamTopicFk",
                table: "ExamQuestions",
                newName: "IX_ExamQuestions_ExamTopicId");

            migrationBuilder.RenameColumn(
                name: "OrganizationFk",
                table: "ExamAnswers",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "ExamQuestionFk",
                table: "ExamAnswers",
                newName: "ExamQuestionId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamAnswers_OrganizationFk",
                table: "ExamAnswers",
                newName: "IX_ExamAnswers_OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamAnswers_ExamQuestionFk",
                table: "ExamAnswers",
                newName: "IX_ExamAnswers_ExamQuestionId");

            migrationBuilder.AlterColumn<string>(
                name: "RefreshToken",
                table: "UserTokens",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(88)",
                oldMaxLength: 88);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Organizations",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Identifier",
                table: "Organizations",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "OrganizationRegistrationTokens",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(340)",
                oldMaxLength: 340);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamAnswers_ExamQuestions_ExamQuestionId",
                table: "ExamAnswers",
                column: "ExamQuestionId",
                principalTable: "ExamQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamAnswers_Organizations_OrganizationId",
                table: "ExamAnswers",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamQuestions_ExamTopics_ExamTopicId",
                table: "ExamQuestions",
                column: "ExamTopicId",
                principalTable: "ExamTopics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamQuestions_Organizations_OrganizationId",
                table: "ExamQuestions",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Exams_Organizations_OrganizationId",
                table: "Exams",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamTopics_Exams_ExamId",
                table: "ExamTopics",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamTopics_Organizations_OrganizationId",
                table: "ExamTopics",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationRegistrationTokens_Organizations_OrganizationId",
                table: "OrganizationRegistrationTokens",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationRegistrationTokens_Users_CreatedById",
                table: "OrganizationRegistrationTokens",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationRegistrationTokens_Users_UsedById",
                table: "OrganizationRegistrationTokens",
                column: "UsedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Organizations_OrganizationId",
                table: "Users",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTokens_Users_UserId",
                table: "UserTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
