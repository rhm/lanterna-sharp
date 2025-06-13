using Lanterna.Core;
using Lanterna.Graphics;
using Lanterna.Input;

namespace Lanterna.Gui2;

public abstract class AbstractBasePane<T> : IBasePane where T : IBasePane
{
    private IComponent? _component;
    private IInteractable? _focusedInteractable;
    private bool _strictFocusChange;
    private bool _enableDirectionBasedMovements = true;
    private Theme? _themeOverride;

    public abstract ITextGUI? TextGUI { get; }

    public virtual IComponent? Component
    {
        get => _component;
        set
        {
            if (_component != null)
            {
                _component.OnRemoved(new ComponentContainer(this));
            }
            _component = value;
            if (_component != null)
            {
                _component.OnAdded(new ComponentContainer(this));
            }
            Invalidate();
        }
    }

    public virtual void Draw(ITextGUIGraphics graphics)
    {
        if (_component != null && _component.Visible)
        {
            _component.Size = graphics.Size;
            _component.Draw(graphics);
        }
    }

    public virtual bool IsInvalid => _component?.IsInvalid ?? false;

    public virtual void Invalidate()
    {
        _component?.Invalidate();
    }

    public virtual bool HandleInput(KeyStroke key)
    {
        if (_focusedInteractable != null)
        {
            var result = _focusedInteractable.HandleInput(key);
            switch (result)
            {
                case IInteractable.Result.Handled:
                    return true;
                case IInteractable.Result.MoveFocusNext:
                    return CycleFocus(IInteractable.FocusChangeDirection.Next);
                case IInteractable.Result.MoveFocusPrevious:
                    return CycleFocus(IInteractable.FocusChangeDirection.Previous);
                case IInteractable.Result.MoveFocusDown:
                    return CycleFocus(IInteractable.FocusChangeDirection.Down);
                case IInteractable.Result.MoveFocusUp:
                    return CycleFocus(IInteractable.FocusChangeDirection.Up);
                case IInteractable.Result.MoveFocusLeft:
                    return CycleFocus(IInteractable.FocusChangeDirection.Left);
                case IInteractable.Result.MoveFocusRight:
                    return CycleFocus(IInteractable.FocusChangeDirection.Right);
            }
        }
        return false;
    }

    public virtual IInteractable? FocusedInteractable => _focusedInteractable;

    public virtual void SetFocusedInteractable(IInteractable? interactable)
    {
        SetFocusedInteractable(interactable, IInteractable.FocusChangeDirection.Teleport);
    }

    protected virtual void SetFocusedInteractable(IInteractable? interactable, IInteractable.FocusChangeDirection direction)
    {
        if (_focusedInteractable == interactable)
            return;

        var previousFocus = _focusedInteractable;
        _focusedInteractable = interactable;

        previousFocus?.OnLeaveFocus(direction, interactable);
        interactable?.OnEnterFocus(direction, previousFocus);
    }

    public virtual bool CycleFocus(IInteractable.FocusChangeDirection direction)
    {
        if (_component is IContainer container)
        {
            var next = direction == IInteractable.FocusChangeDirection.Next ||
                      direction == IInteractable.FocusChangeDirection.Down ||
                      direction == IInteractable.FocusChangeDirection.Right
                ? container.NextFocus(_focusedInteractable)
                : container.PreviousFocus(_focusedInteractable);
            
            if (next != null)
            {
                SetFocusedInteractable(next, direction);
                return true;
            }
        }
        return false;
    }

    public virtual TerminalPosition? CursorPosition =>
        _focusedInteractable?.CursorLocation;

    public virtual TerminalPosition? ToGlobal(TerminalPosition localPosition)
    {
        return localPosition; // Base implementation - override in subclasses
    }

    public virtual TerminalPosition? FromGlobal(TerminalPosition position)
    {
        return position; // Base implementation - override in subclasses
    }

    public virtual void SetStrictFocusChange(bool strictFocusChange)
    {
        _strictFocusChange = strictFocusChange;
    }

    public virtual void SetEnableDirectionBasedMovements(bool enableDirectionBasedMovements)
    {
        _enableDirectionBasedMovements = enableDirectionBasedMovements;
    }

    public virtual Theme Theme
    {
        get
        {
            if (_themeOverride != null)
            {
                return _themeOverride;
            }
            else if (TextGUI != null)
            {
                return TextGUI.Theme;
            }
            else
            {
                return LanternaThemes.DefaultTheme;
            }
        }
    }

