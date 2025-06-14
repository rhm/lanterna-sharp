/*
 * This file is part of lanterna (https://github.com/mabe02/lanterna).
 *
 * lanterna is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * Copyright (C) 2010-2020 Martin Berglund
 */

namespace Lanterna.Gui2;

/// <summary>
/// This ITextGUIThread implementation is assuming the GUI event thread will be the same as the thread that
/// creates the ITextGUI objects. This means on the thread you create the GUI on, when you are done you pass over
/// control to lanterna and let it manage the GUI for you. When the GUI is done, you'll get back control again over the
/// thread. This is different from SeparateTextGUIThread which spawns a new thread that manages the GUI and
/// leaves the current thread for you to handle.
/// <para>
/// Here are two examples of how to use SameTextGUIThread:
/// <code>
/// MultiWindowTextGUI textGUI = new MultiWindowTextGUI(new SameTextGUIThread.Factory(), screen);
/// // ... add components ...
/// while(weWantToContinueRunningTheGUI) {
///     if(!textGUI.GUIThread.ProcessEventsAndUpdate()) {
///         Thread.Sleep(1);
///     }
/// }
/// // ... tear down ...
/// </code>
/// In the example above, we use very precise control over events processing and when to update the GUI. In the example
/// below we pass some of that control over to Lanterna, since the thread won't resume until the window is closed.
/// <code>
/// MultiWindowTextGUI textGUI = new MultiWindowTextGUI(new SameTextGUIThread.Factory(), screen);
/// Window window = new MyWindow();
/// textGUI.AddWindowAndWait(window); // This call will run the event/update loop and won't return until "window" is closed
/// // ... tear down ...
/// </code>
/// </para>
/// </summary>
/// <seealso cref="SeparateTextGUIThread"/>
/// <seealso cref="ITextGUIThread"/>
public class SameTextGUIThread : AbstractTextGUIThread
{
    private readonly System.Threading.Thread _guiThread;

    private SameTextGUIThread(ITextGUI textGUI) : base(textGUI)
    {
        _guiThread = System.Threading.Thread.CurrentThread;

        // By default, reset the exception handler to null so exceptions generated in the ProcessEventsAndUpdate method are thrown
        // back out instead of logged and dropped
        ExceptionHandler = null;
    }

    public override System.Threading.Thread? Thread => _guiThread;

    /// <summary>
    /// Default factory class for SameTextGUIThread, you need to pass this to the ITextGUI constructor if
    /// you want it to use this class
    /// </summary>
    public class Factory : ITextGUIThreadFactory
    {
        public ITextGUIThread CreateTextGUIThread(ITextGUI textGUI)
        {
            return new SameTextGUIThread(textGUI);
        }
    }
}