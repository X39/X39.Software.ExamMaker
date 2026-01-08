using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace X39.Software.ExamMaker.Api.Storage.Migrations
{
    /// <inheritdoc />
    public partial class AddStringPdfTemplateToExam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PdfTemplate",
                table: "Exams",
                type: "character varying(1048575)",
                maxLength: 1048575,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PdfTemplate",
                table: "Exams");
        }
    }
}
