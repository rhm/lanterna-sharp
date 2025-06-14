namespace Lanterna.Gui2;

/// <summary>
/// Extension of the ITextGUI interface, this is intended as the base interface for any ITextGUI that intends to make use
/// of the IWindow class.
/// </summary>
public interface IWindowBasedTextGUI : ITextGUI
{
    /// <summary>
    /// Returns the window manager that is currently controlling this TextGUI. The window manager is in charge of placing
    /// the windows on the surface and also deciding how they behave and move around.
    /// </summary>
    /// <returns>Window manager that is currently controlling the windows in the terminal</returns>
    IWindowManager WindowManager { get; }

    /// <summary>
    /// Adds a window to the TextGUI system, depending on the window manager this window may or may not be immediately
    /// visible. By adding a window to the GUI, it will be associated with this GUI and can receive focus and events from
    /// it. This method call will return immediately, if you want the call to block until the window is closed, please
    /// use AddWindowAndWait(..).
    /// </summary>
    /// <param name="window">Window to add to the GUI</param>
    /// <returns>The IWindowBasedTextGUI itself</returns>
    IWindowBasedTextGUI AddWindow(IWindow window);

    /// <summary>
    /// Adds a window to the TextGUI system, depending on the window manager this window may or may not be immediately
    /// visible. By adding a window to the GUI, it will be associated with this GUI and can receive focus and events from
    /// it. This method blocks until the added window is removed or closed, if you want the call to return immediately,
    /// please use AddWindow(..). This method call is useful for modal dialogs that requires a certain user input
    /// before the application can continue.
    /// </summary>
    /// <param name="window">Window to add to the GUI</param>
    /// <returns>The IWindowBasedTextGUI itself</returns>
    IWindowBasedTextGUI AddWindowAndWait(IWindow window);

    /// <summary>
    /// Removes a window from the TextGUI. This is effectively the same as closing the window. The window will be
    /// unassociated from this TextGUI and will no longer receive any events for it. Any threads waiting on the window
    /// to close will be resumed.
    /// </summary>
    /// <param name="window">Window to close</param>
    /// <returns>The IWindowBasedTextGUI itself</returns>
    IWindowBasedTextGUI RemoveWindow(IWindow window);

    /// <summary>
    /// Returns a list of all windows currently in the TextGUI. The list is unmodifiable and just a snapshot of what the
    /// state was when the method was invoked. If windows are added/removed after the method call, the list will not
    /// reflect this.
    /// </summary>
    /// <returns>Unmodifiable list of all windows in the TextGUI at the time of the call</returns>
    IReadOnlyList<IWindow> Windows { get; }

    /// <summary>
    /// Selects a particular window to be considered 'active' and receive all input events
    /// </summary>
    /// <param name="activeWindow">Window to become active and receive input events</param>
    /// <returns>The IWindowBasedTextGUI itself</returns>
    IWindowBasedTextGUI SetActiveWindow(IWindow? activeWindow);

    /// <summary>
    /// Returns the window which the TextGUI considers the active one at the time of the method call. The active window
    /// is generally the one which relieves all keyboard input.
    /// </summary>
    /// <returns>Active window in the TextGUI or null</returns>
    IWindow? ActiveWindow { get; }

    /// <summary>
    /// Returns the container for the background, which works as a single large component that takes up the whole
    /// terminal area and is always behind all windows.
    /// </summary>
    /// <returns>The IBasePane used by this IWindowBasedTextGUI</returns>
    IBasePane BackgroundPane { get; }

    /// <summary>
    /// Returns the IWindowPostRenderer for this IWindowBasedTextGUI
    /// </summary>
    /// <returns>the IWindowPostRenderer for this IWindowBasedTextGUI</returns>
    IWindowPostRenderer? WindowPostRenderer { get; }

    /// <summary>
    /// Windows are internally stored as a stack and newer windows are added at the top of the stack. The GUI system will
    /// render windows in a predictable order from bottom to top. This method allows you to move a Window from its
    /// current position in the stack to the top, meaning it will be rendered last. This mean it will overlap all other
    /// windows and because of this visually appear on top.
    /// </summary>
    /// <param name="window">Window in the stack to move to the top position</param>
    /// <returns>The IWindowBasedTextGUI itself</returns>
    IWindowBasedTextGUI MoveToTop(IWindow window);

    /// <summary>
    /// Takes the previously active window and makes it active, or if in reverse mode, takes the window at the bottom of
    /// the stack, moves it to the front and makes it active.
    /// </summary>
    /// <param name="reverse">Direction to cycle through the windows</param>
    /// <returns>The IWindowBasedTextGUI itself</returns>
    IWindowBasedTextGUI CycleActiveWindow(bool reverse);

    /// <summary>
    /// Waits for the specified window to be closed
    /// </summary>
    /// <param name="window">Window to wait for</param>
    void WaitForWindowToClose(IWindow window);
}