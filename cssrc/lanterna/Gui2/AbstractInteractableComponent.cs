using Lanterna.Core;
using Lanterna.Input;

namespace Lanterna.Gui2;

public abstract class AbstractInteractableComponent<T> : AbstractComponent<T>, IInteractable 
    where T : class, IInteractable
{
    private IInputFilter? _inputFilter;
    private bool _inFocus;
    private bool _enabled;

    protected AbstractInteractableComponent()
    {
        _inputFilter = null;
        _inFocus = false;
        _enabled = true;
    }

    public virtual IInteractable TakeFocus()
    {
        if (!Enabled)
        {
            return this;
        }
        
        var basePane = BasePane;
        if (basePane != null)
        {
            basePane.SetFocusedInteractable(this);
        }
        return this;
    }

    public void OnEnterFocus(IInteractable.FocusChangeDirection direction, IInteractable? previouslyInFocus)
    {
        _inFocus = true;
        AfterEnterFocus(direction, previouslyInFocus);
    }

    protected virtual void AfterEnterFocus(IInteractable.FocusChangeDirection direction, IInteractable? previouslyInFocus)
    {
        // By default no action
    }

    public void OnLeaveFocus(IInteractable.FocusChangeDirection direction, IInteractable? nextInFocus)
    {
        _inFocus = false;
        AfterLeaveFocus(direction, nextInFocus);
    }

    protected virtual void AfterLeaveFocus(IInteractable.FocusChangeDirection direction, IInteractable? nextInFocus)
    {
        // By default no action
    }

    protected abstract override IComponentRenderer<T> CreateDefaultRenderer();

    public new IInteractableRenderer<T> Renderer => (IInteractableRenderer<T>)base.Renderer;

    public IInteractableRenderer<T> GetRenderer()
    {
        return Renderer;
    }

    public virtual bool IsFocused => _inFocus;

    public virtual bool Enabled
    {
        get => _enabled;
        set
        {
            lock (this)
            {
                _enabled = value;
                if (!value && IsFocused)
                {
                    var basePane = BasePane;
                    if (basePane != null)
                    {
                        basePane.SetFocusedInteractable(null);
                    }
                }
            }
        }
    }

    public virtual bool IsFocusable => true;

    public IInteractable.Result HandleInput(KeyStroke keyStroke)
    {
        lock (this)
        {
            if (_inputFilter == null || _inputFilter.ShouldAllowInput(keyStroke))
            {
                return HandleKeyStroke(keyStroke);
            }
            else
            {
                return IInteractable.Result.Unhandled;
            }
        }
    }

    protected virtual IInteractable.Result HandleKeyStroke(KeyStroke keyStroke)
    {
        // Skip the keystroke if ctrl, alt or shift was down
        if (!keyStroke.AltDown && !keyStroke.CtrlDown && !keyStroke.ShiftDown)
        {
            switch (keyStroke.KeyType)
            {
                case KeyType.ArrowDown:
                    return IInteractable.Result.MoveFocusDown;
                case KeyType.ArrowLeft:
                    return IInteractable.Result.MoveFocusLeft;
                case KeyType.ArrowRight:
                    return IInteractable.Result.MoveFocusRight;
                case KeyType.ArrowUp:
                    return IInteractable.Result.MoveFocusUp;
                case KeyType.Tab:
                    return IInteractable.Result.MoveFocusNext;
                case KeyType.ReverseTab:
                    return IInteractable.Result.MoveFocusPrevious;
                case KeyType.MouseEvent:
                    if (IsMouseMove(keyStroke))
                    {
                        return IInteractable.Result.Unhandled;
                    }
                    BasePane?.SetFocusedInteractable(this);
                    return IInteractable.Result.Handled;
            }
        }
        return IInteractable.Result.Unhandled;
    }

    public virtual TerminalPosition? CursorLocation => 
        ((IInteractableRenderer<T>)Renderer).GetCursorLocation(Self);

    public virtual IInputFilter? InputFilter => _inputFilter;

    public IInteractable SetInputFilter(IInputFilter? inputFilter)
    {
        lock (this)
        {
            _inputFilter = inputFilter;
            return this;
        }
    }

    public bool IsKeyboardActivationStroke(KeyStroke keyStroke)
    {
        bool isKeyboardActivation = (keyStroke.KeyType == KeyType.Character && keyStroke.Character == ' ') || 
                                   keyStroke.KeyType == KeyType.Enter;
        
        return IsFocused && isKeyboardActivation;
    }

    public bool IsMouseActivationStroke(KeyStroke keyStroke)
    {
        bool isMouseActivation = false;
        if (keyStroke is MouseAction action)
        {
            isMouseActivation = action.ActionType == MouseActionType.ClickDown;
        }
        
        return isMouseActivation;
    }

    public bool IsActivationStroke(KeyStroke keyStroke)
    {
        bool isKeyboardActivationStroke = IsKeyboardActivationStroke(keyStroke);
        bool isMouseActivationStroke = IsMouseActivationStroke(keyStroke);
        
        return isKeyboardActivationStroke || isMouseActivationStroke;
    }

    public bool IsMouseDown(KeyStroke keyStroke)
    {
        return keyStroke.KeyType == KeyType.MouseEvent && ((MouseAction)keyStroke).IsMouseDown;
    }

    public bool IsMouseDrag(KeyStroke keyStroke)
    {
        return keyStroke.KeyType == KeyType.MouseEvent && ((MouseAction)keyStroke).IsMouseDrag;
    }

    public bool IsMouseMove(KeyStroke keyStroke)
    {
        return keyStroke.KeyType == KeyType.MouseEvent && ((MouseAction)keyStroke).IsMouseMove;
    }

    public bool IsMouseUp(KeyStroke keyStroke)
    {
        return keyStroke.KeyType == KeyType.MouseEvent && ((MouseAction)keyStroke).IsMouseUp;
    }
}

public interface IInteractableRenderer<in T> : IComponentRenderer<T> where T : IInteractable
{
    TerminalPosition? GetCursorLocation(T component);
}