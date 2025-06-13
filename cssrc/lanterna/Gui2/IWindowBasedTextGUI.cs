namespace Lanterna.Gui2;

public interface IWindowBasedTextGUI : ITextGUI
{
    void AddWindow(IWindow window);
    
    bool RemoveWindow(IWindow window);
    
    IReadOnlyList<IWindow> Windows { get; }
    
    IWindow? ActiveWindow { get; }
    
    void SetActiveWindow(IWindow window);
    
    void WaitForWindowToClose(IWindow window);
}