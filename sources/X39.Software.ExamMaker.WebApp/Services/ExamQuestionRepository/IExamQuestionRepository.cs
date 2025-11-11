using X39.Software.ExamMaker.Api.DataTransferObjects;
using X39.Software.ExamMaker.Models;

namespace X39.Software.ExamMaker.WebApp.Services.ExamQuestionRepository;

public interface IExamQuestionRepository
{
    Task<long> GetAllCountAsync(
        Guid examIdentifier,
        Guid examTopicIdentifier,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<ExamQuestionListingDto>> GetAllAsync(
        Guid examIdentifier,
        Guid examTopicIdentifier,
        int skip,
        int take,
        CancellationToken cancellationToken = default
    );

    Task UpdateAsync(
        Guid examIdentifier,
        Guid topicIdentifier,
        Guid questionIdentifier,
        UpdateValue<string>? title,
        UpdateValue<int?>? correctAnswersToTake,
        UpdateValue<int?>? incorrectAnswersToTake,
        UpdateValue<EQuestionKind>? kind,
        CancellationToken cancellationToken = default
    );
}
