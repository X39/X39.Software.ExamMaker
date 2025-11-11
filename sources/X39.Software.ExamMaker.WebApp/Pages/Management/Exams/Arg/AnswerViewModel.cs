using X39.Software.ExamMaker.Models;
using X39.Software.ExamMaker.WebApp.Services.ExamAnswerRepository;

namespace X39.Software.ExamMaker.WebApp.Pages.Management.Exams.Arg;

public sealed class AnswerViewModel(Guid examIdentifier, Guid topicIdentifier, Guid questionIdentifier, ExamAnswerListingDto answer, Func<Task> stateHasChanged, IExamAnswerRepository examAnswerRepository)
{
    public Guid Identifier => answer.Identifier!.Value;

    public BusyHelper BusyHelper { get; } = new(stateHasChanged);

    public string Answer
    {
        get => answer.Answer ?? string.Empty;
        set
        {
            Task.Run(async () =>
                {
                    using var busy = BusyHelper.Busy();
                    await examAnswerRepository.UpdateAsync(examIdentifier, topicIdentifier, questionIdentifier, Identifier, value, null, null);
                    answer.Answer = value;
                    await stateHasChanged();
                }
            );
        }
    }

    public string Reason
    {
        get => answer.Reason ?? string.Empty;
        set
        {
            Task.Run(async () =>
                {
                    using var busy = BusyHelper.Busy();
                    await examAnswerRepository.UpdateAsync(examIdentifier, topicIdentifier, questionIdentifier, Identifier, null, value, null);
                    answer.Reason = value;
                    await stateHasChanged();
                }
            );
        }
    }

    public bool IsCorrect
    {
        get => answer.IsCorrect ?? false;
        set
        {
            Task.Run(async () =>
                {
                    using var busy = BusyHelper.Busy();
                    await examAnswerRepository.UpdateAsync(examIdentifier, topicIdentifier, questionIdentifier, Identifier, null, null, value);
                    answer.IsCorrect = value;
                    await stateHasChanged();
                }
            );
        }
    }

    public Task InitializeAsync()
    {
        // Empty
        return Task.CompletedTask;
    }
}
