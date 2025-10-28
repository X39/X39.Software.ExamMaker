using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace X39.Software.ExamMaker.Api.Storage.Exam.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionCountsToTopicAndAnswerCountsToQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuestionAmountToTake",
                table: "ExamTopics",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CorrectAnswersToTake",
                table: "ExamQuestions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IncorrectAnswersToTake",
                table: "ExamQuestions",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuestionAmountToTake",
                table: "ExamTopics");

            migrationBuilder.DropColumn(
                name: "CorrectAnswersToTake",
                table: "ExamQuestions");

            migrationBuilder.DropColumn(
                name: "IncorrectAnswersToTake",
                table: "ExamQuestions");
        }
    }
}
