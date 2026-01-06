using X39.Software.ExamMaker.Models;

namespace X39.Software.ExamMaker.WebApp.Services.ExamAnswerRepository;

internal sealed class ExamAnswerRepository(IHttpClientFactory httpClientFactory, BaseUrl baseUrl)
    : RepositoryBase(httpClientFactory, baseUrl), IExamAnswerRepository
{
    public async Task<long> GetAllCountAsync(
        Guid examIdentifier,
        Guid examTopicIdentifier,
        Guid examQuestionIdentifier,
        CancellationToken cancellationToken = default
    )
    {
        var result = await Client.Exam[examIdentifier]
            .Topic[examTopicIdentifier]
            .Question[examQuestionIdentifier]
            .Answer
            .All
            .Count
            .GetAsync(cancellationToken: cancellationToken);
        if (result is null)
            throw new Exception("Server responded with null");
        return result.Value;
    }

    public async Task<IReadOnlyCollection<ExamAnswerListingDto>> GetAllAsync(
        Guid examIdentifier,
        Guid examTopicIdentifier,
        Guid examQuestionIdentifier,
        int skip,
        int take,
        CancellationToken cancellationToken = default
    )
    {
        var result = await Client.Exam[examIdentifier]
            .Topic[examTopicIdentifier]
            .Question[examQuestionIdentifier]
            .Answer
            .All
            .GetAsync(
                conf =>
                {
                    conf.QueryParameters.Skip = skip;
                    conf.QueryParameters.Take = take;
                },
                cancellationToken
            );
        if (result is null)
            throw new Exception("Server responded with null");
        return result.AsReadOnly();
    }

    public async Task UpdateAsync(
        Guid examIdentifier,
        Guid topicIdentifier,
        Guid questionIdentifier,
        Guid answerIdentifier,
        UpdateValue<string>? answer,
        UpdateValue<string?>? reason,
        UpdateValue<bool>? isCorrect,
        CancellationToken cancellationToken = default
    )
    {
        await Client.Exam[examIdentifier]
            .Topic[topicIdentifier]
            .Question[questionIdentifier]
            .Answer[answerIdentifier]
            .Emplace
            .PutAsync(
                new ExamAnswerUpdateDto()
                {
                    Answer    = answer is null ? null : new UpdateValueOfstring { Value    = answer },
                    Reason    = reason is null ? null : new UpdateValueOfstring { Value    = reason },
                    IsCorrect = isCorrect is null ? null : new UpdateValueOfboolean { Value = isCorrect },
                },
                cancellationToken: cancellationToken
            );
    }

    public async Task<ExamAnswerListingDto> CreateAsync(
        Guid examIdentifier,
        Guid topicIdentifier,
        Guid questionIdentifier,
        string answer,
        string? reason,
        bool isCorrect,
        CancellationToken cancellationToken = default
    )
    {
        var result = await Client.Exam[examIdentifier]
            .Topic[topicIdentifier]
            .Question[questionIdentifier]
            .Answer[Guid.NewGuid()]
            .Emplace
            .PutAsync(
                new ExamAnswerUpdateDto
                {
                    Answer    = new UpdateValueOfstring { Value = answer },
                    Reason    = new UpdateValueOfstring { Value = reason },
                    IsCorrect = new UpdateValueOfboolean { Value = isCorrect },
                },
                cancellationToken: cancellationToken
            );
        if (result is null)
            throw new Exception("Server responded with null");
        return result;
    }

    public async Task DeleteAsync(
        Guid examIdentifier,
        Guid topicIdentifier,
        Guid questionIdentifier,
        Guid answerIdentifier,
        CancellationToken cancellationToken = default
    )
    {
        await Client.Exam[examIdentifier]
            .Topic[topicIdentifier]
            .Question[questionIdentifier]
            .Answer[answerIdentifier]
            .DeleteAsync(cancellationToken: cancellationToken);
    }
}
