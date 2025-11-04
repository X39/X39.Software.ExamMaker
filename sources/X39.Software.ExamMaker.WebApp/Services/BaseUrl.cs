using System.Diagnostics.CodeAnalysis;

namespace X39.Software.ExamMaker.WebApp.Services;

public record BaseUrl(string ApiUrl, string SelfUrl)
{
    public Uri ApiUri => new(ApiUrl);
    public Uri SelfUri => new(SelfUrl);

    public string ResolveApiUrl([StringSyntax(StringSyntaxAttribute.Uri)] string path)
        => ResolveApiUri(path)
            .ToString();

    public Uri ResolveApiUri([StringSyntax(StringSyntaxAttribute.Uri)] string path)
    {
        path = path.TrimStart('/');
        return new Uri(ApiUri, path);
    }

    public string ResolveSelfUrl([StringSyntax(StringSyntaxAttribute.Uri)] string path)
        => ResolveSelfUri(path)
            .ToString();

    public Uri ResolveSelfUri([StringSyntax(StringSyntaxAttribute.Uri)] string path)
    {
        path = path.TrimStart('/');
        return new Uri(SelfUri, path);
    }

    public string ResolveResourceUrl([StringSyntax(StringSyntaxAttribute.Uri)] string path) => ResolveSelfUrl(path);
}
