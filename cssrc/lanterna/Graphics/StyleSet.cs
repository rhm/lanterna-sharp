using Lanterna.Core;

namespace Lanterna.Graphics;

public interface IStyleSet<T> where T : IStyleSet<T>
{
    /// <summary>
    /// Returns the current background color
    /// </summary>
    ITextColor BackgroundColor { get; }

    /// <summary>
    /// Updates the current background color
    /// </summary>
    T SetBackgroundColor(ITextColor backgroundColor);

    /// <summary>
    /// Returns the current foreground color
    /// </summary>
    ITextColor ForegroundColor { get; }

    /// <summary>
    /// Updates the current foreground color
    /// </summary>
    T SetForegroundColor(ITextColor foregroundColor);

    /// <summary>
    /// Adds zero or more modifiers to the set of currently active modifiers
    /// </summary>
    T EnableModifiers(params SGR[] modifiers);

    /// <summary>
    /// Removes zero or more modifiers from the set of currently active modifiers
    /// </summary>
    T DisableModifiers(params SGR[] modifiers);

    /// <summary>
    /// Sets the active modifiers to exactly the set passed in to this method. Any previous state of which modifiers are
    /// enabled doesn't matter.
    /// </summary>
    T SetModifiers(ISet<SGR> modifiers);

    /// <summary>
    /// Removes all active modifiers
    /// </summary>
    T ClearModifiers();

    /// <summary>
    /// Returns all the SGR codes that are currently active
    /// </summary>
    ISet<SGR> ActiveModifiers { get; }

    /// <summary>
    /// Copy colors and set of SGR codes
    /// </summary>
    T SetStyleFrom(IStyleSet source);
}

public class StyleSet : IStyleSet<StyleSet>, IStyleSet
{
    private ITextColor? _foregroundColor;
    private ITextColor? _backgroundColor;
    private readonly HashSet<SGR> _style = new();

    public StyleSet() { }
    
    public StyleSet(IStyleSet source)
    {
        SetStyleFrom(source);
    }

    public ITextColor BackgroundColor => _backgroundColor ?? new AnsiTextColor(AnsiColor.Default);

    public StyleSet SetBackgroundColor(ITextColor backgroundColor)
    {
        _backgroundColor = backgroundColor;
        return this;
    }

    public ITextColor ForegroundColor => _foregroundColor ?? new AnsiTextColor(AnsiColor.Default);

    public StyleSet SetForegroundColor(ITextColor foregroundColor)
    {
        _foregroundColor = foregroundColor;
        return this;
    }

    public StyleSet EnableModifiers(params SGR[] modifiers)
    {
        foreach (var modifier in modifiers)
        {
            _style.Add(modifier);
        }
        return this;
    }

    public StyleSet DisableModifiers(params SGR[] modifiers)
    {
        foreach (var modifier in modifiers)
        {
            _style.Remove(modifier);
        }
        return this;
    }

    public StyleSet SetModifiers(ISet<SGR> modifiers)
    {
        _style.Clear();
        foreach (var modifier in modifiers)
        {
            _style.Add(modifier);
        }
        return this;
    }

    public StyleSet ClearModifiers()
    {
        _style.Clear();
        return this;
    }

    public ISet<SGR> ActiveModifiers => new HashSet<SGR>(_style);

    public StyleSet SetStyleFrom(IStyleSet source)
    {
        SetBackgroundColor(source.BackgroundColor);
        SetForegroundColor(source.ForegroundColor);
        SetModifiers(source.ActiveModifiers);
        return this;
    }
}