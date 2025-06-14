using Lanterna.Core;
using Lanterna.Screen;

namespace Lanterna.Graphics;

public interface ITextGraphics
{
    TerminalSize Size { get; }
    ITextColor BackgroundColor { get; set; }
    ITextColor ForegroundColor { get; set; }
    ISet<SGR> ActiveModifiers { get; }
    TabBehaviour TabBehaviour { get; set; }

    ITextGraphics NewTextGraphics(TerminalPosition topLeftCorner, TerminalSize size);
    ITextGraphics SetBackgroundColor(ITextColor backgroundColor);
    ITextGraphics SetForegroundColor(ITextColor foregroundColor);
    ITextGraphics EnableModifiers(params SGR[] modifiers);
    ITextGraphics DisableModifiers(params SGR[] modifiers);
    ITextGraphics SetModifiers(ISet<SGR> modifiers);
    ITextGraphics ClearModifiers();
    ITextGraphics SetTabBehaviour(TabBehaviour tabBehaviour);
    ITextGraphics Fill(char c);
    ITextGraphics FillRectangle(TerminalPosition topLeft, TerminalSize size, char character);
    ITextGraphics FillRectangle(TerminalPosition topLeft, TerminalSize size, TextCharacter character);
    ITextGraphics DrawRectangle(TerminalPosition topLeft, TerminalSize size, char character);
    ITextGraphics DrawRectangle(TerminalPosition topLeft, TerminalSize size, TextCharacter character);
    ITextGraphics FillTriangle(TerminalPosition p1, TerminalPosition p2, TerminalPosition p3, char character);
    ITextGraphics FillTriangle(TerminalPosition p1, TerminalPosition p2, TerminalPosition p3, TextCharacter character);
    ITextGraphics DrawTriangle(TerminalPosition p1, TerminalPosition p2, TerminalPosition p3, char character);
    ITextGraphics DrawTriangle(TerminalPosition p1, TerminalPosition p2, TerminalPosition p3, TextCharacter character);
    ITextGraphics DrawLine(TerminalPosition fromPoint, TerminalPosition toPoint, char character);
    ITextGraphics DrawLine(TerminalPosition fromPoint, TerminalPosition toPoint, TextCharacter character);
    ITextGraphics DrawLine(int fromX, int fromY, int toX, int toY, char character);
    ITextGraphics DrawLine(int fromX, int fromY, int toX, int toY, TextCharacter character);
    ITextGraphics DrawImage(TerminalPosition topLeft, ITextImage image);
    ITextGraphics DrawImage(TerminalPosition topLeft, ITextImage image, TerminalPosition sourceImageTopLeft, TerminalSize sourceImageSize);
    ITextGraphics SetCharacter(TerminalPosition position, char character);
    ITextGraphics SetCharacter(TerminalPosition position, TextCharacter character);
    ITextGraphics SetCharacter(int column, int row, char character);
    ITextGraphics SetCharacter(int column, int row, TextCharacter character);
    TextCharacter? GetCharacter(int column, int row);
    TextCharacter? GetCharacter(TerminalPosition position);
    ITextGraphics PutString(int column, int row, string text);
    ITextGraphics PutString(TerminalPosition position, string text);
    ITextGraphics PutString(int column, int row, string text, SGR extraModifier, params SGR[] optionalExtraModifiers);
    ITextGraphics PutString(TerminalPosition position, string text, SGR extraModifier, params SGR[] optionalExtraModifiers);
    ITextGraphics PutString(int column, int row, string text, ICollection<SGR> extraModifiers);
    ITextGraphics PutCSIStyledString(int column, int row, string text);
    ITextGraphics PutCSIStyledString(TerminalPosition position, string text);
    ITextGraphics SetStyleFrom(IStyleSet source);
}

public interface ITextImage : IScrollable
{
    TerminalSize Size { get; }
    TextCharacter? GetCharacterAt(TerminalPosition position);
    TextCharacter? GetCharacterAt(int column, int row);
    void SetCharacterAt(TerminalPosition position, TextCharacter character);
    void SetCharacterAt(int column, int row, TextCharacter character);
    void SetAll(TextCharacter character);
    ITextGraphics NewTextGraphics();
    ITextImage Resize(TerminalSize newSize, TextCharacter filler);
    void CopyTo(ITextImage destination);
    void CopyTo(ITextImage destination, int startRowIndex, int rows, int startColumnIndex, int columns, int destinationRowOffset, int destinationColumnOffset);
}

public interface IScrollable
{
    void ScrollLines(int firstLine, int lastLine, int distance);
}

public interface IStyleSet
{
    ITextColor ForegroundColor { get; }
    ITextColor BackgroundColor { get; }
    ISet<SGR> ActiveModifiers { get; }
}

