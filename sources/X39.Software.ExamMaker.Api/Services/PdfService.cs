using System.Collections;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Xml;
using Polly.Simmy.Fault;
using SkiaSharp;
using X39.Software.ExamMaker.Api.Storage.Exam.Entities;
using X39.Software.ExamMaker.Api.Storage.Meta;
using X39.Solutions.PdfTemplate;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Data;
using X39.Solutions.PdfTemplate.Services;
using X39.Util;

namespace X39.Software.ExamMaker.Api.Services;

public sealed class PdfService(SkPaintCache paintCache, ControlExpressionCache controlExpressionCache)
{
    public async Task<byte[]> CreateExamPdfAsync(
        Exam exam,
        string pdfTemplate,
        bool includeAnswers,
        CultureInfo cultureInfo,
        int? seed = null,
        CancellationToken cancellationToken = default
    )
    {
        await using var generator = new Generator(paintCache, controlExpressionCache, Enumerable.Empty<IFunction>());
        var random = seed is null ? Random.Shared : new Random(seed.Value);
        generator.AddDefaults();
        generator.TemplateData.RegisterFunction(InlineFunction.Create("organization", (Exam e) => e.Organization));
        generator.TemplateData.RegisterFunction(InlineFunction.Create("title", (ITitle e) => e.Title));
        generator.TemplateData.RegisterFunction(InlineFunction.Create("preamble", (Exam e) => e.Preamble));
        generator.TemplateData.RegisterFunction(InlineFunction.Create("topics", (Exam e) => e.ExamTopics));
        generator.TemplateData.RegisterFunction(InlineFunction.Create("is-correct", (ExamAnswer e) => e.IsCorrect));
        generator.TemplateData.RegisterFunction(InlineFunction.Create("answer-text", (ExamAnswer e) => e.Answer));
        generator.TemplateData.RegisterFunction(InlineFunction.Create("reason-text", (ExamAnswer e) => e.Reason));
        generator.TemplateData.RegisterFunction(new TakeRandomQuestionsFunction(random));
        generator.TemplateData.RegisterFunction(new TakeRandomAnswersFunction(random));
        generator.TemplateData.SetVariable("context", exam);
        generator.TemplateData.SetVariable("include-answers", includeAnswers);
        using var stringReader = new StringReader(pdfTemplate);
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

    public async Task<ImmutableList<byte[]>> CreateExamImagesAsync(
        Exam exam,
        string pdfTemplate,
        bool includeAnswers,
        CultureInfo cultureInfo,
        int? seed = null,
        CancellationToken cancellationToken = default
    )
    {
        await using var generator = new Generator(paintCache, controlExpressionCache, Enumerable.Empty<IFunction>());
        var random = seed is null ? Random.Shared : new Random(seed.Value);
        generator.AddDefaults();
        generator.TemplateData.RegisterFunction(InlineFunction.Create("organization", (Exam e) => e.Organization));
        generator.TemplateData.RegisterFunction(InlineFunction.Create("title", (ITitle e) => e.Title));
        generator.TemplateData.RegisterFunction(InlineFunction.Create("preamble", (Exam e) => e.Preamble));
        generator.TemplateData.RegisterFunction(InlineFunction.Create("topics", (Exam e) => e.ExamTopics));
        generator.TemplateData.RegisterFunction(InlineFunction.Create("is-correct", (ExamAnswer e) => e.IsCorrect));
        generator.TemplateData.RegisterFunction(InlineFunction.Create("answer-text", (ExamAnswer e) => e.Answer));
        generator.TemplateData.RegisterFunction(InlineFunction.Create("reason-text", (ExamAnswer e) => e.Reason));
        generator.TemplateData.RegisterFunction(new TakeRandomQuestionsFunction(random));
        generator.TemplateData.RegisterFunction(new TakeRandomAnswersFunction(random));
        generator.TemplateData.SetVariable("context", exam);
        generator.TemplateData.SetVariable("include-answers", includeAnswers);
        using var stringReader = new StringReader(pdfTemplate);
        using var reader = XmlReader.Create(stringReader);
        var skBitmaps = await generator.GenerateBitmapsAsync(
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
        try
        {
            return skBitmaps.Select(skBitmap =>
                    {
                        using var data = skBitmap.Encode(SKEncodedImageFormat.Webp, 90);
                        return data?.ToArray() ?? Array.Empty<byte>();
                    }
                )
                .ToImmutableList();
        }
        finally
        {
            foreach (var skBitmap in skBitmaps)
                Fault.Ignore(() => skBitmap.Dispose());
        }
    }

    private sealed class TakeRandomAnswersFunction(Random random) : IFunction
    {
        public ValueTask<object?> ExecuteAsync(
            CultureInfo cultureInfo,
            object?[] arguments,
            CancellationToken cancellationToken = new CancellationToken()
        )
        {
            if (arguments is not [ExamQuestion examQuestion])
                throw new ArgumentException("Invalid arguments");
            if (examQuestion.ExamAnswers is null)
                throw new InvalidOperationException("Exam answers are null");
            var correctAnswers = examQuestion.ExamAnswers
                .Where(x => x.IsCorrect)
                .Take(examQuestion.CorrectAnswersToTake ?? examQuestion.ExamAnswers.Count)
                .ToImmutableList();
            var incorrectAnswers = examQuestion.ExamAnswers
                .Where(x => !x.IsCorrect)
                .Take(examQuestion.IncorrectAnswersToTake ?? examQuestion.ExamAnswers.Count)
                .ToImmutableList();
            var randomized = correctAnswers.Concat(incorrectAnswers)
                .OrderBy(_ => random.Next())
                .ToImmutableList();
            return ValueTask.FromResult<object?>(randomized);
        }

        public string Name => "take-random-answers";
        public int Arguments => 1;
        public bool IsVariadic => false;
    }

    private sealed class TakeRandomQuestionsFunction(Random random) : IFunction
    {
        public ValueTask<object?> ExecuteAsync(
            CultureInfo cultureInfo,
            object?[] arguments,
            CancellationToken cancellationToken = new CancellationToken()
        )
        {
            if (arguments is not [ExamTopic topic])
                throw new ArgumentException("Invalid arguments");
            if (topic.ExamQuestions is null)
                throw new InvalidOperationException("Exam questions are null");

            if (topic.QuestionAmountToTake is null)
                return ValueTask.FromResult<object?>(topic.ExamQuestions);

            var randomElements = topic.ExamQuestions!.OrderBy(_ => random.Next())
                .Take(topic.QuestionAmountToTake.Value)
                .ToImmutableList();
            return ValueTask.FromResult<object?>(randomElements);
        }

        public string Name => "take-random-questions";
        public int Arguments => 1;
        public bool IsVariadic => false;
    }

    public static class InlineFunction
    {
        public static IFunction Create<T>(string name, Func<T, object?> func) => new InlineFunction<T>(name, func);

        public static IFunction Create<T1, T2>(string name, Func<T1, T2, object?> func)
            => new InlineFunction<T1, T2>(name, func);
    }

    public sealed class InlineFunction<T>(string name, Func<T, object?> func) : IFunction
    {
        public ValueTask<object?> ExecuteAsync(
            CultureInfo cultureInfo,
            object?[] arguments,
            CancellationToken cancellationToken = new CancellationToken()
        )
        {
            if (arguments is not [T el])
                throw new ArgumentException("Invalid arguments");
            return ValueTask.FromResult(func(el));
        }

        public string Name => name;
        public int Arguments => 1;
        public bool IsVariadic => false;
    }

    public sealed class InlineFunction<T1, T2>(string name, Func<T1, T2, object?> func) : IFunction
    {
        public ValueTask<object?> ExecuteAsync(
            CultureInfo cultureInfo,
            object?[] arguments,
            CancellationToken cancellationToken = new CancellationToken()
        )
        {
            if (arguments is not [T1 arg1, T2 arg2])
                throw new ArgumentException("Invalid arguments");
            return ValueTask.FromResult(func(arg1, arg2));
        }

        public string Name => name;
        public int Arguments => 2;
        public bool IsVariadic => false;
    }
}
