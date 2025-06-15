using Lanterna.Gui2;

namespace Lanterna.Graphics;

/// <summary>
/// Allows you to more easily wrap an existing theme definition and alter the behaviour in some special cases. You normally
/// create a new class that extends from this and override some of the methods to divert the call depending on what you
/// are trying to do.
/// </summary>
public class DelegatingThemeDefinition : IThemeDefinition
{
    private readonly IThemeDefinition themeDefinition;

    /// <summary>
    /// Creates a new DelegatingThemeDefinition with a default implementation that will forward all calls to the
    /// IThemeDefinition that is passed in.
    /// </summary>
    /// <param name="themeDefinition">Other theme definition to delegate all calls to</param>
    public DelegatingThemeDefinition(IThemeDefinition themeDefinition)
    {
        this.themeDefinition = themeDefinition ?? throw new ArgumentNullException(nameof(themeDefinition));
    }

    public virtual IThemeStyle Normal => themeDefinition.Normal;

    public virtual IThemeStyle PreLight => themeDefinition.PreLight;

    public virtual IThemeStyle Selected => themeDefinition.Selected;

    public virtual IThemeStyle Active => themeDefinition.Active;

    public virtual IThemeStyle Insensitive => themeDefinition.Insensitive;

    public virtual IThemeStyle? GetCustom(string name)
    {
        return themeDefinition.GetCustom(name);
    }

    public virtual IThemeStyle GetCustom(string name, IThemeStyle defaultValue)
    {
        return themeDefinition.GetCustom(name, defaultValue);
    }

    public virtual bool GetBooleanProperty(string name, bool defaultValue)
    {
        return themeDefinition.GetBooleanProperty(name, defaultValue);
    }

    public virtual bool IsCursorVisible()
    {
        return themeDefinition.IsCursorVisible();
    }

    public virtual char GetCharacter(string name, char fallback)
    {
        return themeDefinition.GetCharacter(name, fallback);
    }

    public virtual T? GetRenderer<T>() where T : class
    {
        return themeDefinition.GetRenderer<T>();
    }
}