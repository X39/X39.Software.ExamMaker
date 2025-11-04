using X39.Software.ExamMaker.Models;

namespace X39.Software.ExamMaker.WebApp.Services.ExamRepository;

public sealed class ExamRepository(IHttpClientFactory httpClientFactory, BaseUrl baseUrl)
    : RepositoryBase(httpClientFactory, baseUrl), IExamRepository
{
    public async Task<long> GetAllExamsCountAsync(CancellationToken cancellationToken = default)
    {
        var result = await Client.Exam.All.Count.GetAsync(cancellationToken: cancellationToken);
        if (result is null)
            throw new Exception("Server responded with null");
        return result.Value;
    }

    public async Task<IReadOnlyCollection<ExamListingDto>> GetAllExamsAsync(
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

    public async Task UpdateExamAsync(
        Guid identifier,
        string? title,
        string? preamble,
        CancellationToken cancellationToken = default
    )
    {
        await Client.Exam[identifier]
            .Emplace
            .PutAsync(
                new ExamUpdateDto
                {
                    Title    = title,
                    Preamble = preamble,
                },
                cancellationToken: cancellationToken
            );
    }
}