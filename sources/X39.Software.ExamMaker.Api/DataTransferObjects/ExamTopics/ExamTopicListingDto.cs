using NodaTime;

namespace X39.Software.ExamMaker.Api.DataTransferObjects.ExamTopics;

public sealed record ExamTopicListingDto(
    Guid Identifier,
    string Title,
    int? QuestionAmountToTake,
    DateTimeOffset CreatedAt
);
