using System;
using System.Collections.Generic;
using System.Linq;

namespace Suconbu.Dentacs
{
    static class TextBoxHelper
    {
        // TextBox::GetCharacterIndexFromLineIndex does not work correctly in full-sreen mode.
        public static int GetCharacterIndexFromLineIndex(IEnumerable<string> lines, int lineIndex)
        {
            return lines.Take(lineIndex).Select(line => line.Length + Environment.NewLine.Length).Sum();
        }
    }
}
