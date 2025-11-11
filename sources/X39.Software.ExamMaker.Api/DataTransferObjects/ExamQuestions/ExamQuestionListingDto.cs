using NodaTime;
using X39.Software.ExamMaker.Api.Storage.Exam.Entities;

namespace X39.Software.ExamMaker.Api.DataTransferObjects.ExamQuestions;

public sealed record ExamQuestionListingDto(
    Guid Identifier,
    string Title,
    EQuestionKind Kind,
    int? CorrectAnswersToTake,
    int? IncorrectAnswersToTake,
    DateTimeOffset CreatedAt
);
