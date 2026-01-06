using System.Diagnostics.CodeAnalysis;
using X39.Software.ExamMaker.Models;

namespace X39.Software.ExamMaker.WebApp.Services.ExamRepository;

public interface IExamRepository
{
    Task<long> GetAllCountAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ExamListingDto>> GetAllAsync(
        int skip,
        int take,
        CancellationToken cancellationToken = default
    );

    Task UpdateAsync(
        Guid identifier,
        UpdateValue<string>? title,
        UpdateValue<string>? preamble,
        CancellationToken cancellationToken = default
    );
    Task<ExamListingDto> CreateAsync(
        string title,
        string preamble,
        CancellationToken cancellationToken = default
    );

    Task<ExamListingDto> GetAsync(Guid examIdentifier, CancellationToken cancellationToken = default);
}
