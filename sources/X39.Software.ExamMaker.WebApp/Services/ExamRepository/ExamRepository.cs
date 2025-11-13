using X39.Software.ExamMaker.Api.DataTransferObjects;
using X39.Software.ExamMaker.Models;

namespace X39.Software.ExamMaker.WebApp.Services.ExamRepository;

public sealed class ExamRepository(IHttpClientFactory httpClientFactory, BaseUrl baseUrl)
    : RepositoryBase(httpClientFactory, baseUrl), IExamRepository
{
    public async Task<long> GetAllCountAsync(CancellationToken cancellationToken = default)
    {
        var result = await Client.Exam.All.Count.GetAsync(cancellationToken: cancellationToken);
        if (result is null)
            throw new Exception("Server responded with null");
        return result.Value;
    }

    public async Task<IReadOnlyCollection<ExamListingDto>> GetAllAsync(
        int skip,
        int take,
        CancellationToken cancellationToken = default
    )
    {
        var result = await Client.Exam.All.GetAsync(
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
        Guid identifier,
        UpdateValue<string>? title,
        UpdateValue<string>? preamble,
        CancellationToken cancellationToken = default
    )
    {
        await Client.Exam[identifier]
            .Emplace
            .PutAsync(
                new ExamUpdateDto
                {
                    Title    = title is null ? null : new NullableOfUpdateValueOfstring { Value    = title },
                    Preamble = preamble is null ? null : new NullableOfUpdateValueOfstring { Value = preamble },
                },
                cancellationToken: cancellationToken
            );
    }

    public async Task<ExamListingDto> CreateAsync(
        string title,
        string preamble,
        CancellationToken cancellationToken = default
    )
    {
        var result = await Client.Exam[Guid.NewGuid()]
            .Emplace
            .PutAsync(
                new ExamUpdateDto
                {
                    Title    = new NullableOfUpdateValueOfstring { Value    = title },
                    Preamble = new NullableOfUpdateValueOfstring { Value = preamble },
                },
                cancellationToken: cancellationToken
            );
        if (result is null)
            throw new Exception("Server responded with null");
        return result;
    }

    public async Task<ExamListingDto> GetAsync(Guid examIdentifier, CancellationToken cancellationToken = default)
    {
        var result = await Client.Exam[examIdentifier]
            .GetAsync(cancellationToken: cancellationToken);
        if (result is null)
            throw new Exception("Server responded with null");
        return result;
    }
}
