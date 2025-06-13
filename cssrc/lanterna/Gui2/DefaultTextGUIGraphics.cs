using Lanterna.Core;
using Lanterna.Graphics;
using Lanterna.Screen;
using Lanterna.Terminal;

namespace Lanterna.Gui2;

public class DefaultTextGUIGraphics : ITextGUIGraphics
{
    private readonly ITextGUI _textGUI;
    private readonly Graphics.ITextGraphics _backend;

    public DefaultTextGUIGraphics(ITextGUI textGUI, Graphics.ITextGraphics backend)
    {
        _textGUI = textGUI;
        _backend = backend;
    }

    public ITextGUI TextGUI => _textGUI;

    public TerminalSize Size => _backend.Size;

    public ITextColor BackgroundColor 
    { 
        get => _backend.BackgroundColor;
        set => _backend.BackgroundColor = value;
    }

    public ITextColor ForegroundColor 
    { 
        get => _backend.ForegroundColor;
        set => _backend.ForegroundColor = value;
    }

    public ISet<SGR> ActiveModifiers => _backend.ActiveModifiers;

    public TabBehaviour TabBehaviour 
    { 
        get => _backend.TabBehaviour;
        set => _backend.TabBehaviour = value;
    }

    public ITextGUIGraphics NewTextGraphics(TerminalPosition topLeftCorner, TerminalSize size)
    {
        return new DefaultTextGUIGraphics(_textGUI, _backend.NewTextGraphics(topLeftCorner, size));
    }

    public ITextGUIGraphics ApplyThemeStyle(IThemeStyle themeStyle)
    {
        themeStyle?.ApplyTo(this);
        return this;
    }

    public ITextGUIGraphics SetBackgroundColor(ITextColor? backgroundColor)
    {
        if (backgroundColor != null)
        {
            _backend.BackgroundColor = backgroundColor;
        }
        return this;
    }

    public ITextGUIGraphics SetForegroundColor(ITextColor? foregroundColor)
    {
        if (foregroundColor != null)
        {
            _backend.ForegroundColor = foregroundColor;
        }
        return this;
    }

    public ITextGUIGraphics EnableModifiers(params SGR[] modifiers)
    {
        _backend.EnableModifiers(modifiers);
        return this;
    }

    public ITextGUIGraphics DisableModifiers(params SGR[] modifiers)
    {
        _backend.DisableModifiers(modifiers);
        return this;
    }

    public ITextGUIGraphics SetModifiers(ISet<SGR> modifiers)
    {
        _backend.SetModifiers(modifiers);
        return this;
    }

    public ITextGUIGraphics ClearModifiers()
    {
        _backend.ClearModifiers();
        return this;
    }

    public ITextGUIGraphics SetTabBehaviour(TabBehaviour tabBehaviour)
    {
        _backend.TabBehaviour = tabBehaviour;
        return this;
    }

    public ITextGUIGraphics Fill(char c)
    {
        _backend.Fill(c);
        return this;
    }

    public ITextGUIGraphics FillRectangle(TerminalPosition topLeft, TerminalSize size, char character)
    {
        _backend.FillRectangle(topLeft, size, character);
        return this;
    }

    public ITextGUIGraphics FillRectangle(TerminalPosition topLeft, TerminalSize size, TextCharacter character)
    {
        _backend.FillRectangle(topLeft, size, character);
        return this;
    }

    public ITextGUIGraphics DrawRectangle(TerminalPosition topLeft, TerminalSize size, char character)
    {
        _backend.DrawRectangle(topLeft, size, character);
        return this;
    }

    public ITextGUIGraphics DrawRectangle(TerminalPosition topLeft, TerminalSize size, TextCharacter character)
    {
        _backend.DrawRectangle(topLeft, size, character);
        return this;
    }

    public ITextGUIGraphics FillTriangle(TerminalPosition p1, TerminalPosition p2, TerminalPosition p3, char character)
    {
        _backend.FillTriangle(p1, p2, p3, character);
        return this;
    }

    public ITextGUIGraphics FillTriangle(TerminalPosition p1, TerminalPosition p2, TerminalPosition p3, TextCharacter character)
    {
        _backend.FillTriangle(p1, p2, p3, character);
        return this;
    }

    public ITextGUIGraphics DrawTriangle(TerminalPosition p1, TerminalPosition p2, TerminalPosition p3, char character)
    {
        _backend.DrawTriangle(p1, p2, p3, character);
        return this;
    }

    public ITextGUIGraphics DrawTriangle(TerminalPosition p1, TerminalPosition p2, TerminalPosition p3, TextCharacter character)
    {
        _backend.DrawTriangle(p1, p2, p3, character);
        return this;
    }

    public ITextGUIGraphics DrawLine(TerminalPosition fromPoint, TerminalPosition toPoint, char character)
    {
        _backend.DrawLine(fromPoint, toPoint, character);
        return this;
    }

