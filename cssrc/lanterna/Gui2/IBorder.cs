using Lanterna.Core;

namespace Lanterna.Gui2;

public interface IBorder : IComponent, IComposite
{
    IComponent WrappedComponent { get; }
    
    TerminalSize WrappedSize { get; }
}