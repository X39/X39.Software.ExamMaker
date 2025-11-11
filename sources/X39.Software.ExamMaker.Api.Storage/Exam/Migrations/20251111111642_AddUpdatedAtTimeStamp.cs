using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace X39.Software.ExamMaker.Api.Storage.Exam.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdatedAtTimeStamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "ExamTopics",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Instant>(
                name: "UpdatedAt",
                table: "ExamTopics",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L));

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Exams",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Preamble",
                table: "Exams",
                type: "character varying(4095)",
                maxLength: 4095,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Instant>(
                name: "UpdatedAt",
                table: "Exams",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L));

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "ExamQuestions",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Instant>(
                name: "UpdatedAt",
                table: "ExamQuestions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L));

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "ExamAnswers",
                type: "character varying(4095)",
                maxLength: 4095,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Answer",
                table: "ExamAnswers",
                type: "character varying(4095)",
                maxLength: 4095,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Instant>(
                name: "UpdatedAt",
                table: "ExamAnswers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ExamTopics");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ExamQuestions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ExamAnswers");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "ExamTopics",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Exams",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Preamble",
                table: "Exams",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(4095)",
                oldMaxLength: 4095);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "ExamQuestions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "ExamAnswers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4095)",
                oldMaxLength: 4095,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Answer",
                table: "ExamAnswers",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(4095)",
                oldMaxLength: 4095);
        }
    }
}
