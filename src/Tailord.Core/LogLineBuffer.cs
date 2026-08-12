using System.Text;

namespace Tailord.Core;

public sealed class LogLineBuffer
{
    private readonly StringBuilder _partialLine = new();
    private bool _pendingCarriageReturn;

    public string PartialLine => _partialLine.ToString();

    public IReadOnlyList<string> Append(ReadOnlySpan<char> text)
    {
        List<string> completedLines = [];

        foreach (char character in text)
        {
            if (_pendingCarriageReturn)
            {
                AddCompletedLine(completedLines);
                _pendingCarriageReturn = false;

                if (character == '\n')
                {
                    continue;
                }
            }

            switch (character)
            {
                case '\r':
                    _pendingCarriageReturn = true;
                    break;
                case '\n':
                    AddCompletedLine(completedLines);
                    break;
                default:
                    _partialLine.Append(character);
                    break;
            }
        }

        return completedLines;
    }

    private void AddCompletedLine(List<string> completedLines)
    {
        completedLines.Add(_partialLine.ToString());
        _partialLine.Clear();
    }
}
