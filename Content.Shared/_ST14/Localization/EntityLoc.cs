using Robust.Shared.IoC;
using Robust.Shared.Localization;
using Robust.Shared.Prototypes;

namespace Content.Shared._ST14.Localization;

public static class EntityLoc
{
    public static string LocId(EntityPrototype prototype)
    {
        return prototype.CustomLocalizationID ?? $"ent-{prototype.ID}";
    }

    public static string? GetName(EntityPrototype prototype)
    {
        if (IoCManager.Resolve<ILocalizationManager>().TryGetString(LocId(prototype), out var name))
            return name;

        return prototype.SetName;
    }

    public static string? GetDescription(EntityPrototype prototype)
    {
        if (IoCManager.Resolve<ILocalizationManager>().TryGetString($"{LocId(prototype)}.desc", out var description))
            return description;

        return prototype.SetDesc;
    }
}
