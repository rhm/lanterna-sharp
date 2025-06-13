using Lanterna.Core;
using Lanterna.Graphics;
using Lanterna.Input;
using Lanterna.Screen;

namespace Lanterna.Gui2;

public interface ITextGUI
{
    Theme Theme { get; set; }
    
    bool ProcessInput();
    
    IScreen Screen { get; }
    
    void UpdateScreen();
    
    bool IsPendingUpdate { get; }
    
    bool VirtualScreenEnabled { get; set; }
    
    ITextGUIThread GUIThread { get; }
    
    IInteractable? FocusedInteractable { get; }
    
    void AddListener(IListener listener);
    
    void RemoveListener(IListener listener);
    
    public interface IListener
    {
        bool OnUnhandledKeyStroke(ITextGUI textGUI, KeyStroke keyStroke);
    }
}