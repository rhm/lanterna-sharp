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
    void ApplyTo(object graphics); // Use object to avoid circular reference for now
}

public class DefaultThemeStyle : IThemeStyle
{
    public void ApplyTo(object graphics)
    {
        // Default implementation - no special styling
    }
}

public interface IThemedTextGraphics
{
    IThemedTextGraphics ApplyThemeStyle(IThemeStyle themeStyle);
}