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
 * Copyright (C) 2010-2024 Martin Berglund
 */

using Lanterna.Graphics;

namespace Lanterna.Terminal;

/// <summary>
/// This interface extends the normal ITerminal interface and adds a few more methods that are considered rare and shouldn't
/// be encouraged to be used. Some of these may move into ITerminal if it turns out that they are indeed well-supported.
/// Most of these extensions are picked up from here: http://invisible-island.net/xterm/ctlseqs/ctlseqs.html
/// 
/// This interface is NOT considered stable and may change within releases. Do not depend on methods in this interface
/// unless you are ok with occasionally having to fix broken code after minor library upgrades.
/// </summary>
public interface IExtendedTerminal : ITerminal, IScrollable
{
    /// <summary>
    /// Attempts to resize the terminal through dtterm extensions "CSI 8 ; rows ; columns ; t". This isn't widely
    /// supported, which is why the method is not exposed through the common ITerminal interface.
    /// </summary>
    /// <param name="columns">New size (columns)</param>
    /// <param name="rows">New size (rows)</param>
    /// <exception cref="IOException">If the was an underlying I/O error</exception>
    void SetTerminalSize(int columns, int rows);

    /// <summary>
    /// This methods sets the title of the terminal, which is normally only visible if you are running the application
    /// in a terminal emulator in a graphical environment.
    /// </summary>
    /// <param name="title">Title to set on the terminal</param>
    /// <exception cref="IOException">If the was an underlying I/O error</exception>
    void SetTitle(string title);

    /// <summary>
    /// Saves the current window title on a stack managed internally by the terminal.
    /// </summary>
    /// <exception cref="IOException">If the was an underlying I/O error</exception>
    void PushTitle();

    /// <summary>
    /// Replaces the terminal title with the top element from the title stack managed by the terminal (the element is
    /// removed from the stack as expected)
    /// </summary>
    /// <exception cref="IOException">If the was an underlying I/O error</exception>
    void PopTitle();

    /// <summary>
    /// Iconifies the terminal, this likely means minimizing the window with most window managers
    /// </summary>
    /// <exception cref="IOException">If the was an underlying I/O error</exception>
    void Iconify();

    /// <summary>
    /// De-iconifies the terminal, which likely means restoring it from minimized state with most window managers
    /// </summary>
    /// <exception cref="IOException">If the was an underlying I/O error</exception>
    void Deiconify();

    /// <summary>
    /// Maximizes the terminal, so that it takes up all available space
    /// </summary>
    /// <exception cref="IOException">If the was an underlying I/O error</exception>
    void Maximize();

    /// <summary>
    /// Restores the terminal back to its previous size, after having been maximized
    /// </summary>
    /// <exception cref="IOException">If the was an underlying I/O error</exception>
    void Unmaximize();

    /// <summary>
    /// Enabled or disables capturing of mouse event. This is not recommended to use as most users are not familiar with
    /// the fact that terminal emulators allow capturing mouse input. You can decide which events you want to capture but
    /// be careful since different terminal emulators will support these modes differently. Mouse capture mode will be
    /// automatically disabled when the application exits through a shutdown hook.
    /// </summary>
    /// <param name="mouseCaptureMode">Which mouse events to capture, pass in null to disable mouse input capturing</param>
    /// <exception cref="IOException">If the was an underlying I/O error</exception>
    void SetMouseCaptureMode(MouseCaptureMode? mouseCaptureMode);
}