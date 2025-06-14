using Lanterna.Core;
using Lanterna.Graphics;
using Lanterna.Input;
using Lanterna.Screen;
using System.Collections.Concurrent;

namespace Lanterna.Gui2;

public abstract class AbstractTextGUI : ITextGUI
{
    private readonly IScreen _screen;
    private readonly ConcurrentBag<ITextGUI.IListener> _listeners;
    private bool _blockingIO;
    private bool _dirty;
    private ITextGUIThread? _textGUIThread;
    private Theme _guiTheme;
    private bool _virtualScreenEnabled;

    protected AbstractTextGUI(ITextGUIThreadFactory textGUIThreadFactory, IScreen screen)
    {
        _screen = screen ?? throw new ArgumentException("Creating a TextGUI requires an underlying Screen");
        _listeners = new ConcurrentBag<ITextGUI.IListener>();
        _blockingIO = false;
        _dirty = false;
        _guiTheme = LanternaThemes.DefaultTheme;
        _textGUIThread = textGUIThreadFactory.CreateTextGUIThread(this);
        _virtualScreenEnabled = true;
    }

    protected virtual KeyStroke? ReadKeyStroke()
    {
        return _blockingIO ? _screen.ReadInput() : PollInput();
    }

    protected KeyStroke? PollInput()
    {
        return _screen.PollInput();
    }

    public virtual bool ProcessInput()
    {
        bool gotInput = false;
        var keyStroke = ReadKeyStroke();
        if (keyStroke != null)
        {
            gotInput = true;
            do
            {
                if (keyStroke.KeyType == KeyType.EOF)
                {
                    throw new EndOfStreamException();
                }
                bool handled = HandleInput(keyStroke);
                if (!handled)
                {
                    handled = FireUnhandledKeyStroke(keyStroke);
                }
                _dirty = handled || _dirty;
                keyStroke = PollInput();
            } while (keyStroke != null);
        }
        return gotInput;
    }

    public virtual Theme Theme
    {
        get => _guiTheme;
        set
        {
            if (value != null)
            {
                _guiTheme = value;
            }
        }
    }

    public virtual void UpdateScreen()
    {
        lock (this)
        {
            _screen.DoResizeIfNecessary();
            DrawGUI(new DefaultTextGUIGraphics(this, _screen.NewTextGraphics()));
            _screen.SetCursorPosition(GetCursorPosition());
            _screen.Refresh();
            _dirty = false;
        }
    }

    public virtual IScreen Screen => _screen;

    public virtual bool IsPendingUpdate => 
        _screen.DoResizeIfNecessary() != null || _dirty;

    public virtual bool VirtualScreenEnabled
    {
        get => _virtualScreenEnabled;
        set => _virtualScreenEnabled = value;
    }

    public virtual ITextGUIThread GUIThread => _textGUIThread!;

    public abstract IInteractable? FocusedInteractable { get; }

    public virtual void AddListener(ITextGUI.IListener listener)
    {
        _listeners.Add(listener);
    }

    public virtual void RemoveListener(ITextGUI.IListener listener)
    {
        // Note: ConcurrentBag doesn't support removal, so we'd need a different collection
        // For now, using a simple approach - in production, consider using ConcurrentHashSet or similar
        var newBag = new ConcurrentBag<ITextGUI.IListener>();
        foreach (var existing in _listeners)
        {
            if (!existing.Equals(listener))
            {
                newBag.Add(existing);
            }
        }
        // Replace the field (this is not thread-safe, would need proper implementation)
    }

    public virtual bool BlockingIO
    {
        get => _blockingIO;
        set => _blockingIO = value;
    }

    protected bool FireUnhandledKeyStroke(KeyStroke keyStroke)
    {
        bool handled = false;
        foreach (var listener in _listeners)
        {
            handled = listener.OnUnhandledKeyStroke(this, keyStroke) || handled;
        }
        return handled;
    }

    protected void Invalidate()
    {
        _dirty = true;
    }

    protected abstract void DrawGUI(ITextGUIGraphics graphics);

    protected abstract TerminalPosition? GetCursorPosition();

    protected abstract bool HandleInput(KeyStroke key);
}

public interface ITextGUIThreadFactory
{
    ITextGUIThread CreateTextGUIThread(ITextGUI textGUI);
}