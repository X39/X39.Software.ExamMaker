using X39.Software.ExamMaker.Models;

namespace X39.Software.ExamMaker.WebApp.Services.ExamAnswerRepository;

public interface IExamAnswerRepository
{
    Task<long> GetAllExamAnswersCountAsync(
        Guid examIdentifier,
        Guid examTopicIdentifier,
        Guid examQuestionIdentifier,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<ExamAnswerListingDto>> GetAllExamAnswersAsync(
        Guid examIdentifier,
        Guid examTopicIdentifier,
        Guid examQuestionIdentifier,
        int skip,
        int take,
        CancellationToken cancellationToken = default
    );
}
