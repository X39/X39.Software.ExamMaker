using X39.Software.ExamMaker.Api.Storage.Exam.Entities;

namespace X39.Software.ExamMaker.Api.DataTransferObjects.ExamQuestions;

public sealed record ExamQuestionUpdateDto(
    UpdateValue<string>? Title,
    UpdateValue<EQuestionKind>? Kind,
    UpdateValue<int?>? CorrectAnswersToTake,
    UpdateValue<int?>? IncorrectAnswersToTake);
