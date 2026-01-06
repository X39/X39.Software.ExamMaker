using Microsoft.JSInterop;

namespace X39.Software.ExamMaker.WebApp.Services;

/// <summary>
/// Provides access to the local storage of the browser.
/// </summary>
public sealed class JsUtil : IAsyncDisposable
{
    private          Lazy<IJSObjectReference> _accessor = new();
    private readonly IJSRuntime               _jsRuntime;

    /// <summary>
    /// Creates a new instance of the <see cref="LocalStorage"/> class.
    /// </summary>
    /// <param name="jsRuntime">The <see cref="IJSRuntime"/> to use for accessing the local storage.</param>
    public JsUtil(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Ensures that the <see cref="_accessor"/> is created.
    /// </summary>
    private async Task EnsureAccessor()
    {
        if (_accessor.IsValueCreated)
            return;
        var res = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/exports/util.js");
        _accessor = new Lazy<IJSObjectReference>(res);
    }


    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_accessor.IsValueCreated)
            await _accessor.Value.DisposeAsync()
                .ConfigureAwait(false);
    }
    public async ValueTask ApiDownload(string url, string fileName, string jwtToken)
    {
        await EnsureAccessor().ConfigureAwait(false);
        await _accessor.Value.InvokeVoidAsync("ApiDownload", url, fileName, jwtToken).ConfigureAwait(false);
    }

}
