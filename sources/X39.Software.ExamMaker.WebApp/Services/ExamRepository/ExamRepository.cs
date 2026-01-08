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
        UpdateValue<string>? pdfTemplate,
        CancellationToken cancellationToken = default
    )
    {
        await Client.Exam[identifier]
            .Emplace
            .PutAsync(
                new ExamUpdateDto
                {
                    Title       = title is null ? null : new UpdateValueOfstring { Value       = title },
                    Preamble    = preamble is null ? null : new UpdateValueOfstring { Value    = preamble },
                    PdfTemplate = pdfTemplate is null ? null : new UpdateValueOfstring { Value = pdfTemplate },
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
                    Title    = new UpdateValueOfstring { Value = title },
                    Preamble = new UpdateValueOfstring { Value = preamble },
                    PdfTemplate = new UpdateValueOfstring
                    {
                        Value = """
                                <template>
                                    <body>
                                        <text>@title(organization(context))</text>
                                        <text weight="bold" fontsize="24">@title(context)</text>
                                        <line thickness="5pt" color="black"/>
                                        <text margin="0 5pt">@preamble(context)</text>
                                        <line thickness="5pt" color="black"/>
                                        <table>
                                            @foreach topic in topics(context) {
                                                <tr>
                                                    <td>
                                                        <text weight="bold" margin="0 15pt 0 0">@title(topic)</text>
                                                        <line thickness="1pt" color="gray" margin="0 0 0 5pt"/>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                    @foreach question in take-random-questions(topic) {
                                                        <text weight="bold" margin="0 10pt 0 0">@title(question)</text>
                                                        @foreach answer in take-random-answers(question) {
                                                            <table>
                                                                    <tr>
                                                                        <td width="0.5cm">
                                                                            @if include-answers {
                                                                                @if is-correct(answer) {
                                                                                <border padding="1mm"
                                                                                        margin="0 1mm 0 0"
                                                                                        thickness="0.3mm"
                                                                                        color="black"
                                                                                        background="black"
                                                                                        verticalalignment="top"
                                                                                        horizontalalignment="center"/>
                                                                                }
                                                                                @if is-correct(answer) == false {
                                                                                <border padding="1mm"
                                                                                        margin="0 1mm 0 0"
                                                                                        thickness="0.3mm"
                                                                                        color="black"
                                                                                        verticalalignment="top"
                                                                                        horizontalalignment="center"/>
                                                                                }
                                                                            }
                                                                            @if include-answers == false {
                                                                                <border padding="1mm"
                                                                                        margin="0 1mm 0 0"
                                                                                        thickness="0.3mm"
                                                                                        color="black"
                                                                                        verticalalignment="top"
                                                                                        horizontalalignment="center"/>
                                                                            }
                                                                        </td>
                                                                        <td><text>@answer-text(answer)</text></td>
                                                                    </tr>
                                                                    @if include-answers {
                                                                        <tr>
                                                                            <td/>
                                                                            <td><text>@reason-text(answer)</text></td>
                                                                        </tr>
                                                                    }
                                                            </table>
                                                        }
                                                    }
                                                    </td>
                                                </tr>
                                            }
                                        </table>
                                    </body>
                                </template>
                                """,
                    },
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

    public async Task<PdfPreviewDto> GetPdfPreviewAsync(
        Guid examIdentifier,
        string? xmlTemplate,
        CancellationToken cancellationToken = default
    )
    {
        var result = await Client.Exam[examIdentifier]
            .Image
            .All
            .PostAsync(xmlTemplate ?? string.Empty, cancellationToken: cancellationToken);
        if (result is null)
            throw new Exception("Server responded with null");
        return result;
    }
}
