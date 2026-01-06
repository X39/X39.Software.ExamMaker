using X39.Software.ExamMaker.Models;

namespace X39.Software.ExamMaker.WebApp.Services.ExamQuestionRepository;

internal sealed class ExamQuestionRepository(IHttpClientFactory httpClientFactory, BaseUrl baseUrl)
    : RepositoryBase(httpClientFactory, baseUrl), IExamQuestionRepository
{
    public async Task<long> GetAllCountAsync(
        Guid examIdentifier,
        Guid examTopicIdentifier,
        CancellationToken cancellationToken = default
    )
    {
        var result = await Client.Exam[examIdentifier]
            .Topic[examTopicIdentifier]
            .Question
            .All
            .Count
            .GetAsync(cancellationToken: cancellationToken);
        if (result is null)
            throw new Exception("Server responded with null");
        return result.Value;
    }

    public async Task<IReadOnlyCollection<ExamQuestionListingDto>> GetAllAsync(
        Guid examIdentifier,
        Guid examTopicIdentifier,
        int skip,
        int take,
        CancellationToken cancellationToken = default
    )
    {
        var result = await Client.Exam[examIdentifier]
            .Topic[examTopicIdentifier]
            .Question
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
        UpdateValue<string>? title,
        UpdateValue<int?>? correctAnswersToTake,
        UpdateValue<int?>? incorrectAnswersToTake,
        UpdateValue<EQuestionKind>? kind,
        CancellationToken cancellationToken = default
    )
    {
        await Client.Exam[examIdentifier]
            .Topic[topicIdentifier]
            .Question[questionIdentifier]
            .Emplace
            .PutAsync(
                new ExamQuestionUpdateDto
                {
                    Title = title is null ? null : new UpdateValueOfstring { Value = title },
                    CorrectAnswersToTake = correctAnswersToTake is null ? null : new UpdateValueOfint { Value = correctAnswersToTake },
                    IncorrectAnswersToTake = incorrectAnswersToTake is null ? null : new UpdateValueOfint { Value = incorrectAnswersToTake },
                    Kind = kind is null ? null : new UpdateValueOfEQuestionKind { Value = kind.Value.Value },
                },
                cancellationToken: cancellationToken
            );
    }

    public async Task<ExamQuestionListingDto> CreateAsync(
        Guid examIdentifier,
        Guid topicIdentifier,
        string title,
        int? correctAnswersToTake,
        int? incorrectAnswersToTake,
        EQuestionKind kind,
        CancellationToken cancellationToken = default
    )
    {
        var result = await Client.Exam[examIdentifier]
            .Topic[topicIdentifier]
            .Question[Guid.NewGuid()]
            .Emplace
            .PutAsync(
                new ExamQuestionUpdateDto
                {
                    Title = new UpdateValueOfstring { Value = title },
                    CorrectAnswersToTake = new UpdateValueOfint { Value = correctAnswersToTake },
                    IncorrectAnswersToTake = new UpdateValueOfint { Value = incorrectAnswersToTake },
                    Kind = new UpdateValueOfEQuestionKind { Value = kind },
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
        CancellationToken cancellationToken = default
    )
    {
        await Client.Exam[examIdentifier]
            .Topic[topicIdentifier]
            .Question[questionIdentifier]
            .DeleteAsync(cancellationToken: cancellationToken);
    }
}
