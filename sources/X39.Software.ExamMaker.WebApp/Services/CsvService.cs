using System.Globalization;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using X39.Software.ExamMaker.Models;
using X39.Software.ExamMaker.WebApp.Models;
using X39.Software.ExamMaker.WebApp.Services.ExamAnswerRepository;
using X39.Software.ExamMaker.WebApp.Services.ExamQuestionRepository;
using X39.Software.ExamMaker.WebApp.Services.ExamRepository;
using X39.Software.ExamMaker.WebApp.Services.ExamTopicRepository;

namespace X39.Software.ExamMaker.WebApp.Services;

public sealed class CsvService(
    IExamRepository examRepository,
    IExamTopicRepository examTopicRepository,
    IExamQuestionRepository examQuestionRepository,
    IExamAnswerRepository examAnswerRepository)
{
    private static readonly CsvConfiguration Configuration = new(CultureInfo.InvariantCulture)
    {
        Delimiter = ";",
        PrepareHeaderForMatch = args => args.Header.ToLower(),
    };

    public async Task<string> ExportAsync(Guid examIdentifier, CancellationToken cancellationToken = default)
    {
        var exam = await examRepository.GetAsync(examIdentifier, cancellationToken);
        var rows = new List<CsvExamRow>();

        var topics = await examTopicRepository.GetAllExamTopicsAsync(examIdentifier, 0, int.MaxValue, cancellationToken);
        foreach (var topic in topics)
        {
            var questions = await examQuestionRepository.GetAllAsync(examIdentifier, topic.Identifier!.Value, 0,
                int.MaxValue, cancellationToken);
            if (!questions.Any())
            {
                rows.Add(new CsvExamRow
                {
                    ExamTitle = exam.Title ?? string.Empty,
                    ExamPreamble = exam.Preamble ?? string.Empty,
                    TopicTitle = topic.Title ?? string.Empty,
                    TopicQuestionAmountToTake = topic.QuestionAmountToTake,
                });
                continue;
            }

            foreach (var question in questions)
            {
                var answers = await examAnswerRepository.GetAllAsync(examIdentifier, topic.Identifier.Value,
                    question.Identifier!.Value, 0, int.MaxValue, cancellationToken);
                if (!answers.Any())
                {
                    rows.Add(new CsvExamRow
                    {
                        ExamTitle = exam.Title ?? string.Empty,
                        ExamPreamble = exam.Preamble ?? string.Empty,
                        TopicTitle = topic.Title ?? string.Empty,
                        TopicQuestionAmountToTake = topic.QuestionAmountToTake,
                        QuestionTitle = question.Title ?? string.Empty,
                        QuestionKind = question.Kind ?? EQuestionKind.MultipleChoice,
                        QuestionCorrectAnswersToTake = question.CorrectAnswersToTake,
                        QuestionIncorrectAnswersToTake = question.IncorrectAnswersToTake,
                    });
                    continue;
                }

                foreach (var answer in answers)
                {
                    rows.Add(new CsvExamRow
                    {
                        ExamTitle = exam.Title ?? string.Empty,
                        ExamPreamble = exam.Preamble ?? string.Empty,
                        TopicTitle = topic.Title ?? string.Empty,
                        TopicQuestionAmountToTake = topic.QuestionAmountToTake,
                        QuestionTitle = question.Title ?? string.Empty,
                        QuestionKind = question.Kind ?? EQuestionKind.MultipleChoice,
                        QuestionCorrectAnswersToTake = question.CorrectAnswersToTake,
                        QuestionIncorrectAnswersToTake = question.IncorrectAnswersToTake,
                        AnswerText = answer.Answer ?? string.Empty,
                        AnswerIsCorrect = answer.IsCorrect ?? false,
                        AnswerReason = answer.Reason ?? string.Empty,
                    });
                }
            }
        }

        using var writer = new StringWriter();
        using var csv = new CsvWriter(writer, Configuration);
        await csv.WriteRecordsAsync(rows, cancellationToken);
        return writer.ToString();
    }

    public async Task ImportAsync(string csvData, CancellationToken cancellationToken = default)
    {
        using var reader = new StringReader(csvData);
        using var csv = new CsvReader(reader, Configuration);

        var rows = new List<CsvExamRow>();
        await foreach (var row in csv.GetRecordsAsync<CsvExamRow>(cancellationToken))
        {
            rows.Add(row);
        }
        if (rows.Count == 0) return;

        var firstRow = rows.First();
        var exam = await examRepository.CreateAsync(firstRow.ExamTitle, firstRow.ExamPreamble, cancellationToken);
        var examId = exam.Identifier!.Value;

        var topics = rows.GroupBy(r => r.TopicTitle);
        foreach (var topicGroup in topics)
        {
            var topicTitle = topicGroup.Key;
            if (string.IsNullOrWhiteSpace(topicTitle)) continue;

            var firstTopicRow = topicGroup.First();
            var topic = await examTopicRepository.CreateAsync(examId, topicTitle,
                firstTopicRow.TopicQuestionAmountToTake, cancellationToken);
            var topicId = topic.Identifier!.Value;

            var questions = topicGroup.GroupBy(r => r.QuestionTitle);
            foreach (var questionGroup in questions)
            {
                var questionTitle = questionGroup.Key;
                if (string.IsNullOrWhiteSpace(questionTitle)) continue;

                var firstQuestionRow = questionGroup.First();
                var question = await examQuestionRepository.CreateAsync(
                    examId,
                    topicId,
                    questionTitle,
                    firstQuestionRow.QuestionCorrectAnswersToTake,
                    firstQuestionRow.QuestionIncorrectAnswersToTake,
                    firstQuestionRow.QuestionKind,
                    cancellationToken);
                var questionId = question.Identifier!.Value;

                foreach (var row in questionGroup)
                {
                    if (string.IsNullOrWhiteSpace(row.AnswerText)) continue;

                    await examAnswerRepository.CreateAsync(
                        examId,
                        topicId,
                        questionId,
                        row.AnswerText,
                        row.AnswerReason,
                        row.AnswerIsCorrect,
                        cancellationToken);
                }
            }
        }
    }
}
