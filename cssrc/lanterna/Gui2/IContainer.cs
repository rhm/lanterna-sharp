using Lanterna.Input;

namespace Lanterna.Gui2;

public interface IContainer : IComponent
{
    int ChildCount { get; }
    
    IReadOnlyCollection<IComponent> Children { get; }
    
    IReadOnlyList<IComponent> ChildrenList { get; }
    
    bool ContainsComponent(IComponent component);
    
    bool RemoveComponent(IComponent component);
    
    IInteractable? NextFocus(IInteractable? fromThis);
    
    IInteractable? PreviousFocus(IInteractable? fromThis);
    
    bool HandleInput(KeyStroke key);
    
    void UpdateLookupMap(IInteractableLookupMap interactableLookupMap);
}