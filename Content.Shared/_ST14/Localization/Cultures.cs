using System.Globalization;
using System.Linq;
using Robust.Shared.Configuration;

namespace Content.Shared._ST14.Localization;

public static class Cultures
{
    // Base culture of the content, must match ContentLocalizationManager.Culture.
    public const string Default = "ru-RU";

    // Empty means "no explicit pick", the client follows the server language.
    public const string Auto = "";

    public static readonly string[] Supported =
    {
        "en-US",
        "ru-RU",
    };

    public static bool IsSupported(string culture)
    {
        return Supported.Contains(culture);
    }

    public static CultureInfo Resolve(IConfigurationManager configuration)
    {
        var chosen = configuration.GetCVar(ST14CVars.ClientLanguage);

        if (IsSupported(chosen))
            return new CultureInfo(chosen);

        return new CultureInfo(Default);
    }

    public static CultureInfo ResolveServer(IConfigurationManager configuration)
    {
        var chosen = configuration.GetCVar(ST14CVars.ServerLanguage);

        if (IsSupported(chosen))
            return new CultureInfo(chosen);

        return new CultureInfo(Default);
    }
}
