using X39.Software.ExamMaker.Api.DataTransferObjects;
using X39.Software.ExamMaker.Models;

namespace X39.Software.ExamMaker.WebApp.Services.ExamAnswerRepository;

public interface IExamAnswerRepository
{
    Task<long> GetAllCountAsync(
        Guid examIdentifier,
        Guid examTopicIdentifier,
        Guid examQuestionIdentifier,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<ExamAnswerListingDto>> GetAllAsync(
        Guid examIdentifier,
        Guid examTopicIdentifier,
        Guid examQuestionIdentifier,
        int skip,
        int take,
        CancellationToken cancellationToken = default
    );

    Task UpdateAsync(
        Guid examIdentifier,
        Guid topicIdentifier,
        Guid questionIdentifier,
        Guid answerIdentifier,
        UpdateValue<string>? answer,
        UpdateValue<string?>? reason,
        UpdateValue<bool>? isCorrect,
        CancellationToken cancellationToken = default
    );

    Task<ExamAnswerListingDto> CreateAsync(
        Guid examIdentifier,
        Guid topicIdentifier,
        Guid questionIdentifier,
        string answer,
        string? reason,
        bool isCorrect,
        CancellationToken cancellationToken = default
    );

    Task DeleteAsync(
        Guid examIdentifier,
        Guid topicIdentifier,
        Guid questionIdentifier,
        Guid answerIdentifier,
        CancellationToken cancellationToken = default
    );
}
