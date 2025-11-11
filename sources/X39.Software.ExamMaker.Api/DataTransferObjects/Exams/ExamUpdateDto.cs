namespace X39.Software.ExamMaker.Api.DataTransferObjects.Exams;

public sealed record ExamUpdateDto(UpdateValue<string>? Title, UpdateValue<string>? Preamble);
