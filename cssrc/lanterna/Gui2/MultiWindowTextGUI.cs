/*
 * This file is part of lanterna (https://github.com/mabe02/lanterna).
 *
 * lanterna is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * Copyright (C) 2010-2020 Martin Berglund
 */

using System.Collections.Concurrent;
using Lanterna.Core;
using Lanterna.Graphics;
using Lanterna.Input;
using Lanterna.Screen;

namespace Lanterna.Gui2;

/// <summary>
/// This is the main Text GUI implementation built into Lanterna, supporting multiple tiled windows and a dynamic
/// background area that can be fully customized. If you want to create a text-based GUI with windows and controls,
/// it's very likely this is what you want to use.
/// <para>
/// Note: This class used to always wrap the IScreen object with a VirtualScreen to ensure that the UI
/// always fits. As of 3.1.0, we don't do this anymore so when you create the MultiWindowTextGUI you can wrap
/// the screen parameter yourself if you want to keep this behavior.
/// </para>
/// </summary>
public class MultiWindowTextGUI : AbstractTextGUI, IWindowBasedTextGUI
{
    private readonly IWindowManager _windowManager;
    private readonly IBasePane _backgroundPane;
    private readonly WindowList _windowList;
    private readonly ConcurrentDictionary<IWindow, ITextImage> _windowRenderBufferCache;
    private readonly IWindowPostRenderer? _postRenderer;

    private bool _eofWhenNoWindows;

    private IWindow? _titleBarDragWindow;
    private TerminalPosition _originWindowPosition;
    private TerminalPosition _dragStart;

    /// <summary>
    /// Creates a new MultiWindowTextGUI that uses the specified IScreen as the backend for all drawing
    /// operations. The background area of the GUI will be a solid color, depending on theme (default is blue). The
    /// current thread will be used as the GUI thread for all Lanterna library operations.
    /// </summary>
    /// <param name="screen">Screen to use as the backend for drawing operations</param>
    public MultiWindowTextGUI(IScreen screen) : this(new SameTextGUIThread.Factory(), screen)
    {
    }

    /// <summary>
    /// Creates a new MultiWindowTextGUI that uses the specified IScreen as the backend for all drawing
    /// operations. The background area of the GUI will be a solid color, depending on theme (default is blue). This
    /// constructor allows you control the threading model for the UI.
    /// </summary>
    /// <param name="guiThreadFactory">Factory implementation to use when creating the ITextGUIThread</param>
    /// <param name="screen">Screen to use as the backend for drawing operations</param>
    public MultiWindowTextGUI(ITextGUIThreadFactory guiThreadFactory, IScreen screen) 
        : this(guiThreadFactory, screen, new DefaultWindowManager(), null, new GUIBackdrop())
    {
    }

    /// <summary>
    /// Creates a new MultiWindowTextGUI that uses the specified IScreen as the backend for all drawing
    /// operations. The background area of the GUI will be a solid color, depending on theme (default is blue). This
    /// constructor allows you control the threading model for the UI and set a custom IWindowManager.
    /// </summary>
    /// <param name="guiThreadFactory">Factory implementation to use when creating the ITextGUIThread</param>
    /// <param name="screen">Screen to use as the backend for drawing operations</param>
    /// <param name="windowManager">Custom window manager to use</param>
    public MultiWindowTextGUI(ITextGUIThreadFactory guiThreadFactory, IScreen screen, IWindowManager windowManager)
        : this(guiThreadFactory, screen, windowManager, null, new GUIBackdrop())
    {
    }

    /// <summary>
    /// Creates a new MultiWindowTextGUI that uses the specified IScreen as the backend for all drawing
    /// operations. The background area of the GUI will be the component supplied instead of the usual backdrop. This
    /// constructor allows you to set a custom IWindowManager instead of DefaultWindowManager.
    /// </summary>
    /// <param name="screen">Screen to use as the backend for drawing operations</param>
    /// <param name="windowManager">Window manager implementation to use</param>
    /// <param name="background">Component to use as the background of the GUI, behind all the windows</param>
    public MultiWindowTextGUI(IScreen screen, IWindowManager windowManager, IComponent background)
        : this(screen, windowManager, null, background)
    {
    }

