using Lanterna.Core;

namespace Lanterna.Graphics;

public class Theme
{
    public Gui2.IWindowDecorationRenderer? WindowDecorationRenderer { get; set; }
    public Gui2.IWindowPostRenderer? WindowPostRenderer { get; set; }

    public ThemeDefinition GetDefinition(Type componentType)
    {
        return new ThemeDefinition();
    }

    public T GetRenderer<T>() where T : class
    {
        throw new NotImplementedException("Theme renderer not implemented");
    }
}

public class ThemeDefinition
{
    public IThemeStyle Normal { get; } = new DefaultThemeStyle();
    public IThemeStyle Active { get; } = new DefaultThemeStyle();
    public IThemeStyle Insensitive { get; } = new DefaultThemeStyle();
    public IThemeStyle PreLight { get; } = new DefaultThemeStyle();
    public IThemeStyle Selected { get; } = new DefaultThemeStyle();

    public bool CursorVisible { get; set; } = true;

    public char GetCharacter(string name, char defaultChar)
    {
        return defaultChar;
    }

    public bool GetBooleanProperty(string name, bool defaultValue)
    {
        return defaultValue;
    }

    public T GetRenderer<T>() where T : class
    {
        throw new NotImplementedException("ThemeDefinition renderer not implemented");
    }
}

public interface IThemeStyle
{
    /// <summary>
    /// Returns the foreground color associated with this style
    /// </summary>
    ITextColor ForegroundColor { get; }

    /// <summary>
    /// Returns the background color associated with this style
    /// </summary>
    ITextColor BackgroundColor { get; }

    /// <summary>
    /// Returns the set of SGR flags associated with this style. This set is either unmodifiable or a copy so
    /// altering it will not change the theme in any way.
    /// </summary>
    ISet<SGR> SGRs { get; }
}

public class DefaultThemeStyle : IThemeStyle
{
    public ITextColor ForegroundColor { get; }
    public ITextColor BackgroundColor { get; }
    public ISet<SGR> SGRs { get; }

    public DefaultThemeStyle()
    {
        ForegroundColor = new AnsiTextColor(AnsiColor.Default);
        BackgroundColor = new AnsiTextColor(AnsiColor.Default);
        SGRs = new HashSet<SGR>();
    }

    public DefaultThemeStyle(ITextColor foregroundColor, ITextColor backgroundColor, ISet<SGR> sgrs)
    {
        ForegroundColor = foregroundColor;
        BackgroundColor = backgroundColor;
        SGRs = new HashSet<SGR>(sgrs);
    }
}

public interface IThemedTextGraphics
{
    IThemedTextGraphics ApplyThemeStyle(IThemeStyle themeStyle);
}