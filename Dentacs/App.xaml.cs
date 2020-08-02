using System;
using System.Globalization;
using System.Windows;

namespace Suconbu.Dentacs
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var languageName = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri($"/Resource/String.{languageName}.xaml", UriKind.Relative)
            });
        }
    }
}