    public ITextGUIGraphics DrawLine(TerminalPosition fromPoint, TerminalPosition toPoint, TextCharacter character)
    {
        _backend.DrawLine(fromPoint, toPoint, character);
        return this;
    }

    public ITextGUIGraphics DrawLine(int fromX, int fromY, int toX, int toY, char character)
    {
        _backend.DrawLine(fromX, fromY, toX, toY, character);
        return this;
    }

    public ITextGUIGraphics DrawLine(int fromX, int fromY, int toX, int toY, TextCharacter character)
    {
        _backend.DrawLine(fromX, fromY, toX, toY, character);
        return this;
    }

    public ITextGUIGraphics DrawImage(TerminalPosition topLeft, ITextImage image)
    {
        _backend.DrawImage(topLeft, image);
        return this;
    }

    public ITextGUIGraphics DrawImage(TerminalPosition topLeft, ITextImage image, TerminalPosition sourceImageTopLeft, TerminalSize sourceImageSize)
    {
        _backend.DrawImage(topLeft, image, sourceImageTopLeft, sourceImageSize);
        return this;
    }

    public ITextGUIGraphics SetCharacter(TerminalPosition position, char character)
    {
        _backend.SetCharacter(position, character);
        return this;
    }

    public ITextGUIGraphics SetCharacter(TerminalPosition position, TextCharacter character)
    {
        _backend.SetCharacter(position, character);
        return this;
    }

    public ITextGUIGraphics SetCharacter(int column, int row, char character)
    {
        _backend.SetCharacter(column, row, character);
        return this;
    }

    public ITextGUIGraphics SetCharacter(int column, int row, TextCharacter character)
    {
        _backend.SetCharacter(column, row, character);
        return this;
    }

    public ITextGUIGraphics PutString(int column, int row, string text)
    {
        _backend.PutString(column, row, text);
        return this;
    }

    public ITextGUIGraphics PutString(TerminalPosition position, string text)
    {
        _backend.PutString(position, text);
        return this;
    }

    public ITextGUIGraphics PutString(int column, int row, string text, SGR extraModifier, params SGR[] optionalExtraModifiers)
    {
        _backend.PutString(column, row, text, extraModifier, optionalExtraModifiers);
        return this;
    }

    public ITextGUIGraphics PutString(TerminalPosition position, string text, SGR extraModifier, params SGR[] optionalExtraModifiers)
    {
        _backend.PutString(position, text, extraModifier, optionalExtraModifiers);
        return this;
    }

    public ITextGUIGraphics PutString(int column, int row, string text, ICollection<SGR> extraModifiers)
    {
        _backend.PutString(column, row, text, extraModifiers);
        return this;
    }

    public ITextGUIGraphics PutCSIStyledString(int column, int row, string text)
    {
        _backend.PutCSIStyledString(column, row, text);
        return this;
    }

    public ITextGUIGraphics PutCSIStyledString(TerminalPosition position, string text)
    {
        _backend.PutCSIStyledString(position, text);
        return this;
    }

    public ITextGUIGraphics SetStyleFrom(IStyleSet source)
    {
        _backend.SetStyleFrom(source);
        return this;
    }

    // Explicit interface implementations for Graphics.ITextGraphics that return ITextGraphics instead of ITextGUIGraphics
    Graphics.ITextGraphics Graphics.ITextGraphics.NewTextGraphics(TerminalPosition topLeftCorner, TerminalSize size) => 
        NewTextGraphics(topLeftCorner, size);

    Graphics.ITextGraphics Graphics.ITextGraphics.SetBackgroundColor(ITextColor backgroundColor) => 
        SetBackgroundColor(backgroundColor);

    Graphics.ITextGraphics Graphics.ITextGraphics.SetForegroundColor(ITextColor foregroundColor) => 
        SetForegroundColor(foregroundColor);

    Graphics.ITextGraphics Graphics.ITextGraphics.EnableModifiers(params SGR[] modifiers) => 
        EnableModifiers(modifiers);

    Graphics.ITextGraphics Graphics.ITextGraphics.DisableModifiers(params SGR[] modifiers) => 
        DisableModifiers(modifiers);

    Graphics.ITextGraphics Graphics.ITextGraphics.SetModifiers(ISet<SGR> modifiers) => 
        SetModifiers(modifiers);

    Graphics.ITextGraphics Graphics.ITextGraphics.ClearModifiers() => 
        ClearModifiers();

    Graphics.ITextGraphics Graphics.ITextGraphics.SetTabBehaviour(TabBehaviour tabBehaviour) => 
        SetTabBehaviour(tabBehaviour);

    Graphics.ITextGraphics Graphics.ITextGraphics.Fill(char c) => 
        Fill(c);

    Graphics.ITextGraphics Graphics.ITextGraphics.FillRectangle(TerminalPosition topLeft, TerminalSize size, char character) => 
        FillRectangle(topLeft, size, character);

    Graphics.ITextGraphics Graphics.ITextGraphics.FillRectangle(TerminalPosition topLeft, TerminalSize size, TextCharacter character) => 
        FillRectangle(topLeft, size, character);

