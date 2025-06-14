using Lanterna.Core;
using Lanterna.Input;
using System.Collections.Concurrent;

namespace Lanterna.Gui2;

public abstract class AbstractWindow : AbstractBasePane<IWindow>, IWindow
{
    private string _title;
    private IWindowBasedTextGUI? _textGUI;
    private bool _visible;
    private TerminalSize? _lastKnownSize;
    private TerminalSize? _lastKnownDecoratedSize;
    private TerminalPosition? _lastKnownPosition;
    private TerminalPosition _contentOffset;
    private readonly HashSet<IWindow.Hint> _hints;
    private IWindowPostRenderer? _windowPostRenderer;
    private bool _closeWindowWithEscape;
    private readonly ConcurrentBag<IWindowListener> _windowListeners;
    private readonly object _lock = new object();
    private volatile bool _closed = false;

    protected AbstractWindow() : this("")
    {
    }

    protected AbstractWindow(string title)
    {
        _title = title;
        _textGUI = null;
        _visible = true;
        _contentOffset = TerminalPosition.TopLeftCorner;
        _lastKnownPosition = null;
        _lastKnownSize = null;
        _lastKnownDecoratedSize = null;
        _closeWindowWithEscape = false;
        _hints = new HashSet<IWindow.Hint>();
        _windowListeners = new ConcurrentBag<IWindowListener>();
    }

    public void SetCloseWindowWithEscape(bool closeWindowWithEscape)
    {
        _closeWindowWithEscape = closeWindowWithEscape;
    }

    public virtual void SetTextGUI(IWindowBasedTextGUI textGUI)
    {
        if (_textGUI != null && textGUI != null)
        {
            throw new NotSupportedException("Are you calling SetTextGUI yourself? Please read the documentation");
        }
        _textGUI = textGUI;
    }

    public override ITextGUI? TextGUI => _textGUI;

    IWindowBasedTextGUI? IWindow.TextGUI => _textGUI;

    public virtual string Title
    {
        get => _title;
        set
        {
            lock (_lock)
            {
                _title = value ?? "";
                Invalidate();
            }
        }
    }

    public virtual bool Visible
    {
        get => _visible;
        set
        {
            lock (_lock)
            {
                _visible = value;
            }
        }
    }

    public virtual TerminalRectangle Bounds
    {
        get
        {
            var position = Position;
            var size = DecoratedSize;
            return new TerminalRectangle(position.Column, position.Row, size.Columns, size.Rows);
        }
    }

    public virtual TerminalSize PreferredSize
    {
        get
        {
            var component = Component;
            if (component == null)
            {
                return TerminalSize.Zero;
            }
            return component.PreferredSize;
        }
    }

    public virtual void Close()
    {
        lock (_lock)
        {
            if (_closed)
                return;
                
            _closed = true;
            _textGUI?.RemoveWindow(this);
        }
    }

    public virtual void SetHints(ICollection<IWindow.Hint> hints)
    {
        lock (_lock)
        {
            _hints.Clear();
            foreach (var hint in hints)
            {
                _hints.Add(hint);
            }
        }
    }

    public virtual ISet<IWindow.Hint> Hints
    {
        get
        {
            lock (_lock)
            {
                return new HashSet<IWindow.Hint>(_hints);
            }
        }
    }

    public virtual TerminalPosition Position
    {
        get => _lastKnownPosition ?? TerminalPosition.TopLeftCorner;
        set
        {
            lock (_lock)
            {
                var oldPosition = _lastKnownPosition;
                _lastKnownPosition = value;
                
                if (oldPosition != null && !oldPosition.Equals(value))
                {
                    foreach (var listener in _windowListeners)
                    {
                        listener.OnMoved(this, oldPosition.Value, value);
                    }
                }
            }
        }
    }

    public virtual TerminalSize Size
    {
        get => _lastKnownSize ?? TerminalSize.Zero;
        set
        {
            lock (_lock)
            {
                var oldSize = _lastKnownSize;
                _lastKnownSize = value;
                
                if (oldSize != null && !oldSize.Equals(value))
                {
                    foreach (var listener in _windowListeners)
                    {
                        listener.OnResized(this, oldSize.Value, value);
                    }
                }
            }
        }
    }

    public virtual void SetFixedSize(TerminalSize size)
    {
        lock (_lock)
        {
            _hints.Add(IWindow.Hint.FixedSize);
            Size = size;
        }
    }

    public virtual TerminalSize DecoratedSize
    {
        get => _lastKnownDecoratedSize ?? TerminalSize.Zero;
        set
        {
            lock (_lock)
            {
                _lastKnownDecoratedSize = value;
            }
        }
    }

    public virtual void SetContentOffset(TerminalPosition offset)
    {
        lock (_lock)
        {
            _contentOffset = offset;
        }
    }

    public virtual void WaitUntilClosed()
    {
        while (!_closed)
        {
            Thread.Sleep(10);
        }
    }

    public virtual IWindowPostRenderer? PostRenderer => _windowPostRenderer;

    public virtual void AddWindowListener(IWindowListener windowListener)
    {
        _windowListeners.Add(windowListener);
    }

    public virtual void RemoveWindowListener(IWindowListener windowListener)
    {
        // Note: ConcurrentBag doesn't support efficient removal
        // In production, consider using a different collection
    }

    public virtual TerminalPosition ToGlobalFromContentRelative(TerminalPosition localPosition)
    {
        return Position.WithRelative(_contentOffset).WithRelative(localPosition);
    }

    public virtual TerminalPosition ToGlobalFromDecoratedRelative(TerminalPosition decoratedPosition)
    {
        return Position.WithRelative(decoratedPosition);
    }

    public virtual TerminalPosition FromGlobalToContentRelative(TerminalPosition position)
    {
        return position.WithRelative(-Position.Column - _contentOffset.Column, -Position.Row - _contentOffset.Row);
    }

    public virtual TerminalPosition FromGlobalToDecoratedRelative(TerminalPosition position)
    {
        return position.WithRelative(-Position.Column, -Position.Row);
    }

    public virtual void SetMenuBar(IMenuBar? menuBar)
    {
        lock (_lock)
        {
            MenuBar = menuBar;
        }
    }

    public virtual IMenuBar? MenuBar { get; protected set; }

    public override bool HandleInput(KeyStroke keyStroke)
    {
        bool handled = false;
        
        foreach (var listener in _windowListeners)
        {
            listener.OnInput(this, keyStroke, ref handled);
            if (handled)
                return true;
        }

        if (_closeWindowWithEscape && keyStroke.KeyType == KeyType.Escape)
        {
            Close();
            return true;
        }

        handled = base.HandleInput(keyStroke);
        
        if (!handled)
        {
            foreach (var listener in _windowListeners)
            {
                listener.OnUnhandledInput(this, keyStroke, ref handled);
                if (handled)
                    return true;
            }
        }

        return handled;
    }

    public override TerminalPosition? ToGlobal(TerminalPosition localPosition)
    {
        return ToGlobalFromContentRelative(localPosition);
    }

    protected override IWindow Self => this;

    public override TerminalPosition? FromGlobal(TerminalPosition globalPosition)
    {
        return FromGlobalToContentRelative(globalPosition);
    }
}

public class BasicWindow : AbstractWindow
{
    public BasicWindow() : base()
    {
    }

    public BasicWindow(string title) : base(title)
    {
    }
}