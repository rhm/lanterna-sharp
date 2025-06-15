using Lanterna.Core;
using Lanterna.Graphics;

namespace Lanterna.Gui2;

public abstract class AbstractComponent<T> : IComponent where T : class, IComponent
{
    private IComponentRenderer<T>? _overrideRenderer;
    private IComponentRenderer<T>? _themeRenderer;
    private Theme? _themeRenderersTheme;
    private IComponentRenderer<T>? _defaultRenderer;
    
    private IContainer? _parent;
    private TerminalSize _size;
    private TerminalSize? _explicitPreferredSize;
    private TerminalPosition _position;
    private Theme? _themeOverride;
    private ILayoutData? _layoutData;
    private bool _visible;
    private bool _invalid;

    protected AbstractComponent()
    {
        _size = TerminalSize.Zero;
        _position = TerminalPosition.TopLeftCorner;
        _explicitPreferredSize = null;
        _layoutData = null;
        _visible = true;
        _invalid = true;
        _parent = null;
        _overrideRenderer = null;
        _themeRenderer = null;
        _themeRenderersTheme = null;
        _defaultRenderer = null;
    }

    protected abstract IComponentRenderer<T> CreateDefaultRenderer();

    protected void RunOnGUIThreadIfExistsOtherwiseRunDirect(Action action)
    {
        if (TextGUI?.GUIThread != null)
        {
            TextGUI.GUIThread.InvokeLater(action);
        }
        else
        {
            action();
        }
    }

    public T SetRenderer(IComponentRenderer<T>? renderer)
    {
        _overrideRenderer = renderer;
        return Self;
    }

    public virtual IComponentRenderer<IComponent> Renderer 
    { 
        get
        {
            lock (this)
            {
                if (_overrideRenderer != null)
                {
                    return (IComponentRenderer<IComponent>)_overrideRenderer;
                }

                var currentTheme = Theme;
                if ((_themeRenderer == null && BasePane != null) ||
                    (_themeRenderer != null && currentTheme != _themeRenderersTheme))
                {
                    _themeRenderer = currentTheme.GetDefinition(GetType()).GetRenderer<T>() as IComponentRenderer<T>;
                    if (_themeRenderer != null)
                    {
                        _themeRenderersTheme = currentTheme;
                    }
                }
                if (_themeRenderer != null)
                {
                    return (IComponentRenderer<IComponent>)_themeRenderer;
                }

                if (_defaultRenderer == null)
                {
                    _defaultRenderer = CreateDefaultRenderer();
                    if (_defaultRenderer == null)
                    {
                        throw new InvalidOperationException($"{GetType()} returned a null default renderer");
                    }
                }
                return (IComponentRenderer<IComponent>)_defaultRenderer;
            }
        }
    }

    public virtual void Invalidate()
    {
        _invalid = true;
    }

    public virtual TerminalSize Size
    {
        get => _size;
        set
        {
            lock (this)
            {
                _size = value;
            }
        }
    }

    public virtual TerminalSize PreferredSize
    {
        get
        {
            if (_explicitPreferredSize.HasValue)
            {
                return _explicitPreferredSize.Value;
            }
            else
            {
                return CalculatePreferredSize();
            }
        }
    }

    public IComponent SetPreferredSize(TerminalSize? explicitPreferredSize)
    {
        lock (this)
        {
            _explicitPreferredSize = explicitPreferredSize;
            return this;
        }
    }

    public virtual bool Visible
    {
        get => _visible;
        set
        {
            if (_visible != value)
            {
                _visible = value;
                if (value)
                {
                    Invalidate();
                }
                else
                {
                    Parent?.Invalidate();
                }
            }
        }
    }

    protected virtual TerminalSize CalculatePreferredSize()
    {
        lock (this)
        {
            return ((IComponentRenderer<T>)Renderer).GetPreferredSize(Self);
        }
    }

    public virtual TerminalPosition Position
    {
        get => _position;
        set
        {
            lock (this)
            {
                _position = value;
            }
        }
    }

