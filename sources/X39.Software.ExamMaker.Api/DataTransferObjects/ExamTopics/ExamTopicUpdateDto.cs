namespace X39.Software.ExamMaker.Api.DataTransferObjects.ExamTopics;

public sealed record ExamTopicUpdateDto(
    UpdateValue<string>? Title,
    UpdateValue<int?>? QuestionAmountToTake);
