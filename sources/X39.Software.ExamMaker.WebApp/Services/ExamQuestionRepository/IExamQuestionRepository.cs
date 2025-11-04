using X39.Software.ExamMaker.Models;

namespace X39.Software.ExamMaker.WebApp.Services.ExamQuestionRepository;

public interface IExamQuestionRepository
{
    Task<long> GetAllExamQuestionsCountAsync(
        Guid examIdentifier,
        Guid examTopicIdentifier,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<ExamQuestionListingDto>> GetAllExamQuestionsAsync(
        Guid examIdentifier,
        Guid examTopicIdentifier,
        int skip,
        int take,
        CancellationToken cancellationToken = default
    );
}