    /// <summary>
    /// Creates a new MultiWindowTextGUI that uses the specified IScreen as the backend for all drawing
    /// operations. The background area of the GUI will be the component supplied instead of the usual backdrop. This
    /// constructor allows you to set a custom IWindowManager instead of DefaultWindowManager as well
    /// as a custom IWindowPostRenderer that can be used to tweak the appearance of any window.
    /// </summary>
    /// <param name="screen">Screen to use as the backend for drawing operations</param>
    /// <param name="windowManager">Window manager implementation to use</param>
    /// <param name="postRenderer">IWindowPostRenderer object to invoke after each window has been drawn</param>
    /// <param name="background">Component to use as the background of the GUI, behind all the windows</param>
    public MultiWindowTextGUI(IScreen screen, IWindowManager windowManager, IWindowPostRenderer? postRenderer, IComponent background)
        : this(new SameTextGUIThread.Factory(), screen, windowManager, postRenderer, background)
    {
    }

    /// <summary>
    /// Creates a new MultiWindowTextGUI that uses the specified IScreen as the backend for all drawing
    /// operations. The background area of the GUI will be the component supplied instead of the usual backdrop. This
    /// constructor allows you to set a custom IWindowManager instead of DefaultWindowManager as well
    /// as a custom IWindowPostRenderer that can be used to tweak the appearance of any window. This constructor
    /// also allows you to control the threading model for the UI.
    /// </summary>
    /// <param name="guiThreadFactory">Factory implementation to use when creating the ITextGUIThread</param>
    /// <param name="screen">Screen to use as the backend for drawing operations</param>
    /// <param name="windowManager">Window manager implementation to use</param>
    /// <param name="postRenderer">IWindowPostRenderer object to invoke after each window has been drawn</param>
    /// <param name="background">Component to use as the background of the GUI, behind all the windows</param>
    public MultiWindowTextGUI(ITextGUIThreadFactory guiThreadFactory, IScreen screen, IWindowManager windowManager, 
        IWindowPostRenderer? postRenderer, IComponent? background) : base(guiThreadFactory, screen)
    {
        _windowList = new WindowList();
        if (windowManager == null)
        {
            throw new ArgumentException("Creating a window-based TextGUI requires a WindowManager");
        }
        if (background == null)
        {
            // Use a sensible default instead of throwing
            background = new GUIBackdrop();
        }
        
        _windowManager = windowManager;
        _backgroundPane = new MultiWindowTextGUIBackgroundPane(this);
        _backgroundPane.Component = background;
        _windowRenderBufferCache = new ConcurrentDictionary<IWindow, ITextImage>();
        _postRenderer = postRenderer;
        _eofWhenNoWindows = false;
    }

    public override bool IsPendingUpdate
    {
        get
        {
            lock (this)
            {
                foreach (var window in Windows)
                {
                    if (window.Visible && window.IsInvalid)
                    {
                        return true;
                    }
                }
                return base.IsPendingUpdate || _backgroundPane.IsInvalid || _windowManager.IsInvalid;
            }
        }
    }

    public override void UpdateScreen()
    {
        lock (this)
        {
            // Virtual screen support will be added later when IVirtualScreen is implemented
            base.UpdateScreen();
        }
    }

    protected override KeyStroke? ReadKeyStroke()
    {
        var keyStroke = PollInput();
        if (_windowList.IsHadWindowAtSomePoint() && _eofWhenNoWindows && keyStroke == null && !Windows.Any())
        {
            return new KeyStroke(KeyType.EOF);
        }
        else if (keyStroke != null)
        {
            return keyStroke;
        }
        else
        {
            return base.ReadKeyStroke();
        }
    }

