using Robust.Shared.GameStates;

namespace Content.Shared._ST14.Localization;

[RegisterComponent, NetworkedComponent]
public sealed partial class RandomMetadataNameComponent : Component
{
    [DataField]
    public string Format = string.Empty;

    [DataField]
    public List<string> Parts = new();
}
