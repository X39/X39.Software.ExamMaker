using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace X39.Software.ExamMaker.Api.Storage.Exam.Migrations
{
    /// <inheritdoc />
    public partial class AddPreambleToExam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Preamble",
                table: "Exams",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Preamble",
                table: "Exams");
        }
    }
}
