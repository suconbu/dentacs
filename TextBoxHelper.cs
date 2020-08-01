using System;
using System.Collections.Generic;
using System.Linq;

namespace Suconbu.Dentacs
{
    public static class TextBoxHelper
    {
        // Reason for prepared these methods:
        // TextBox::GetCharacterIndexFromLineIndex does not work correctly in full-sreen mode

        public static int GetCharacterIndexOfLineStartFromLineIndex(IEnumerable<string> lines, int lineIndex)
        {
            return lines.Take(lineIndex).Select(line => line.Length + Environment.NewLine.Length).Sum();
        }

        public static int GetCharacterIndexOfLineEndFromLineIndex(IEnumerable<string> lines, int lineIndex)
        {
            return TextBoxHelper.GetCharacterIndexOfLineStartFromLineIndex(lines, lineIndex) + lines.ElementAt(lineIndex).Length + Environment.NewLine.Length;
        }

        public static void GetStartEndLineIndex(IEnumerable<string> lines, int charIndexStart, int charIndexEnd, out int lineIndexStart, out int lineIndexEnd)
        {
            lineIndexStart = -1;
            lineIndexEnd = -1;
            int lineIndex = 0;
            int count = 0;

            int start = charIndexStart;
            int end = charIndexEnd;
            if (end < start)
            {
                start = charIndexEnd;
                end = charIndexStart;
            }

            foreach (var line in lines)
            {
                count += line.Length + Environment.NewLine.Length;
                if (lineIndexStart == -1 && start < count)
                {
                    lineIndexStart = lineIndex;
                }
                if (lineIndexStart != -1 && end <= count)
                {
                    lineIndexEnd = lineIndex;
                    break;
                }
                lineIndex++;
            }
            return;
        }
    }
}
