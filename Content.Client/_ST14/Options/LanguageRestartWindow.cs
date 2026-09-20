using System.Numerics;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Console;
using Robust.Shared.IoC;
using Robust.Shared.Localization;
using Robust.Shared.Utility;

namespace Content.Client._ST14.Options;

/// <summary>
/// Shown after a language change since screens are built once at startup and only a restart
/// re-localizes them
/// </summary>
public sealed class LanguageRestartWindow : DefaultWindow
{
    public LanguageRestartWindow()
    {
        Title = Loc.GetString("st14-language-restart-title");
        SetSize = new Vector2(480, 190);

        var box = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            Margin = new Thickness(10),
            SeparationOverride = 12,
        };

        var message = new RichTextLabel();
        message.SetMessage(FormattedMessage.FromUnformatted(Loc.GetString("st14-language-restart-message")));
        box.AddChild(message);

        var buttons = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 10,
        };

        var now = new Button { Text = Loc.GetString("st14-language-restart-now") };
        now.OnPressed += _ => Quit();

        var later = new Button { Text = Loc.GetString("st14-language-restart-later") };
        later.OnPressed += _ => Close();

        buttons.AddChild(now);
        buttons.AddChild(later);
        box.AddChild(buttons);

        Contents.AddChild(box);
    }

    // Sandbox blocks Process so quitting is all content can do
    private static void Quit()
    {
        IoCManager.Resolve<IConsoleHost>().ExecuteCommand("quit");
    }
}
