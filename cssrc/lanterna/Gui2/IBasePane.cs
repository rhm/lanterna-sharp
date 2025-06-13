using Lanterna.Core;
using Lanterna.Graphics;
using Lanterna.Input;

namespace Lanterna.Gui2;

public interface IBasePane : IComposite
{
    ITextGUI? TextGUI { get; }
    
    void Draw(ITextGUIGraphics graphics);
    
    bool IsInvalid { get; }
    
    void Invalidate();
    
    bool HandleInput(KeyStroke key);
    
    IInteractable? FocusedInteractable { get; }
    
    void SetFocusedInteractable(IInteractable? interactable);
    
    TerminalPosition? CursorPosition { get; }
    
    TerminalPosition? ToGlobal(TerminalPosition localPosition);
    
    TerminalPosition? FromGlobal(TerminalPosition position);
    
    void SetStrictFocusChange(bool strictFocusChange);
    
    void SetEnableDirectionBasedMovements(bool enableDirectionBasedMovements);
    
    Theme Theme { get; }
    
    void SetTheme(Theme? theme);
    
    void SetMenuBar(IMenuBar? menuBar);
    
    IMenuBar? MenuBar { get; }
}