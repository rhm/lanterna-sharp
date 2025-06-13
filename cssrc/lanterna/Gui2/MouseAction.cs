using Lanterna.Core;
using Lanterna.Input;

namespace Lanterna.Gui2;

public class MouseAction : KeyStroke
{
    public MouseActionType ActionType { get; set; }
    public TerminalPosition Position { get; set; }
    public int Button { get; set; }
    
    public MouseAction(MouseActionType actionType, int button, TerminalPosition position)
        : base(KeyType.MouseEvent)
    {
        ActionType = actionType;
        Button = button;
        Position = position;
    }
    
    public bool IsMouseDown => ActionType == MouseActionType.ClickDown;
    public bool IsMouseUp => ActionType == MouseActionType.ClickRelease;
    public bool IsMouseDrag => ActionType == MouseActionType.Drag;
    public bool IsMouseMove => ActionType == MouseActionType.Move;
}

public enum MouseActionType
{
    ClickDown,
    ClickRelease,
    Drag,
    Move,
    ScrollUp,
    ScrollDown
}