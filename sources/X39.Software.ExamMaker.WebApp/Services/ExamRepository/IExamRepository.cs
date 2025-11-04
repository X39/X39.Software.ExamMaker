using System.Diagnostics.CodeAnalysis;
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
        string? title,
        string? preamble,
        CancellationToken cancellationToken = default
    );
}
