using Content.Client.Options.UI;
using Content.Shared._ST14.Localization;
using Robust.Shared;

namespace Content.Client._ST14.Options;

public static class LanguageOption
{
    public static void Add(OptionsTabControlRow control, OptionDropDown dropDown)
    {
        var entries = new List<OptionDropDownCVar<string>.ValueOption>
        {
            new(Cultures.Auto, Loc.GetString("st14-options-language-auto")),
        };

        foreach (var culture in Cultures.Supported)
        {
            entries.Add(new OptionDropDownCVar<string>.ValueOption(culture, Loc.GetString($"st14-options-language-{culture}")));
        }

        control.AddOptionDropDown(ST14CVars.ClientLanguage, dropDown, entries);
    }
}
