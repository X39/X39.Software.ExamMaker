using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;

namespace X39.Software.ExamMaker.WebApp;

public static class NavigationManagerExtensions
{
    public static bool TryGetQueryParameter(
        this NavigationManager self,
        string queryParameter,
        [NotNullWhen(true)] out string? value,
        bool caseSensitive = false
    )
    {
        var queryParameters = System.Web.HttpUtility.ParseQueryString(self.Uri);
        var key = caseSensitive
            ? queryParameters.AllKeys.FirstOrDefault(e
                => e?.Equals(queryParameter, StringComparison.OrdinalIgnoreCase) ?? false
            )
            : queryParameter;
        value = queryParameters[key];
        return value != null;
    }
}
