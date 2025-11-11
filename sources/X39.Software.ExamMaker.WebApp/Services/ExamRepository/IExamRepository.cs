using System.Diagnostics.CodeAnalysis;
using X39.Software.ExamMaker.Api.DataTransferObjects;
using X39.Software.ExamMaker.Models;

namespace X39.Software.ExamMaker.WebApp.Services.ExamRepository;

public interface IExamRepository
{
    Task<long> GetAllExamsCountAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ExamListingDto>> GetAllExamsAsync(
        int skip,
        int take,
        CancellationToken cancellationToken = default
    );

    Task UpdateExamAsync(
        Guid identifier,
        UpdateValue<string>? title,
        UpdateValue<string>? preamble,
        CancellationToken cancellationToken = default
    );

    Task<ExamListingDto> GetExamAsync(Guid examIdentifier, CancellationToken cancellationToken = default);
}
