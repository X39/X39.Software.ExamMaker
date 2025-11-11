using NodaTime;

namespace X39.Software.ExamMaker.Api.DataTransferObjects.ExamAnswers;

public sealed record ExamAnswerListingDto(
    Guid Identifier,
    string Answer,
    string? Reason,
    bool IsCorrect,
    DateTimeOffset CreatedAt
);
