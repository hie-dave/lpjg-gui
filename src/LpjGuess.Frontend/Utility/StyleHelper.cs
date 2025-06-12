using Gtk;

namespace LpjGuess.Frontend.Utility;

/// <summary>
/// Utility class for style-related operations.
/// </summary>
public static class StyleHelper
{
    /// <summary>
    /// Whether any colour-related tools should use a "dark mode" option.
    /// </summary>
    /// <returns>True to use dark mode option, false otherwise.</returns>
    public static bool UseDarkMode()
    {
        // GNOME-wide "prefer dark theme" setting.
        if (Settings.GetDefault()?.GtkApplicationPreferDarkTheme == true)
            return true;

        // Application-level "prefer dark theme" setting.
        if (Configuration.Instance.PreferDarkMode)
            return true;

        // GNOME-wide "dark mode" setting.
        var styleManager = Adw.StyleManager.GetDefault();
        if (styleManager != null && styleManager.Dark)
            return true;

        return false;
    }
}
