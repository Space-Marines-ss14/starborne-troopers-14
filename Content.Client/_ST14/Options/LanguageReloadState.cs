using Robust.Client.State;

namespace Content.Client._ST14.Options;

/// <summary>
/// Empty state we switch through to rebuild every window after a language change
/// </summary>
public sealed class LanguageReloadState : State
{
    protected override void Startup()
    {
    }

    protected override void Shutdown()
    {
    }
}
