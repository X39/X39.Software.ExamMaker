using X39.Software.ExamMaker.Models;

namespace X39.Software.ExamMaker.WebApp.Services.ExamTopicRepository;

internal sealed class ExamTopicRepository(IHttpClientFactory httpClientFactory, BaseUrl baseUrl)
    : RepositoryBase(httpClientFactory, baseUrl), IExamTopicRepository
{
    public async Task<long> GetAllExamTopicsCountAsync(
        Guid examIdentifier,
        CancellationToken cancellationToken = default
    )
    {
        var result = await Client.Exam[examIdentifier]
            .Topic
            .All
            .Count
            .GetAsync(cancellationToken: cancellationToken);
        if (result is null)
            throw new Exception("Server responded with null");
        return result.Value;
    }

    public async Task<IReadOnlyCollection<ExamTopicListingDto>> GetAllExamTopicsAsync(
        Guid examIdentifier,
        int skip,
        int take,
        CancellationToken cancellationToken = default
    )
    {
        var result = await Client.Exam[examIdentifier]
            .Topic
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
        UpdateValue<string>? title,
        UpdateValue<int?>? questionAmountToTake,
        CancellationToken cancellationToken = default
    )
    {
        await Client.Exam[examIdentifier]
            .Topic[topicIdentifier]
            .Emplace
            .PutAsync(
                new ExamTopicUpdateDto
                {
                    Title = title is null ? null : new UpdateValueOfstring { Value = title },
                    QuestionAmountToTake = questionAmountToTake is null
                        ? null
                        : new UpdateValueOfint { Value = questionAmountToTake },
                },
                cancellationToken: cancellationToken
            );
    }

    public async Task<ExamTopicListingDto> CreateAsync(
        Guid examIdentifier,
        string title,
        int? questionAmountToTake,
        CancellationToken cancellationToken = default
    )
    {
        var result = await Client.Exam[examIdentifier]
            .Topic[Guid.NewGuid()]
            .Emplace
            .PutAsync(
                new ExamTopicUpdateDto
                {
                    Title                = new UpdateValueOfstring { Value = title },
                    QuestionAmountToTake = new UpdateValueOfint { Value = questionAmountToTake },
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
        CancellationToken cancellationToken = default
    )
    {
        await Client.Exam[examIdentifier]
            .Topic[topicIdentifier]
            .DeleteAsync(cancellationToken: cancellationToken);
    }
}
