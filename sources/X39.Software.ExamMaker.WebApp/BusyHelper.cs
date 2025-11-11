namespace X39.Software.ExamMaker.WebApp;

public sealed class BusyHelper(Func<Task> stateHasChanged)
{
    public bool IsBusy => _busyCount > 0;
    private int _busyCount;

    public IDisposable Busy()
        => new Disposable(
            () =>
            {
                Interlocked.Increment(ref _busyCount);
                stateHasChanged();
            },
            () =>
            {
                Interlocked.Decrement(ref _busyCount);
                stateHasChanged();
            }
        );
}