    protected override void DrawGUI(ITextGUIGraphics graphics)
    {
        lock (this)
        {
            DrawBackgroundPane(graphics);
            _windowManager.PrepareWindows(this, _windowList.GetWindowsInStableOrder(), graphics.Size);
            
            foreach (var window in Windows)
            {
                if (window.Visible)
                {
                    // Simplified implementation - draw windows directly without buffering
                    // TODO: Add window buffering when ITextImage infrastructure is implemented
                    var windowGraphics = graphics.NewTextGraphics(window.Position, window.DecoratedSize);
                    var insideWindowDecorationsGraphics = windowGraphics;
                    var contentOffset = TerminalPosition.TopLeftCorner;
                    
                    if (!window.Hints.Contains(IWindow.Hint.NoDecorations))
                    {
                        var decorationRenderer = _windowManager.GetWindowDecorationRenderer(window);
                        insideWindowDecorationsGraphics = decorationRenderer.Draw(this, windowGraphics, window);
                        contentOffset = decorationRenderer.GetOffset(window);
                    }

                    window.Draw(insideWindowDecorationsGraphics);
                    window.SetContentOffset(contentOffset);
                    
                    if (windowGraphics != insideWindowDecorationsGraphics)
                    {
                        Borders.JoinLinesWithFrame(windowGraphics);
                    }

                    if (!window.Hints.Contains(IWindow.Hint.NoPostRendering))
                    {
                        if (window.PostRenderer != null)
                        {
                            window.PostRenderer.PostRender(graphics, this, window);
                        }
                        else if (_postRenderer != null)
                        {
                            _postRenderer.PostRender(graphics, this, window);
                        }
                        else if (Theme.WindowPostRenderer != null)
                        {
                            Theme.WindowPostRenderer.PostRender(graphics, this, window);
                        }
                    }
                }
            }

            // TODO: Implement window render buffer caching when ITextImage is available
        }
    }

    private void DrawBackgroundPane(ITextGUIGraphics graphics)
    {
        _backgroundPane.Draw(new DefaultTextGUIGraphics(this, graphics));
    }

    protected override TerminalPosition? GetCursorPosition()
    {
        lock (this)
        {
            var activeWindow = ActiveWindow;
            if (activeWindow != null)
            {
                var cursorPos = activeWindow.CursorPosition;
                if (cursorPos.HasValue)
                {
                    return activeWindow.ToGlobal(cursorPos.Value);
                }
                return null;
            }
            else
            {
                return _backgroundPane.CursorPosition;
            }
        }
    }

    /// <summary>
    /// Sets whether the TextGUI should return EOF when you try to read input while there are no windows in the window
    /// manager. Setting this to true (off by default) will make the GUI automatically exit when the last window has been
    /// closed.
    /// </summary>
    /// <param name="eofWhenNoWindows">Should the GUI return EOF when there are no windows left</param>
    public void SetEOFWhenNoWindows(bool eofWhenNoWindows)
    {
        _eofWhenNoWindows = eofWhenNoWindows;
    }

    /// <summary>
    /// Returns whether the TextGUI should return EOF when you try to read input while there are no windows in the window
    /// manager. When this is true (false by default) will make the GUI automatically exit when the last window has been
    /// closed.
    /// </summary>
    /// <returns>Should the GUI return EOF when there are no windows left</returns>
    public bool IsEOFWhenNoWindows => _eofWhenNoWindows;

    public override IInteractable? FocusedInteractable
    {
        get
        {
            lock (this)
            {
                var activeWindow = ActiveWindow;
                if (activeWindow != null)
                {
                    return activeWindow.FocusedInteractable;
                }
                else
                {
                    return _backgroundPane.FocusedInteractable;
                }
            }
        }
    }

