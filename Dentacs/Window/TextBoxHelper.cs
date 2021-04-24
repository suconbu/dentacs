using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Suconbu.Dentacs
{
    public static class TextBoxHelper
    {
        // Reason for prepared these methods:
        // TextBox::GetCharacterIndexFromLineIndex does not work correctly in full-sreen mode

        public static int GetCharacterIndexOfLineStartFromLineIndex(LineString[] lines, int lineIndex)
        {
            return lines.Take(lineIndex).Select(line => line.FullLength).Sum();
        }

        public static int GetCharacterIndexOfLineEndFromLineIndex(LineString[] lines, int lineIndex)
        {
            return TextBoxHelper.GetCharacterIndexOfLineStartFromLineIndex(lines, lineIndex) + lines.ElementAt(lineIndex).FullLength;
        }

        public static void GetStartEndLineIndex(LineString[] lines, int charIndexStart, int charIndexEnd, out int lineIndexStart, out int lineIndexEnd)
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
                count += line.FullLength;
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
            lineIndexStart = (lineIndexStart == -1) ? (lines.Length - 1) : lineIndexStart;
            lineIndexEnd = (lineIndexEnd == -1) ? (lines.Length - 1) : lineIndexEnd;
            return;
        }
    }

    public struct LineString
    {
        public string Text;
        public string NewLine;
        public int FullLength { get => this.Text.Length + this.NewLine.Length; }

        public static IEnumerable<LineString> Split(string input)
        {
            if (input == null) yield break;

            string terminated = input + '\0';
            int startIndex = 0;
            int i = 0;
            while (i < input.Length)
            {
                var c1 = terminated[i];
                var c2 = terminated[i + 1];
                var newLine =
                    (c1 == '\r') ? ((c2 == '\n') ? "\r\n" : "\r") :
                    (c1 == '\n') ? "\n" : null;
                if (newLine != null)
                {
                    yield return new LineString { Text = input.Substring(startIndex, i - startIndex), NewLine = newLine };
                    i += newLine.Length;
                    startIndex = i;
                }
                else
                {
                    i++;
                }
            }
            yield return new LineString { Text = input.Substring(startIndex, input.Length - startIndex), NewLine = string.Empty };
        }

        public static string Join(IEnumerable<LineString> lines)
        {
            if (lines == null) return null;

            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                sb.Append(line.Text + line.NewLine);
            }
            return sb.ToString();
        }
    }
}
