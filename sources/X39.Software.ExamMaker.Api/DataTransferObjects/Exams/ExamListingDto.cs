namespace X39.Software.ExamMaker.Api.DataTransferObjects.Exams;

public sealed record ExamListingDto(Guid Identifier, string Title, string Preamble, string PdfTemplate, DateTimeOffset CreatedAt);