    protected override bool HandleInput(KeyStroke keyStroke)
    {
        lock (this)
        {
            HandleMouseEvents(keyStroke);
            var activeWindow = ActiveWindow;
            if (activeWindow != null)
            {
                return activeWindow.HandleInput(keyStroke);
            }
            else
            {
                return _backgroundPane.HandleInput(keyStroke);
            }
        }
    }

    private void HandleMouseEvents(KeyStroke keyStroke)
    {
        if (keyStroke is MouseAction mouseAction)
        {
            HandleMouseDownPossiblyChangeActiveWindow(mouseAction);
            HandleMouseDownPossiblyStartTitleDrag(mouseAction);
            HandleMouseDragPossiblyMoveWindow(mouseAction);
        }
    }

    private void HandleMouseDownPossiblyChangeActiveWindow(MouseAction mouse)
    {
        if (mouse.IsMouseDown)
        {
            // For now, active windows do not overlap?
            // By happenstance, the last in the list in case of many overlapping will be active
            var priorActiveWindow = ActiveWindow;
            bool anyHit = false;
            var snapshot = Windows.ToList();
            
            foreach (var window in snapshot)
            {
                if (window.Bounds.Contains(mouse.Position))
                {
                    SetActiveWindow(window);
                    anyHit = true;
                }
            }
            
            // Clear popup menus if they clicked onto another window or missed all windows
            if (priorActiveWindow != null && (priorActiveWindow != ActiveWindow || !anyHit))
            {
                if (priorActiveWindow.Hints.Contains(IWindow.Hint.MenuPopup))
                {
                    priorActiveWindow.Close();
                }
            }
        }
    }

    private void HandleMouseDownPossiblyStartTitleDrag(MouseAction mouse)
    {
        if (mouse.IsMouseDown)
        {
            _titleBarDragWindow = null;
            var window = ActiveWindow;
            if (window == null)
            {
                return;
            }

            if (window.Hints.Contains(IWindow.Hint.MenuPopup))
            {
                // Popup windows are not draggable
                return;
            }

            var decorator = _windowManager.GetWindowDecorationRenderer(window);
            var titleBarRectangle = decorator.GetTitleBarRectangle(window);
            var local = window.FromGlobalToDecoratedRelative(mouse.Position);
            
            if (titleBarRectangle.Contains(local))
            {
                _titleBarDragWindow = window;
                _originWindowPosition = _titleBarDragWindow.Position;
                _dragStart = mouse.Position;
                MoveToTop(window);
            }
        }
    }

    private void HandleMouseDragPossiblyMoveWindow(MouseAction mouse)
    {
        if (_titleBarDragWindow == null)
        {
            return;
        }
        
        if (mouse.IsMouseDrag)
        {
            var mp = mouse.Position;
            var wp = _originWindowPosition;
            int dx = mp.Column - _dragStart.Column;
            int dy = mp.Row - _dragStart.Row;
            ChangeWindowHintsForDragged(_titleBarDragWindow);
            _titleBarDragWindow.Position = new TerminalPosition(wp.Column + dx, wp.Row + dy);
            // TODO: any additional children popups (shown menus, etc) should also be moved (or just closed)
        }
    }

    /// <summary>
    /// In order for window to be draggable, it would no longer be CENTERED.    
    /// Removes Hint.CENTERED, adds Hint.FIXED_POSITION to the window hints.
    /// </summary>
    private void ChangeWindowHintsForDragged(IWindow window)
    {
        var hints = new HashSet<IWindow.Hint>(window.Hints);
        hints.Remove(IWindow.Hint.Centered);
        hints.Add(IWindow.Hint.FixedPosition);
        window.SetHints(hints);
    }

    // IWindowBasedTextGUI implementation
    public IWindowManager WindowManager => _windowManager;

    public IWindowBasedTextGUI AddWindow(IWindow window)
    {
        lock (this)
        {
            // To protect against NPE if the user forgot to set a content component
            if (window.Component == null)
            {
                window.Component = new EmptySpace(TerminalSize.One);
            }

            if (window.TextGUI != null)
            {
                window.TextGUI.RemoveWindow(window);
            }
            
            window.SetTextGUI(this);
            _windowManager.OnAdded(this, window, _windowList.GetWindowsInStableOrder());
            _windowList.AddWindow(window);
            Invalidate();
            return this;
        }
    }