    public virtual TerminalPosition GlobalPosition => 
        ToGlobal(TerminalPosition.TopLeftCorner) ?? TerminalPosition.TopLeftCorner;

    public virtual bool IsInvalid => _invalid;

    public virtual void Draw(ITextGUIGraphics graphics)
    {
        lock (this)
        {
            Size = graphics.Size;
            OnBeforeDrawing();
            ((IComponentRenderer<T>)Renderer).DrawComponent(graphics, Self);
            OnAfterDrawing(graphics);
            _invalid = false;
        }
    }

    protected virtual void OnBeforeDrawing()
    {
        // No operation by default
    }

    protected virtual void OnAfterDrawing(ITextGUIGraphics graphics)
    {
        // No operation by default
    }

    public virtual IComponent SetLayoutData(ILayoutData? data)
    {
        lock (this)
        {
            if (_layoutData != data)
            {
                _layoutData = data;
                Invalidate();
            }
            return this;
        }
    }

    public virtual ILayoutData? LayoutData 
    { 
        get => _layoutData;
        set 
        {
            lock (this)
            {
                if (_layoutData != value)
                {
                    _layoutData = value;
                    Invalidate();
                }
            }
        }
    }

    public virtual IContainer? Parent => _parent;

    public virtual bool HasParent(IContainer parent)
    {
        if (_parent == null)
        {
            return false;
        }
        
        var recursiveParent = _parent;
        while (recursiveParent != null)
        {
            if (recursiveParent == parent)
            {
                return true;
            }
            recursiveParent = recursiveParent.Parent;
        }
        return false;
    }

    public virtual ITextGUI? TextGUI => _parent?.TextGUI;

    public virtual Theme Theme
    {
        get
        {
            lock (this)
            {
                if (_themeOverride != null)
                {
                    return _themeOverride;
                }
                else if (_parent != null)
                {
                    return _parent.Theme;
                }
                else if (BasePane != null)
                {
                    return BasePane.Theme;
                }
                else
                {
                    return LanternaThemes.DefaultTheme;
                }
            }
        }
    }

    public virtual IThemeDefinition ThemeDefinition => Theme.GetDefinition(GetType());

    public IComponent SetTheme(Theme? theme)
    {
        lock (this)
        {
            _themeOverride = theme;
            Invalidate();
            return this;
        }
    }

    public virtual bool IsInside(IContainer container)
    {
        IComponent? test = this;
        while (test?.Parent != null)
        {
            if (test.Parent == container)
            {
                return true;
            }
            test = test.Parent;
        }
        return false;
    }

    public virtual IBasePane? BasePane => _parent?.BasePane;

    public virtual TerminalPosition? ToBasePane(TerminalPosition position)
    {
        var parent = Parent;
        if (parent == null)
        {
            return null;
        }
        return parent.ToBasePane(Position.WithRelative(position));
    }

    public virtual TerminalPosition? ToGlobal(TerminalPosition position)
    {
        var parent = Parent;
        if (parent == null)
        {
            return null;
        }
        return parent.ToGlobal(Position.WithRelative(position));
    }

    public virtual IBorder WithBorder(IBorder border)
    {
        lock (this)
        {
            border.Component = this;
            return border;
        }
    }

    public IComponent AddTo(IPanel panel)
    {
        lock (this)
        {
            panel.AddComponent(this);
            return this;
        }
    }

    public virtual void OnAdded(IContainer container)
    {
        lock (this)
        {
            if (_parent != container && _parent != null)
            {
                _parent.RemoveComponent(this);
            }
            _parent = container;
        }
    }

    public virtual void OnRemoved(IContainer container)
    {
        lock (this)
        {
            if (_parent == container)
            {
                _parent = null;
                _themeRenderer = null;
            }
            else
            {
                throw new InvalidOperationException($"{this} is not {container}'s child.");
            }
        }
    }

    protected T Self => (T)(object)this;
}

public static class LanternaThemes
{
    public static Theme DefaultTheme { get; set; } = new Theme(); // TODO: Implement proper default theme
}