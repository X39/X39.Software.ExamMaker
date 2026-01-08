namespace X39.Software.ExamMaker.Api.DataTransferObjects;

public record PdfPreviewDto(ImageDto[]? Images, string? ErrorMessage, string? StackTrace);