    public IWindowBasedTextGUI AddWindowAndWait(IWindow window)
    {
        AddWindow(window);
        window.WaitUntilClosed();
        return this;
    }

    public IWindowBasedTextGUI RemoveWindow(IWindow window)
    {
        lock (this)
        {
            bool contained = _windowList.RemoveWindow(window);
            if (!contained)
            {
                // Didn't contain this window
                return this;
            }
            
            window.SetTextGUI(null);
            _windowManager.OnRemoved(this, window, _windowList.GetWindowsInStableOrder());
            Invalidate();
            return this;
        }
    }

    public void WaitForWindowToClose(IWindow window)
    {
        while (window.TextGUI != null)
        {
            bool sleep = true;
            var guiThread = GUIThread;
            if (System.Threading.Thread.CurrentThread == guiThread.Thread)
            {
                try
                {
                    sleep = !guiThread.ProcessEventsAndUpdate();
                }
                catch (EndOfStreamException)
                {
                    // The GUI has closed so allow exit
                    break;
                }
                catch (IOException e)
                {
                    throw new InvalidOperationException("Unexpected IOException while waiting for window to close", e);
                }
            }
            if (sleep)
            {
                try
                {
                    System.Threading.Thread.Sleep(1);
                }
                catch (ThreadInterruptedException)
                {
                    // Ignored
                }
            }
        }
    }

    public IReadOnlyList<IWindow> Windows
    {
        get
        {
            lock (this)
            {
                return _windowList.GetWindowsInZOrder();
            }
        }
    }

    public IWindowBasedTextGUI SetActiveWindow(IWindow? activeWindow)
    {
        lock (this)
        {
            _windowList.SetActiveWindow(activeWindow);
            return this;
        }
    }

    public IWindow? ActiveWindow
    {
        get
        {
            lock (this)
            {
                return _windowList.GetActiveWindow();
            }
        }
    }

    public IBasePane BackgroundPane => _backgroundPane;

    public IWindowPostRenderer? WindowPostRenderer => _postRenderer;

    public IWindowBasedTextGUI MoveToTop(IWindow window)
    {
        lock (this)
        {
            _windowList.MoveToTop(window);
            Invalidate();
            return this;
        }
    }

    public MultiWindowTextGUI MoveToBottom(IWindow window)
    {
        lock (this)
        {
            _windowList.MoveToBottom(window);
            Invalidate();
            return this;
        }
    }

    /// <summary>
    /// Switches the active window by cyclically shuffling the window list. If reverse parameter is false
    /// then the current top window is placed at the bottom of the stack and the window immediately behind it is the new
    /// top. If reverse is set to true then the window at the bottom of the stack is moved up to the
    /// front and the previous top window will be immediately below it
    /// </summary>
    /// <param name="reverse">Direction to cycle through the windows</param>
    /// <returns>Itself</returns>
    public IWindowBasedTextGUI CycleActiveWindow(bool reverse)
    {
        lock (this)
        {
            _windowList.CycleActiveWindow(reverse);
            return this;
        }
    }

    private class MultiWindowTextGUIBackgroundPane : AbstractBasePane<IBasePane>
    {
        private readonly MultiWindowTextGUI _textGUI;

        public MultiWindowTextGUIBackgroundPane(MultiWindowTextGUI textGUI)
        {
            _textGUI = textGUI;
        }

        public override ITextGUI? TextGUI => _textGUI;

        public override TerminalPosition? ToGlobal(TerminalPosition localPosition)
        {
            return localPosition;
        }

        public override TerminalPosition? FromGlobal(TerminalPosition globalPosition)
        {
            return globalPosition;
        }

        protected override IBasePane Self => this;
    }
}