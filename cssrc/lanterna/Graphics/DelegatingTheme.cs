using Lanterna.Gui2;

namespace Lanterna.Graphics;

/// <summary>
/// Allows you to more easily wrap an existing theme and alter the behaviour in some special cases. You normally create a
/// new class that extends from this and override some of the methods to divert the call depending on what you are trying
/// to do. 
/// </summary>
public class DelegatingTheme : ITheme
{
    private readonly ITheme theme;

    /// <summary>
    /// Creates a new DelegatingTheme with a default implementation that will forward all calls to the
    /// ITheme that is passed in.
    /// </summary>
    /// <param name="theme">Other theme to delegate all calls to</param>
    public DelegatingTheme(ITheme theme)
    {
        this.theme = theme ?? throw new ArgumentNullException(nameof(theme));
    }

    public virtual IThemeDefinition GetDefaultDefinition()
    {
        return theme.GetDefaultDefinition();
    }

    public virtual IThemeDefinition GetDefinition(Type clazz)
    {
        return theme.GetDefinition(clazz);
    }

    public virtual IWindowPostRenderer? GetWindowPostRenderer()
    {
        return theme.GetWindowPostRenderer();
    }

    public virtual IWindowDecorationRenderer? GetWindowDecorationRenderer()
    {
        return theme.GetWindowDecorationRenderer();
    }
}