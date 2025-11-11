using X39.Software.ExamMaker.Api.DataTransferObjects;
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
                    Answer    = answer is null ? null : new NullableOfUpdateValueOfstring { Value     = answer },
                    Reason    = reason is null ? null : new NullableOfUpdateValueOfstring { Value     = reason },
                    IsCorrect = isCorrect is null ? null : new NullableOfUpdateValueOfboolean { Value = isCorrect },
                },
                cancellationToken: cancellationToken
            );
    }
}
