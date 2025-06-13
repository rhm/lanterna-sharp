using Lanterna.Core;
using Lanterna.Graphics;
using Lanterna.Screen;

namespace Lanterna.Gui2;

public interface ITextGUIGraphics : ITextGraphics, IThemedTextGraphics
{
    ITextGUI TextGUI { get; }
    
    new ITextGUIGraphics NewTextGraphics(TerminalPosition topLeftCorner, TerminalSize size);
    
    new ITextGUIGraphics ApplyThemeStyle(IThemeStyle themeStyle);
    
    new ITextGUIGraphics SetBackgroundColor(ITextColor backgroundColor);
    
    new ITextGUIGraphics SetForegroundColor(ITextColor foregroundColor);
    
    new ITextGUIGraphics EnableModifiers(params SGR[] modifiers);
    
    new ITextGUIGraphics DisableModifiers(params SGR[] modifiers);
    
    new ITextGUIGraphics SetModifiers(ISet<SGR> modifiers);
    
    new ITextGUIGraphics ClearModifiers();
    
    new ITextGUIGraphics SetTabBehaviour(TabBehaviour tabBehaviour);
    
    new ITextGUIGraphics Fill(char c);
    
    new ITextGUIGraphics FillRectangle(TerminalPosition topLeft, TerminalSize size, char character);
    
    new ITextGUIGraphics FillRectangle(TerminalPosition topLeft, TerminalSize size, TextCharacter character);
    
    new ITextGUIGraphics DrawRectangle(TerminalPosition topLeft, TerminalSize size, char character);
    
    new ITextGUIGraphics DrawRectangle(TerminalPosition topLeft, TerminalSize size, TextCharacter character);
    
    new ITextGUIGraphics FillTriangle(TerminalPosition p1, TerminalPosition p2, TerminalPosition p3, char character);
    
    new ITextGUIGraphics FillTriangle(TerminalPosition p1, TerminalPosition p2, TerminalPosition p3, TextCharacter character);
    
    new ITextGUIGraphics DrawTriangle(TerminalPosition p1, TerminalPosition p2, TerminalPosition p3, char character);
    
    new ITextGUIGraphics DrawTriangle(TerminalPosition p1, TerminalPosition p2, TerminalPosition p3, TextCharacter character);
    
    new ITextGUIGraphics DrawLine(TerminalPosition fromPoint, TerminalPosition toPoint, char character);
    
    new ITextGUIGraphics DrawLine(TerminalPosition fromPoint, TerminalPosition toPoint, TextCharacter character);
    
    new ITextGUIGraphics DrawLine(int fromX, int fromY, int toX, int toY, char character);
    
    new ITextGUIGraphics DrawLine(int fromX, int fromY, int toX, int toY, TextCharacter character);
    
    new ITextGUIGraphics DrawImage(TerminalPosition topLeft, ITextImage image);
    
    new ITextGUIGraphics DrawImage(TerminalPosition topLeft, ITextImage image, TerminalPosition sourceImageTopLeft, TerminalSize sourceImageSize);
    
    new ITextGUIGraphics SetCharacter(TerminalPosition position, char character);
    
    new ITextGUIGraphics SetCharacter(TerminalPosition position, TextCharacter character);
    
    new ITextGUIGraphics SetCharacter(int column, int row, char character);
    
    new ITextGUIGraphics SetCharacter(int column, int row, TextCharacter character);
    
    new ITextGUIGraphics PutString(int column, int row, string text);
    
    new ITextGUIGraphics PutString(TerminalPosition position, string text);
    
    new ITextGUIGraphics PutString(int column, int row, string text, SGR extraModifier, params SGR[] optionalExtraModifiers);
    
    new ITextGUIGraphics PutString(TerminalPosition position, string text, SGR extraModifier, params SGR[] optionalExtraModifiers);
    
    new ITextGUIGraphics PutString(int column, int row, string text, ICollection<SGR> extraModifiers);
    
    new ITextGUIGraphics PutCSIStyledString(int column, int row, string text);
    
    new ITextGUIGraphics PutCSIStyledString(TerminalPosition position, string text);
    
    new ITextGUIGraphics SetStyleFrom(IStyleSet source);
}