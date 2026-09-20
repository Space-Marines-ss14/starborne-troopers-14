using Robust.Shared.Configuration;

namespace Content.Shared._ST14.Localization;

[CVarDefs]
public static class ST14CVars
{
    public static readonly CVarDef<string> ClientLanguage =
        CVarDef.Create("st14.language", "", CVar.ARCHIVE);

    public static readonly CVarDef<string> ServerLanguage =
        CVarDef.Create("st14.server_language", "", CVar.ARCHIVE);
}
