using System.Collections.ObjectModel;
using X39.Software.ExamMaker.Models;
using X39.Software.ExamMaker.WebApp.Services.ExamAnswerRepository;
using X39.Software.ExamMaker.WebApp.Services.ExamQuestionRepository;
using X39.Software.ExamMaker.WebApp.Services.ExamTopicRepository;

namespace X39.Software.ExamMaker.WebApp.Pages.Management.Exams.Arg;

public sealed class TopicViewModel(
    Guid examIdentifier,
    ExamTopicListingDto topic,
    Func<Task> stateHasChanged,
    IExamTopicRepository examTopicRepository,
    IExamQuestionRepository examQuestionRepository,
    IExamAnswerRepository examAnswerRepository
)
{
    public ObservableCollection<QuestionViewModel> Questions { get; } = new();
    public Guid Identifier => topic.Identifier!.Value;

    public BusyHelper BusyHelper { get; } = new(stateHasChanged);

    public string Title
    {
        get => topic.Title ?? string.Empty;
        set
        {
            Task.Run(async () =>
                {
                    using var busy = BusyHelper.Busy();
                    await examTopicRepository.UpdateAsync(examIdentifier, Identifier, value, null);
                    topic.Title = value;
                    await stateHasChanged();
                }
            );
        }
    }

    public int? QuestionAmountToTake
    {
        get => topic.QuestionAmountToTake;
        set
        {
            Task.Run(async () =>
                {
                    using var busy = BusyHelper.Busy();
                    await examTopicRepository.UpdateAsync(examIdentifier, Identifier, null, value);
                    topic.QuestionAmountToTake = value;
                    await stateHasChanged();
                }
            );
        }
    }

    public async Task InitializeAsync()
    {
        var questionCount = await examQuestionRepository.GetAllCountAsync(examIdentifier, Identifier);
        const int takeAmount = 50;
        for (var topicIndex = 0; topicIndex < questionCount; topicIndex += takeAmount)
        {
            var questions = await examQuestionRepository.GetAllAsync(
                examIdentifier,
                Identifier,
                topicIndex,
                takeAmount
            );
            foreach (var question in questions)
            {
                var viewModel = new QuestionViewModel(
                    examIdentifier,
                    Identifier,
                    question,
                    stateHasChanged,
                    examQuestionRepository,
                    examAnswerRepository
                );
                await viewModel.InitializeAsync();
                Questions.Add(viewModel);
            }
        }
    }

    public async Task AddQuestionAsync()
    {
        using var busy = BusyHelper.Busy();
        var newQuestion = await examQuestionRepository.CreateAsync(
            examIdentifier,
            Identifier,
            string.Empty,
            null,
            null,
            EQuestionKind.MultipleChoice
        );
        var viewModel = new QuestionViewModel(
            examIdentifier,
            Identifier,
            newQuestion,
            stateHasChanged,
            examQuestionRepository,
            examAnswerRepository
        );
        await viewModel.InitializeAsync();
        Questions.Add(viewModel);
        await stateHasChanged();
    }

    public async Task DeleteQuestionAsync(QuestionViewModel question)
    {
        using var busy = BusyHelper.Busy();
        await examQuestionRepository.DeleteAsync(examIdentifier, Identifier, question.Identifier);
        Questions.Remove(question);
        await stateHasChanged();
    }
}
