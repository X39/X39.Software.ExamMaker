using System.ComponentModel;
using NodaTime;
using X39.Software.ExamMaker.Api.Storage.Meta;

namespace X39.Software.ExamMaker.Api;

public static class UpdateTimeStampHelper
{
    public static UpdateTimeStampHelper<T> Create<T>(T t) where T : IUpdatedAt, INotifyPropertyChanged => new(t);
}
public sealed class UpdateTimeStampHelper<T> : IDisposable
    where T : IUpdatedAt, INotifyPropertyChanged
{
    private readonly T _t;

    public UpdateTimeStampHelper(T t)
    {
        _t                =  t;
        t.PropertyChanged += TOnPropertyChanged;
    }

    private void TOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is T t && e.PropertyName != nameof(t.UpdatedAt))
            t.UpdatedAt = SystemClock.Instance.GetCurrentInstant();
    }

    public void Dispose()
    {
        _t.PropertyChanged -= TOnPropertyChanged;
    }
}
