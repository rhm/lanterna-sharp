using Lanterna.Core;
using Lanterna.Input;

namespace Lanterna.Gui2;

public interface IWindow : IBasePane
{
    public class Hint
    {
        public static readonly Hint NoDecorations = new Hint("NoDeco");
        public static readonly Hint NoPostRendering = new Hint("NoPostRend");
        public static readonly Hint NoFocus = new Hint("NoFocus");
        public static readonly Hint Centered = new Hint("Centered");
        public static readonly Hint FixedPosition = new Hint("FixedPos");
        public static readonly Hint MenuPopup = new Hint("MenuPopup");
        public static readonly Hint FixedSize = new Hint("FixedSize");
        public static readonly Hint FitTerminalWindow = new Hint("FitTermWin");
        public static readonly Hint Modal = new Hint("Modal");
        public static readonly Hint FullScreen = new Hint("FullScreen");
        public static readonly Hint Expanded = new Hint("Expanded");

        private readonly string _info;

        protected Hint()
        {
            _info = "";
        }

        protected Hint(string info)
        {
            _info = info;
        }

        public override string ToString()
        {
            return !string.IsNullOrEmpty(_info) ? _info : base.ToString();
        }
    }

    new IWindowBasedTextGUI? TextGUI { get; }
    
    void SetTextGUI(IWindowBasedTextGUI textGUI);
    
    string Title { get; }
    
    bool Visible { get; set; }
    
    TerminalRectangle Bounds { get; }
    
    TerminalSize PreferredSize { get; }
    
    void Close();
    
    void SetHints(ICollection<Hint> hints);
    
    ISet<Hint> Hints { get; }
    
    TerminalPosition Position { get; set; }
    
    TerminalSize Size { get; set; }
    
    void SetFixedSize(TerminalSize size);
    
    TerminalSize DecoratedSize { get; set; }
    
    void SetContentOffset(TerminalPosition offset);
    
    void WaitUntilClosed();
    
    IWindowPostRenderer? PostRenderer { get; }
    
    void AddWindowListener(IWindowListener windowListener);
    
    void RemoveWindowListener(IWindowListener windowListener);
    
    TerminalPosition ToGlobalFromContentRelative(TerminalPosition localPosition);
    
    TerminalPosition ToGlobalFromDecoratedRelative(TerminalPosition decoratedPosition);
    
    TerminalPosition FromGlobalToContentRelative(TerminalPosition position);
    
    TerminalPosition FromGlobalToDecoratedRelative(TerminalPosition position);
    
    void SetMenuBar(IMenuBar? menuBar);
    
    IMenuBar? MenuBar { get; }
}


public interface IWindowPostRenderer
{
    void PostRender(ITextGUIGraphics graphics, IWindow window);
}

public interface IWindowListener
{
    void OnResized(IWindow window, TerminalSize oldSize, TerminalSize newSize);
    void OnMoved(IWindow window, TerminalPosition oldPosition, TerminalPosition newPosition);
    void OnInput(IWindow window, KeyStroke keyStroke, ref bool handled);
    void OnUnhandledInput(IWindow window, KeyStroke keyStroke, ref bool handled);
}

public interface IMenuBar : IComponent
{
    // Menu bar interface placeholder
}