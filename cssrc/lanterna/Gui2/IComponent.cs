using Lanterna.Core;
using Lanterna.Graphics;

namespace Lanterna.Gui2;

public interface IComponent : ITextGUIElement
{
    TerminalPosition Position { get; set; }
    
    TerminalPosition GlobalPosition { get; }
    
    TerminalSize Size { get; set; }
    
    TerminalSize PreferredSize { get; }
    
    IComponent SetPreferredSize(TerminalSize? explicitPreferredSize);
    
    IComponent SetLayoutData(ILayoutData? data);
    
    ILayoutData? LayoutData { get; }
    
    bool Visible { get; set; }
    
    IContainer? Parent { get; }
    
    bool HasParent(IContainer parent);
    
    ITextGUI? TextGUI { get; }
    
    Theme Theme { get; }
    
    ThemeDefinition ThemeDefinition { get; }
    
    IComponent SetTheme(Theme? theme);
    
    bool IsInside(IContainer container);
    
    IComponentRenderer<IComponent> Renderer { get; }
    
    void Invalidate();
    
    IBorder WithBorder(IBorder border);
    
    TerminalPosition? ToBasePane(TerminalPosition position);
    
    TerminalPosition? ToGlobal(TerminalPosition position);
    
    IBasePane? BasePane { get; }
    
    IComponent AddTo(IPanel panel);
    
    void OnAdded(IContainer container);
    
    void OnRemoved(IContainer container);
}