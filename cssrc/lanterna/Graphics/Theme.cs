using Lanterna.Core;
using Lanterna.Gui2;
using System.Collections.Generic;

namespace Lanterna.Graphics;

/// <summary>
/// Interface for Theme implementations that provide style information for GUI components
/// </summary>
public interface ITheme
{
    /// <summary>
    /// Returns the default theme definition that will be used if there is no specific definition for a class
    /// </summary>
    IThemeDefinition GetDefaultDefinition();

    /// <summary>
    /// Returns the theme definition for a specific class
    /// </summary>
    IThemeDefinition GetDefinition(Type clazz);

    /// <summary>
    /// Returns the window post renderer associated with this theme, or null if there is none
    /// </summary>
    IWindowPostRenderer? GetWindowPostRenderer();

    /// <summary>
    /// Returns the window decoration renderer associated with this theme, or null if there is none
    /// </summary>
    IWindowDecorationRenderer? GetWindowDecorationRenderer();
}

/// <summary>
/// Theme definition interface that provides style information for different component states
/// </summary>
public interface IThemeDefinition
{
    /// <summary>
    /// Returns the theme style for a component in its normal state
    /// </summary>
    IThemeStyle Normal { get; }

    /// <summary>
    /// Returns the theme style for a component when it's in pre-light state (mouse hover)
    /// </summary>
    IThemeStyle PreLight { get; }

    /// <summary>
    /// Returns the theme style for a component when it's selected
    /// </summary>
    IThemeStyle Selected { get; }

    /// <summary>
    /// Returns the theme style for a component when it's active (focused)
    /// </summary>
    IThemeStyle Active { get; }

    /// <summary>
    /// Returns the theme style for a component when it's insensitive (disabled)
    /// </summary>
    IThemeStyle Insensitive { get; }

    /// <summary>
    /// Returns a custom theme style by name
    /// </summary>
    IThemeStyle? GetCustom(string name);

    /// <summary>
    /// Returns a custom theme style by name with a default fallback
    /// </summary>
    IThemeStyle GetCustom(string name, IThemeStyle defaultValue);

    /// <summary>
    /// Returns a character associated with a symbolic name
    /// </summary>
    char GetCharacter(string name, char fallback);

    /// <summary>
    /// Returns whether the cursor should be visible for this theme definition
    /// </summary>
    bool IsCursorVisible();

    /// <summary>
    /// Returns a boolean property value by name
    /// </summary>
    bool GetBooleanProperty(string name, bool defaultValue);

    /// <summary>
    /// Returns a component renderer for the specified type
    /// </summary>
    T? GetRenderer<T>() where T : class;
}

/// <summary>
/// Theme style interface that provides color and SGR information
/// </summary>
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
    /// Returns the set of SGR flags associated with this style
    /// </summary>
    ISet<SGR> SGRs { get; }
}

/// <summary>
/// Default implementation of IThemeStyle
/// </summary>
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

/// <summary>
/// Interface for themed text graphics that can apply theme styles
/// </summary>
public interface IThemedTextGraphics
{
    IThemedTextGraphics ApplyThemeStyle(IThemeStyle themeStyle);
}

/// <summary>
/// Legacy Theme class for backward compatibility - implements ITheme
/// </summary>
public class Theme : ITheme
{
    public IWindowDecorationRenderer? WindowDecorationRenderer { get; set; }
    public IWindowPostRenderer? WindowPostRenderer { get; set; }

    public virtual IThemeDefinition GetDefaultDefinition()
    {
        return new ThemeDefinition();
    }

    public virtual IThemeDefinition GetDefinition(Type componentType)
    {
        return GetDefaultDefinition();
    }

    public virtual IWindowPostRenderer? GetWindowPostRenderer()
    {
        return WindowPostRenderer;
    }

    public virtual IWindowDecorationRenderer? GetWindowDecorationRenderer()
    {
        return WindowDecorationRenderer;
    }

    public T? GetRenderer<T>() where T : class
    {
        throw new NotImplementedException("Theme renderer not implemented");
    }
}

/// <summary>
/// Legacy ThemeDefinition class for backward compatibility - implements IThemeDefinition
/// </summary>
public class ThemeDefinition : IThemeDefinition
{
    public IThemeStyle Normal { get; } = new DefaultThemeStyle();
    public IThemeStyle Active { get; } = new DefaultThemeStyle();
    public IThemeStyle Insensitive { get; } = new DefaultThemeStyle();
    public IThemeStyle PreLight { get; } = new DefaultThemeStyle();
    public IThemeStyle Selected { get; } = new DefaultThemeStyle();

    public bool CursorVisible { get; set; } = true;

    public virtual IThemeStyle? GetCustom(string name)
    {
        return null;
    }

    public virtual IThemeStyle GetCustom(string name, IThemeStyle defaultValue)
    {
        return GetCustom(name) ?? defaultValue;
    }

    public virtual char GetCharacter(string name, char fallback)
    {
        return fallback;
    }

    public virtual bool IsCursorVisible()
    {
        return CursorVisible;
    }

    public virtual bool GetBooleanProperty(string name, bool defaultValue)
    {
        return defaultValue;
    }

    public virtual T? GetRenderer<T>() where T : class
    {
        throw new NotImplementedException("ThemeDefinition renderer not implemented");
    }
}