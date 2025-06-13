namespace Lanterna.Gui2;

public interface IPanel : IContainer
{
    IPanel AddComponent(IComponent component);
    
    IPanel AddComponent(IComponent component, ILayoutData? layoutData);
    
    ILayoutManager LayoutManager { get; set; }
}