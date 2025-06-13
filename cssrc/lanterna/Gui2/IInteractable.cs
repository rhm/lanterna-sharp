using Lanterna.Core;
using Lanterna.Input;

namespace Lanterna.Gui2;

public interface IInteractable : IComponent
{
    TerminalPosition? CursorLocation { get; }
    
    Result HandleInput(KeyStroke keyStroke);
    
    IInteractable TakeFocus();
    
    void OnEnterFocus(FocusChangeDirection direction, IInteractable? previouslyInFocus);
    
    void OnLeaveFocus(FocusChangeDirection direction, IInteractable? nextInFocus);
    
    bool IsFocused { get; }
    
    IInteractable SetInputFilter(IInputFilter? inputFilter);
    
    IInputFilter? InputFilter { get; }
    
    bool Enabled { get; set; }
    
    bool IsFocusable { get; }
    
    public enum Result
    {
        Unhandled,
        Handled,
        MoveFocusNext,
        MoveFocusPrevious,
        MoveFocusLeft,
        MoveFocusRight,
        MoveFocusUp,
        MoveFocusDown
    }
    
    public enum FocusChangeDirection
    {
        Down,
        Right,
        Up,
        Left,
        Next,
        Previous,
        Teleport,
        Reset
    }
}