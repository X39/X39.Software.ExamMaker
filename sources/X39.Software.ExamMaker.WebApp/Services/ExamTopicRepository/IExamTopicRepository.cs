using X39.Software.ExamMaker.Models;

namespace X39.Software.ExamMaker.WebApp.Services.ExamTopicRepository;

public interface IExamTopicRepository
{
    Task<long> GetAllExamTopicsCountAsync(Guid examIdentifier, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ExamTopicListingDto>> GetAllExamTopicsAsync(
        Guid examIdentifier,
        int skip,
        int take,
        CancellationToken cancellationToken = default
    );
}
