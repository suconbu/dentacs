using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;

namespace Suconbu.Dentacs
{
    class LanguageHelper
    {
        public static ResourceDictionary GetStringResourceDictionary(string twoLetterISOLanguageName = null)
        {
            var language = twoLetterISOLanguageName ?? CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            return new ResourceDictionary()
            {
                Source = new Uri($"/Resource/String.{language}.xaml", UriKind.Relative)
            };
        }
    }
}
