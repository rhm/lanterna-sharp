using Lanterna.Core;

namespace Lanterna.Gui2;

public interface IInteractableLookupMap
{
    void Add(IInteractable interactable, TerminalPosition globalPosition);
    
    IInteractable? FindClosest(TerminalPosition position, IInteractable.FocusChangeDirection direction);
}