    public virtual void SetTheme(Theme? theme)
    {
        _themeOverride = theme;
        Invalidate();
    }

    public virtual void SetMenuBar(IMenuBar? menuBar)
    {
        MenuBar = menuBar;
    }

    public virtual IMenuBar? MenuBar { get; protected set; }

    // Helper class to allow the component to think it has a container parent
    private class ComponentContainer : IContainer
    {
        private readonly AbstractBasePane<T> _basePane;

        public ComponentContainer(AbstractBasePane<T> basePane)
        {
            _basePane = basePane;
        }

        public int ChildCount => _basePane._component != null ? 1 : 0;
        public IReadOnlyCollection<IComponent> Children => 
            _basePane._component != null ? new[] { _basePane._component } : Array.Empty<IComponent>();
        public IReadOnlyList<IComponent> ChildrenList => 
            _basePane._component != null ? new[] { _basePane._component } : Array.Empty<IComponent>();

        public bool ContainsComponent(IComponent component) => _basePane._component == component;
        public bool RemoveComponent(IComponent component)
        {
            if (_basePane._component == component)
            {
                _basePane.Component = null;
                return true;
            }
            return false;
        }

        public IInteractable? NextFocus(IInteractable? fromThis)
        {
            if (_basePane._component is IInteractable interactable && interactable.Enabled && interactable.IsFocusable)
                return interactable;
            if (_basePane._component is IContainer container)
                return container.NextFocus(fromThis);
            return null;
        }

        public IInteractable? PreviousFocus(IInteractable? fromThis)
        {
            if (_basePane._component is IInteractable interactable && interactable.Enabled && interactable.IsFocusable)
                return interactable;
            if (_basePane._component is IContainer container)
                return container.PreviousFocus(fromThis);
            return null;
        }

        public bool HandleInput(KeyStroke key) => _basePane.HandleInput(key);

        public void UpdateLookupMap(IInteractableLookupMap interactableLookupMap)
        {
            if (_basePane._component is IContainer container)
                container.UpdateLookupMap(interactableLookupMap);
            else if (_basePane._component is IInteractable interactable && interactable.Enabled && interactable.IsFocusable)
            {
                var globalPos = _basePane.ToGlobal(TerminalPosition.TopLeftCorner);
                if (globalPos.HasValue)
                    interactableLookupMap.Add(interactable, globalPos.Value);
            }
        }

        // All the IComponent methods that a container needs but BasePane doesn't implement
        public TerminalPosition Position { get; set; } = TerminalPosition.TopLeftCorner;
        public TerminalPosition GlobalPosition => Position;
        public TerminalSize Size { get; set; } = TerminalSize.Zero;
        public TerminalSize PreferredSize => Size;
        public IComponent SetPreferredSize(TerminalSize? explicitPreferredSize) => this;
        public IComponent SetLayoutData(ILayoutData? data) => this;
        public ILayoutData? LayoutData => null;
        public bool Visible { get; set; } = true;
        public IContainer? Parent => null;
        public bool HasParent(IContainer parent) => false;
        public ITextGUI? TextGUI => _basePane.TextGUI;
        public Theme Theme => _basePane.Theme;
        public ThemeDefinition ThemeDefinition => Theme.GetDefinition(GetType());
        public IComponent SetTheme(Theme? theme) { _basePane.SetTheme(theme); return this; }
        public bool IsInside(IContainer container) => false;
        public IComponentRenderer<IComponent> Renderer => throw new NotImplementedException();
        public void Invalidate() => _basePane.Invalidate();
        public IBorder WithBorder(IBorder border) => throw new NotImplementedException();
        public TerminalPosition? ToBasePane(TerminalPosition position) => _basePane.ToGlobal(position);
        public TerminalPosition? ToGlobal(TerminalPosition position) => _basePane.ToGlobal(position);
        public IBasePane? BasePane => _basePane;
        public IComponent AddTo(IPanel panel) => throw new NotImplementedException();
        public void OnAdded(IContainer container) { }
        public void OnRemoved(IContainer container) { }
        public void Draw(ITextGUIGraphics graphics) => _basePane.Draw(graphics);
        public bool IsInvalid => _basePane.IsInvalid;
    }
}