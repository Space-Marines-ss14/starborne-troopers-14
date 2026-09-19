using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Shared._ST14.Localization;

[Serializable, NetSerializable]
public sealed class ServerLanguageMessage : EntityEventArgs
{
    public readonly string Culture;

    public ServerLanguageMessage(string culture)
    {
        Culture = culture;
    }
}
