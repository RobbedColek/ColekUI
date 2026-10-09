using System.Globalization;

namespace ColekUI.Sample.Pages;

public sealed class SettingsViewModel
{
    private static readonly string[] Cultures = ["en-US", "pl-PL"];

    public string[] Languages { get; } = ["English", "Polski"];

    public int SelectedLanguage
    {
        get => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "pl" ? 1 : 0;
        set
        {
            if (value < 0 || value >= Cultures.Length)
                return;

            var culture = CultureInfo.GetCultureInfo(Cultures[value]);
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }
    }
}
