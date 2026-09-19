using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._ST14.Localization;

[Serializable, NetSerializable]
public sealed class RandomMetadataNameComponentState : ComponentState
{
    public readonly string Format;
    public readonly List<string> Parts;

    public RandomMetadataNameComponentState(string format, List<string> parts)
    {
        Format = format;
        Parts = parts;
    }
}
