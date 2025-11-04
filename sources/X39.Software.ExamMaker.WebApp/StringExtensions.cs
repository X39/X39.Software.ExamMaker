namespace X39.Software.ExamMaker.WebApp;

public static class StringExtensions
{
    public static string GetInitials(this string? self)
    {
        if (self is null)
            return string.Empty;
        var splatted = self.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var chars = splatted.Select(e => e.First())
            .ToArray();
        return new string(chars);
    }
}
