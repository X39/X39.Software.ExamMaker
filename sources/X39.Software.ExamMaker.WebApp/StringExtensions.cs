using System.Text;

namespace X39.Software.ExamMaker.WebApp;

public static class StringExtensions
{
    extension(string? self)
    {
        public string GetInitials()
        {
            if (self is null)
                return string.Empty;
            var splatted = self.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var chars = splatted.Select(e => e.First())
                .ToArray();
            return new string(chars);
        }

        public string ToIdentifierString()
        {
            if (self is null)
                return string.Empty;
            var builder = new StringBuilder(self.Length);
            for (var index = 0; index < self.Length; index++)
            {
                var c = self[index];
                c = c.ToLowerInvariant();
                if (builder.Length > 1 && builder[^1] is '-')
                    continue; // Do not have repeated '-'
                builder.Append(char.IsLetterOrDigit(c) ? c : '-');
            }

            return builder.ToString();
        }
    }
}
