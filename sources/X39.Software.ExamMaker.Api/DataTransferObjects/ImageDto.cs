namespace X39.Software.ExamMaker.Api.DataTransferObjects;

public record ImageDto(Memory<byte> Data, string MimeType, string FileName);
