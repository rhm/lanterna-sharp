namespace Lanterna.Gui2;

public interface ITextGUIThread
{
    void Start();
    
    void Stop();
    
    void WaitForStop();
    
    void InvokeAndWait(Action task);
    
    Task InvokeAsync(Action task);
    
    bool IsEventDispatchThread { get; }
}