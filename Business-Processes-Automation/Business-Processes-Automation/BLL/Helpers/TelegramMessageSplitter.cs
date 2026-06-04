using System.Text;

namespace Business_Processes_Automation.BLL.Helpers;

public static class TelegramMessageSplitter
{
    public const int DefaultMaxLength = 4000;

    public static IReadOnlyList<string> Split(string text, int maxLength = DefaultMaxLength)
    {
        if (string.IsNullOrEmpty(text))
        {
            return [string.Empty];
        }

        if (text.Length <= maxLength)
        {
            return [text];
        }

        var parts = new List<string>();
        var lines = text.Split('\n');
        var current = new StringBuilder();

        foreach (var line in lines)
        {
            var lineWithBreak = current.Length == 0 ? line : "\n" + line;

            if (current.Length + lineWithBreak.Length > maxLength)
            {
                if (current.Length > 0)
                {
                    parts.Add(current.ToString().TrimEnd());
                    current.Clear();
                }

                if (line.Length > maxLength)
                {
                    for (var offset = 0; offset < line.Length; offset += maxLength)
                    {
                        var length = Math.Min(maxLength, line.Length - offset);
                        parts.Add(line.Substring(offset, length));
                    }

                    continue;
                }

                current.Append(line);
                continue;
            }

            if (current.Length == 0)
            {
                current.Append(line);
            }
            else
            {
                current.Append('\n');
                current.Append(line);
            }
        }

        if (current.Length > 0)
        {
            parts.Add(current.ToString().TrimEnd());
        }

        return parts;
    }
}
