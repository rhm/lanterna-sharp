using Lanterna.Core;
using Lanterna.Screen;

namespace Lanterna.Graphics;

/// <summary>
/// TextGraphics implementation that does nothing, but has a pre-defined size
/// </summary>
internal class NullTextGraphics : ITextGraphics
{
    private readonly TerminalSize _size;
    private ITextColor _foregroundColor;
    private ITextColor _backgroundColor;
    private TabBehaviour _tabBehaviour;
    private readonly HashSet<SGR> _activeModifiers;

    /// <summary>
    /// Creates a new NullTextGraphics that will return the specified size value if asked how big it is but other
    /// than that ignore all other calls.
    /// </summary>
    /// <param name="size">The size to report</param>
    public NullTextGraphics(TerminalSize size)
    {
        _size = size;
        _foregroundColor = new AnsiTextColor(AnsiColor.Default);
        _backgroundColor = new AnsiTextColor(AnsiColor.Default);
        _tabBehaviour = TabBehaviour.AlignToColumn4;
        _activeModifiers = new HashSet<SGR>();
    }

    /// <summary>
    /// The default implementation just returns null, as this Graphics never writes anywhere.
    /// </summary>
    /// <param name="pos">position to translate</param>
    /// <returns>null</returns>
    public TerminalPosition? ToScreenPosition(TerminalPosition? pos)
    {
        return null;
    }

    public TerminalSize Size => _size;

    public ITextGraphics NewTextGraphics(TerminalPosition topLeftCorner, TerminalSize size)
    {
        return this;
    }

    public ITextColor BackgroundColor 
    { 
        get => _backgroundColor;
        set => _backgroundColor = value;
    }

    public ITextGraphics SetBackgroundColor(ITextColor backgroundColor)
    {
        _backgroundColor = backgroundColor;
        return this;
    }

    public ITextColor ForegroundColor 
    { 
        get => _foregroundColor;
        set => _foregroundColor = value;
    }

    public ITextGraphics SetForegroundColor(ITextColor foregroundColor)
    {
        _foregroundColor = foregroundColor;
        return this;
    }

    public ITextGraphics EnableModifiers(params SGR[] modifiers)
    {
        foreach (var modifier in modifiers)
        {
            _activeModifiers.Add(modifier);
        }
        return this;
    }

    public ITextGraphics DisableModifiers(params SGR[] modifiers)
    {
        foreach (var modifier in modifiers)
        {
            _activeModifiers.Remove(modifier);
        }
        return this;
    }

    public ITextGraphics SetModifiers(ISet<SGR> modifiers)
    {
        ClearModifiers();
        foreach (var modifier in modifiers)
        {
            _activeModifiers.Add(modifier);
        }
        return this;
    }

    public ITextGraphics ClearModifiers()
    {
        _activeModifiers.Clear();
        return this;
    }

    public ISet<SGR> ActiveModifiers => new HashSet<SGR>(_activeModifiers);

    public TabBehaviour TabBehaviour 
    { 
        get => _tabBehaviour;
        set => _tabBehaviour = value;
    }

    public ITextGraphics SetTabBehaviour(TabBehaviour tabBehaviour)
    {
        _tabBehaviour = tabBehaviour;
        return this;
    }

    public ITextGraphics Fill(char c) => this;

    public ITextGraphics SetCharacter(int column, int row, char character) => this;

    public ITextGraphics SetCharacter(int column, int row, TextCharacter character) => this;

    public ITextGraphics SetCharacter(TerminalPosition position, char character) => this;

    public ITextGraphics SetCharacter(TerminalPosition position, TextCharacter character) => this;

    public ITextGraphics DrawLine(TerminalPosition fromPoint, TerminalPosition toPoint, char character) => this;

    public ITextGraphics DrawLine(TerminalPosition fromPoint, TerminalPosition toPoint, TextCharacter character) => this;

    public ITextGraphics DrawLine(int fromX, int fromY, int toX, int toY, char character) => this;

    public ITextGraphics DrawLine(int fromX, int fromY, int toX, int toY, TextCharacter character) => this;

    public ITextGraphics DrawTriangle(TerminalPosition p1, TerminalPosition p2, TerminalPosition p3, char character) => this;

    public ITextGraphics DrawTriangle(TerminalPosition p1, TerminalPosition p2, TerminalPosition p3, TextCharacter character) => this;

    public ITextGraphics FillTriangle(TerminalPosition p1, TerminalPosition p2, TerminalPosition p3, char character) => this;

    public ITextGraphics FillTriangle(TerminalPosition p1, TerminalPosition p2, TerminalPosition p3, TextCharacter character) => this;

    public ITextGraphics DrawRectangle(TerminalPosition topLeft, TerminalSize size, char character) => this;

    public ITextGraphics DrawRectangle(TerminalPosition topLeft, TerminalSize size, TextCharacter character) => this;

    public ITextGraphics FillRectangle(TerminalPosition topLeft, TerminalSize size, char character) => this;

    public ITextGraphics FillRectangle(TerminalPosition topLeft, TerminalSize size, TextCharacter character) => this;

    public ITextGraphics DrawImage(TerminalPosition topLeft, ITextImage image) => this;

    public ITextGraphics DrawImage(TerminalPosition topLeft, ITextImage image, TerminalPosition sourceImageTopLeft, TerminalSize sourceImageSize) => this;

    public ITextGraphics PutString(int column, int row, string text) => this;

    public ITextGraphics PutString(TerminalPosition position, string text) => this;

    public ITextGraphics PutString(int column, int row, string text, SGR extraModifier, params SGR[] optionalExtraModifiers) => this;

    public ITextGraphics PutString(TerminalPosition position, string text, SGR extraModifier, params SGR[] optionalExtraModifiers) => this;

    public ITextGraphics PutString(int column, int row, string text, ICollection<SGR> extraModifiers) => this;

    public ITextGraphics PutCSIStyledString(int column, int row, string text) => this;

    public ITextGraphics PutCSIStyledString(TerminalPosition position, string text) => this;

    public TextCharacter? GetCharacter(int column, int row) => null;

    public TextCharacter? GetCharacter(TerminalPosition position) => null;

    public ITextGraphics SetStyleFrom(IStyleSet source)
    {
        SetBackgroundColor(source.BackgroundColor);
        SetForegroundColor(source.ForegroundColor);
        SetModifiers(source.ActiveModifiers);
        return this;
    }
}