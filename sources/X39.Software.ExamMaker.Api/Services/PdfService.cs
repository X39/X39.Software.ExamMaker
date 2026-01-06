using System.Collections;
using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using System.Xml;
using X39.Software.ExamMaker.Api.Storage.Exam.Entities;
using X39.Solutions.PdfTemplate;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Data;
using X39.Solutions.PdfTemplate.Services;

namespace X39.Software.ExamMaker.Api.Services;

public sealed class PdfService(SkPaintCache paintCache, ControlExpressionCache controlExpressionCache)
{
    private const string PdfTemplate = """
                                       <template>
                                        <body>
                                            <text>@context.Organization.Title</text>
                                            <text fontweight="bold" fontsize="16">@context.Title</text>
                                            <text>@context.Preamble</text>
                                            <line thickness="1pt" color="black" margin="0 0 10pt 0"/>
                                            @foreach topic in @context.Topics {
                                                <text fontweight="bold">@topic.Title</text>
                                                <line thickness="1pt" color="gray"/>
                                                @foreach question in take-random-questions(@topic.Questions, @topic.QuestionAmountToTake) {
                                                    <text fontweight="bold">@topic.Title</text>
                                                    <table>
                                                    @foreach answer in take-random-answers(question.Answers, @question.CorrectAmountToTake, @question.IncorrectAmountToTake) {
                                                        <tr>
                                                            <td>
                                                                @if @include-answers {
                                                                    @if @answer.IsCorrect {
                                                                        <border padding="0.5cm" thickness="1mm" color="black" background="black" verticalalignment="center" horizontalalignment="center"/>
                                                                    }
                                                                    @if @answer.IsCorrect == false {
                                                                        <border padding="0.5cm" thickness="1mm" color="black" verticalalignment="center" horizontalalignment="center"/>
                                                                    }
                                                                }
                                                                @if @include-answers == false {
                                                                    <border padding="0.5cm" thickness="1mm" color="black" verticalalignment="center" horizontalalignment="center"/>
                                                                }
                                                            </td>
                                                            <td>@answer.Text</td>
                                                        </tr>
                                                        @if @include-answers {
                                                            <tr>
                                                                <td/>
                                                                <td>@answer.Reason</td>
                                                            </tr>
                                                        }
                                                    }
                                                    </table>
                                                }
                                            }
                                        </body>
                                       </template>
                                       """;

    public async Task<byte[]> CreateExamPdfAsync(
        Exam exam,
        bool includeAnswers,
        CultureInfo cultureInfo,
        CancellationToken cancellationToken = default
    )
    {
        await using var generator = new Generator(paintCache, controlExpressionCache, Enumerable.Empty<IFunction>());
        generator.AddDefaults();
        generator.AddData("context", exam);
        generator.AddData("include-answers", includeAnswers);
        generator.TemplateData.RegisterFunction(new TakeRandomQuestionsFunction());
        generator.TemplateData.SetVariable("context", exam);
        generator.TemplateData.SetVariable("include-answers", includeAnswers);
        using var stringReader = new StringReader(PdfTemplate);
        using var reader = XmlReader.Create(stringReader);
        using var pdfStream = new MemoryStream();
        await generator.GeneratePdfAsync(
            pdfStream,
            reader,
            cultureInfo,
            new DocumentOptions
            {
                Margin                  = new Thickness(new Length(1, ELengthUnit.Centimeters)),
                PageHeightInMillimeters = 297,
                PageWidthInMillimeters  = 210,
                DotsPerInch             = 300,
                Producer                = "X39 Software Exam Maker",
                Modified                = DateTime.UtcNow,
            },
            cancellationToken
        );
        return pdfStream.ToArray();
    }

    private sealed class TakeRandomAnswersFunction : IFunction
    {
        public ValueTask<object?> ExecuteAsync(
            CultureInfo cultureInfo,
            object?[] arguments,
            CancellationToken cancellationToken = new CancellationToken()
        )
        {
            if (arguments is not [ICollection<ExamAnswer> elements, _, _])
                throw new ArgumentException("Invalid arguments");
            var correctAnswersToTake = (int?) arguments[1] ?? elements.Count;
            var incorrectAnswersToTake = (int?) arguments[2] ?? elements.Count;
            var correctAnswers = elements.Where(x => x.IsCorrect)
                .Take(correctAnswersToTake)
                .ToImmutableList();
            var incorrectAnswers = elements.Where(x => !x.IsCorrect)
                .Take(incorrectAnswersToTake)
                .ToImmutableList();
            var randomized = correctAnswers.Concat(incorrectAnswers)
                .OrderBy(x => Random.Shared.Next())
                .ToImmutableList();
            return ValueTask.FromResult<object?>(randomized);
        }

        public string Name => "take-random-answers";
        public int Arguments => 3;
        public bool IsVariadic => false;
    }

    private sealed class TakeRandomQuestionsFunction : IFunction
    {
        public ValueTask<object?> ExecuteAsync(
            CultureInfo cultureInfo,
            object?[] arguments,
            CancellationToken cancellationToken = new CancellationToken()
        )
        {
            if (arguments is not [ICollection<ExamAnswer> elements, _])
                throw new ArgumentException("Invalid arguments");
            if (arguments[1] is not int and not null)
                throw new ArgumentException("Invalid arguments");
            if (arguments[1] is null)
            {
                return ValueTask.FromResult<object?>(elements);
            }
            else
            {
                var count = (int) arguments[1]!;
                var randomElements = elements.OrderBy(x => Random.Shared.Next())
                    .Take(count)
                    .ToImmutableList();
                return ValueTask.FromResult<object?>(randomElements);
            }
        }

        public string Name => "take-random-questions";
        public int Arguments => 2;
        public bool IsVariadic => false;
    }
}
