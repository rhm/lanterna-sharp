using Lanterna.Core;

namespace Lanterna.Graphics;

/// <summary>
/// This basic implementation of IThemeStyle keeps the styles in its internal state and allows you to mutate them. It can
/// be used to more easily override an existing theme and make small changes programmatically to it.
/// </summary>
public class DefaultMutableThemeStyle : IThemeStyle
{
    private ITextColor _foreground;
    private ITextColor _background;
    private HashSet<SGR> _sgrs;

    /// <summary>
    /// Creates a new DefaultMutableThemeStyle based on an existing IThemeStyle. The values of this style
    /// that is passed in will be copied into the new object that is created.
    /// </summary>
    /// <param name="themeStyleToCopy">IThemeStyle object to copy the style parameters from</param>
    public DefaultMutableThemeStyle(IThemeStyle themeStyleToCopy)
        : this(themeStyleToCopy.ForegroundColor, themeStyleToCopy.BackgroundColor, themeStyleToCopy.SGRs)
    {
    }

    /// <summary>
    /// Creates a new DefaultMutableThemeStyle with a specified style (foreground, background and SGR state)
    /// </summary>
    /// <param name="foreground">Foreground color of the text with this style</param>
    /// <param name="background">Background color of the text with this style</param>
    /// <param name="sgrs">Modifiers to apply to the text with this style</param>
    public DefaultMutableThemeStyle(ITextColor foreground, ITextColor background, params SGR[] sgrs)
        : this(foreground, background, sgrs.Length > 0 ? new HashSet<SGR>(sgrs) : new HashSet<SGR>())
    {
    }

    private DefaultMutableThemeStyle(ITextColor foreground, ITextColor background, ISet<SGR> sgrs)
    {
        _foreground = foreground ?? throw new ArgumentNullException(nameof(foreground), "Cannot set DefaultMutableThemeStyle's foreground to null");
        _background = background ?? throw new ArgumentNullException(nameof(background), "Cannot set DefaultMutableThemeStyle's background to null");
        _sgrs = new HashSet<SGR>(sgrs);
    }

    public ITextColor ForegroundColor => _foreground;

    public ITextColor BackgroundColor => _background;

    public ISet<SGR> SGRs => new HashSet<SGR>(_sgrs);

    /// <summary>
    /// Modifies the foreground color of this DefaultMutableThemeStyle to the value passed in
    /// </summary>
    /// <param name="foreground">New foreground color for this theme style</param>
    /// <returns>Itself</returns>
    public DefaultMutableThemeStyle SetForeground(ITextColor foreground)
    {
        _foreground = foreground;
        return this;
    }

    /// <summary>
    /// Modifies the background color of this DefaultMutableThemeStyle to the value passed in
    /// </summary>
    /// <param name="background">New background color for this theme style</param>
    /// <returns>Itself</returns>
    public DefaultMutableThemeStyle SetBackground(ITextColor background)
    {
        _background = background;
        return this;
    }

    /// <summary>
    /// Modifies the SGR modifiers of this DefaultMutableThemeStyle to the values passed it.
    /// </summary>
    /// <param name="sgrs">New SGR modifiers for this theme style, the values in this set will be copied into the internal state</param>
    /// <returns>Itself</returns>
    public DefaultMutableThemeStyle SetSGRs(ISet<SGR> sgrs)
    {
        _sgrs = new HashSet<SGR>(sgrs);
        return this;
    }
}