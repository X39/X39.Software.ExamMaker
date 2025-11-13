using System.Collections.ObjectModel;
using X39.Software.ExamMaker.Models;
using X39.Software.ExamMaker.WebApp.Services.ExamAnswerRepository;
using X39.Software.ExamMaker.WebApp.Services.ExamQuestionRepository;
using X39.Software.ExamMaker.WebApp.Services.ExamRepository;
using X39.Software.ExamMaker.WebApp.Services.ExamTopicRepository;

namespace X39.Software.ExamMaker.WebApp.Pages.Management.Exams.Arg;

public sealed class ExamViewModel(
    ExamListingDto exam,
    Func<Task> stateHasChanged,
    IExamRepository examRepository,
    IExamTopicRepository examTopicRepository,
    IExamQuestionRepository examQuestionRepository,
    IExamAnswerRepository examAnswerRepository
)
{
    public ObservableCollection<TopicViewModel> Topics { get; } = new();
    public Guid Identifier => exam.Identifier!.Value;

    public BusyHelper BusyHelper { get; } = new(stateHasChanged);

    public string Title
    {
        get => exam.Title ?? string.Empty;
        set
        {
            Task.Run(async () =>
                {
                    using var busy = BusyHelper.Busy();
                    await examRepository.UpdateAsync(Identifier, value, null);
                    exam.Title = value;
                    await stateHasChanged();
                }
            );
        }
    }

    public string Preamble
    {
        get => exam.Preamble ?? string.Empty;
        set
        {
            Task.Run(async () =>
                {
                    using var busy = BusyHelper.Busy();
                    await examRepository.UpdateAsync(Identifier, null, value);
                    exam.Preamble = value;
                    await stateHasChanged();
                }
            );
        }
    }

    public async Task InitializeAsync()
    {
        var topicCount = await examTopicRepository.GetAllExamTopicsCountAsync(exam.Identifier!.Value);
        const int takeAmount = 50;
        for (var topicIndex = 0; topicIndex < topicCount; topicIndex += takeAmount)
        {
            var topics = await examTopicRepository.GetAllExamTopicsAsync(Identifier, topicIndex, takeAmount);
            foreach (var topic in topics)
            {
                var viewModel = new TopicViewModel(
                    Identifier,
                    topic,
                    stateHasChanged,
                    examTopicRepository,
                    examQuestionRepository,
                    examAnswerRepository
                );
                await viewModel.InitializeAsync();
                Topics.Add(viewModel);
            }
        }
    }

    public async Task AddTopicAsync()
    {
        using var busy = BusyHelper.Busy();
        var newTopic = await examTopicRepository.CreateAsync(Identifier, string.Empty, null);
        var viewModel = new TopicViewModel(
            Identifier,
            newTopic,
            stateHasChanged,
            examTopicRepository,
            examQuestionRepository,
            examAnswerRepository
        );
        await viewModel.InitializeAsync();
        Topics.Add(viewModel);
        await stateHasChanged();
    }

    public async Task DeleteTopicAsync(TopicViewModel topic)
    {
        using var busy = BusyHelper.Busy();
        await examTopicRepository.DeleteAsync(Identifier, topic.Identifier);
        Topics.Remove(topic);
        await stateHasChanged();
    }
}
