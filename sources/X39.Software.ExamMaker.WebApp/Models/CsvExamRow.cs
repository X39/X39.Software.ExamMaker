using X39.Software.ExamMaker.Models;

namespace X39.Software.ExamMaker.WebApp.Models;

public sealed class CsvExamRow
{
    public string ExamTitle { get; set; } = string.Empty;
    public string ExamPreamble { get; set; } = string.Empty;
    public string TopicTitle { get; set; } = string.Empty;
    public int? TopicQuestionAmountToTake { get; set; }
    public string QuestionTitle { get; set; } = string.Empty;
    public EQuestionKind QuestionKind { get; set; }
    public int? QuestionCorrectAnswersToTake { get; set; }
    public int? QuestionIncorrectAnswersToTake { get; set; }
    public string AnswerText { get; set; } = string.Empty;
    public bool AnswerIsCorrect { get; set; }
    public string AnswerReason { get; set; } = string.Empty;
}
