using Lanterna.Core;

namespace Lanterna.Gui2;

public interface IComponentRenderer<in T> where T : IComponent
{
    TerminalSize GetPreferredSize(T component);
    
    void DrawComponent(ITextGUIGraphics graphics, T component);
}