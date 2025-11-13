using System.Collections.ObjectModel;
using X39.Software.ExamMaker.Models;
using X39.Software.ExamMaker.WebApp.Services.ExamAnswerRepository;
using X39.Software.ExamMaker.WebApp.Services.ExamQuestionRepository;

namespace X39.Software.ExamMaker.WebApp.Pages.Management.Exams.Arg;

public sealed class QuestionViewModel(
    Guid examIdentifier,
    Guid topicIdentifier,
    ExamQuestionListingDto question,
    Func<Task> stateHasChanged,
    IExamQuestionRepository examQuestionRepository,
    IExamAnswerRepository examAnswerRepository
)
{
    public ObservableCollection<AnswerViewModel> Answers { get; } = new();
    public Guid Identifier => question.Identifier!.Value;

    public BusyHelper BusyHelper { get; } = new(stateHasChanged);

    public string Title
    {
        get => question.Title ?? string.Empty;
        set
        {
            Task.Run(async () =>
                {
                    using var busy = BusyHelper.Busy();
                    await examQuestionRepository.UpdateAsync(
                        examIdentifier,
                        topicIdentifier,
                        Identifier,
                        value,
                        null,
                        null,
                        null
                    );
                    question.Title = value;
                    await stateHasChanged();
                }
            );
        }
    }

    public int? CorrectAnswersToTake
    {
        get => question.CorrectAnswersToTake;
        set
        {
            Task.Run(async () =>
                {
                    using var busy = BusyHelper.Busy();
                    await examQuestionRepository.UpdateAsync(
                        examIdentifier,
                        topicIdentifier,
                        Identifier,
                        null,
                        value,
                        null,
                        null
                    );
                    question.CorrectAnswersToTake = value;
                    await stateHasChanged();
                }
            );
        }
    }

    public int? IncorrectAnswersToTake
    {
        get => question.IncorrectAnswersToTake;
        set
        {
            Task.Run(async () =>
                {
                    using var busy = BusyHelper.Busy();
                    await examQuestionRepository.UpdateAsync(
                        examIdentifier,
                        topicIdentifier,
                        Identifier,
                        null,
                        null,
                        value,
                        null
                    );
                    question.IncorrectAnswersToTake = value;
                    await stateHasChanged();
                }
            );
        }
    }

    public EQuestionKindEnum Kind
    {
        get => (EQuestionKindEnum) (question.Kind?.Integer ?? default);
        set
        {
            Task.Run(async () =>
                {
                    using var busy = BusyHelper.Busy();
                    await examQuestionRepository.UpdateAsync(
                        examIdentifier,
                        topicIdentifier,
                        Identifier,
                        null,
                        null,
                        null,
                        value
                    );
                    question.Kind = new EQuestionKind { Integer = (int) value };
                    await stateHasChanged();
                }
            );
        }
    }

    public async Task InitializeAsync()
    {
        var answerCount = await examAnswerRepository.GetAllCountAsync(examIdentifier, topicIdentifier, Identifier);
        const int takeAmount = 50;
        for (var topicIndex = 0; topicIndex < answerCount; topicIndex += takeAmount)
        {
            var answers = await examAnswerRepository.GetAllAsync(
                examIdentifier,
                topicIdentifier,
                Identifier,
                topicIndex,
                takeAmount
            );
            foreach (var answer in answers)
            {
                var viewModel = new AnswerViewModel(
                    examIdentifier,
                    Identifier,
                    Identifier,
                    answer,
                    stateHasChanged,
                    examAnswerRepository
                );
                await viewModel.InitializeAsync();
                Answers.Add(viewModel);
            }
        }
    }

    public async Task AddAnswerAsync()
    {
        using var busy = BusyHelper.Busy();
        var newAnswer = await examAnswerRepository.CreateAsync(examIdentifier, topicIdentifier, Identifier, string.Empty, null, false);
        var viewModel = new AnswerViewModel(
            examIdentifier,
            topicIdentifier,
            Identifier,
            newAnswer,
            stateHasChanged,
            examAnswerRepository
        );
        await viewModel.InitializeAsync();
        Answers.Add(viewModel);
        await stateHasChanged();
    }

    public async Task DeleteAnswerAsync(AnswerViewModel answer)
    {
        using var busy = BusyHelper.Busy();
        await examAnswerRepository.DeleteAsync(examIdentifier, topicIdentifier, Identifier, answer.Identifier);
        Answers.Remove(answer);
        await stateHasChanged();
    }
}
