using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Shared._ST14.Localization;

[Serializable, NetSerializable]
public sealed class SetLanguageMessage : EntityEventArgs
{
    public readonly string Culture;

    public SetLanguageMessage(string culture)
    {
        Culture = culture;
    }
}