    Graphics.ITextGraphics Graphics.ITextGraphics.DrawRectangle(TerminalPosition topLeft, TerminalSize size, char character) => 
        DrawRectangle(topLeft, size, character);

    Graphics.ITextGraphics Graphics.ITextGraphics.DrawRectangle(TerminalPosition topLeft, TerminalSize size, TextCharacter character) => 
        DrawRectangle(topLeft, size, character);

    Graphics.ITextGraphics Graphics.ITextGraphics.FillTriangle(TerminalPosition p1, TerminalPosition p2, TerminalPosition p3, char character) => 
        FillTriangle(p1, p2, p3, character);

    Graphics.ITextGraphics Graphics.ITextGraphics.FillTriangle(TerminalPosition p1, TerminalPosition p2, TerminalPosition p3, TextCharacter character) => 
        FillTriangle(p1, p2, p3, character);

    Graphics.ITextGraphics Graphics.ITextGraphics.DrawTriangle(TerminalPosition p1, TerminalPosition p2, TerminalPosition p3, char character) => 
        DrawTriangle(p1, p2, p3, character);

    Graphics.ITextGraphics Graphics.ITextGraphics.DrawTriangle(TerminalPosition p1, TerminalPosition p2, TerminalPosition p3, TextCharacter character) => 
        DrawTriangle(p1, p2, p3, character);

    Graphics.ITextGraphics Graphics.ITextGraphics.DrawLine(TerminalPosition fromPoint, TerminalPosition toPoint, char character) => 
        DrawLine(fromPoint, toPoint, character);

    Graphics.ITextGraphics Graphics.ITextGraphics.DrawLine(TerminalPosition fromPoint, TerminalPosition toPoint, TextCharacter character) => 
        DrawLine(fromPoint, toPoint, character);

    Graphics.ITextGraphics Graphics.ITextGraphics.DrawLine(int fromX, int fromY, int toX, int toY, char character) => 
        DrawLine(fromX, fromY, toX, toY, character);

    Graphics.ITextGraphics Graphics.ITextGraphics.DrawLine(int fromX, int fromY, int toX, int toY, TextCharacter character) => 
        DrawLine(fromX, fromY, toX, toY, character);

    Graphics.ITextGraphics Graphics.ITextGraphics.DrawImage(TerminalPosition topLeft, ITextImage image) => 
        DrawImage(topLeft, image);

    Graphics.ITextGraphics Graphics.ITextGraphics.DrawImage(TerminalPosition topLeft, ITextImage image, TerminalPosition sourceImageTopLeft, TerminalSize sourceImageSize) => 
        DrawImage(topLeft, image, sourceImageTopLeft, sourceImageSize);

    Graphics.ITextGraphics Graphics.ITextGraphics.SetCharacter(TerminalPosition position, char character) => 
        SetCharacter(position, character);

    Graphics.ITextGraphics Graphics.ITextGraphics.SetCharacter(TerminalPosition position, TextCharacter character) => 
        SetCharacter(position, character);

    Graphics.ITextGraphics Graphics.ITextGraphics.SetCharacter(int column, int row, char character) => 
        SetCharacter(column, row, character);

    Graphics.ITextGraphics Graphics.ITextGraphics.SetCharacter(int column, int row, TextCharacter character) => 
        SetCharacter(column, row, character);

    Graphics.ITextGraphics Graphics.ITextGraphics.PutString(int column, int row, string text) => 
        PutString(column, row, text);

    Graphics.ITextGraphics Graphics.ITextGraphics.PutString(TerminalPosition position, string text) => 
        PutString(position, text);

    Graphics.ITextGraphics Graphics.ITextGraphics.PutString(int column, int row, string text, SGR extraModifier, params SGR[] optionalExtraModifiers) => 
        PutString(column, row, text, extraModifier, optionalExtraModifiers);

    Graphics.ITextGraphics Graphics.ITextGraphics.PutString(TerminalPosition position, string text, SGR extraModifier, params SGR[] optionalExtraModifiers) => 
        PutString(position, text, extraModifier, optionalExtraModifiers);

    Graphics.ITextGraphics Graphics.ITextGraphics.PutString(int column, int row, string text, ICollection<SGR> extraModifiers) => 
        PutString(column, row, text, extraModifiers);

    Graphics.ITextGraphics Graphics.ITextGraphics.PutCSIStyledString(int column, int row, string text) => 
        PutCSIStyledString(column, row, text);

    Graphics.ITextGraphics Graphics.ITextGraphics.PutCSIStyledString(TerminalPosition position, string text) => 
        PutCSIStyledString(position, text);

    Graphics.ITextGraphics Graphics.ITextGraphics.SetStyleFrom(IStyleSet source) => 
        SetStyleFrom(source);

    Graphics.IThemedTextGraphics Graphics.IThemedTextGraphics.ApplyThemeStyle(IThemeStyle themeStyle) => 
        ApplyThemeStyle(themeStyle);
}