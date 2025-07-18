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

using Lanterna.Core;

namespace Lanterna.Terminal;

/// <summary>
/// Listener interface that can be used to be alerted on terminal resizing
/// </summary>
public interface ITerminalResizeListener
{
    /// <summary>
    /// The terminal has changed its size, most likely because the user has resized the window. This callback is
    /// invoked by something inside the lanterna library, it could be a signal handler thread, it could be the main
    /// thread, it could be something else, so please be careful with what kind of operation you do in here. Also,
    /// make sure not to take too long before returning. Best practice would be to update an internal status in your
    /// program to mark that the terminal has been resized (possibly along with the new size) and then in your main
    /// loop you deal with this at the beginning of each redraw.
    /// </summary>
    /// <param name="terminal">Terminal that was resized</param>
    /// <param name="newSize">Size of the terminal after the resize</param>
    void OnResized(ITerminal terminal, TerminalSize newSize);
}