using NodaTime;

namespace X39.Software.ExamMaker.Api.DataTransferObjects.ExamAnswers;

public sealed record ExamAnswerUpdateDto(
    UpdateValue<string>? Answer,
    UpdateValue<string?>? Reason,
    UpdateValue<bool>? IsCorrect
);
