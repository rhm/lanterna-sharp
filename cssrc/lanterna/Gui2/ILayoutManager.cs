using Lanterna.Core;

namespace Lanterna.Gui2;

public interface ILayoutManager
{
    TerminalSize GetPreferredSize(IReadOnlyList<IComponent> components);
    
    void DoLayout(TerminalSize area, IReadOnlyList<IComponent> components);
    
    bool HasChanged { get; }
}