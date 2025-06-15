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

namespace Lanterna.Terminal.Virtual;

/// <summary>
/// Listener interface for IVirtualTerminal that allows you to receive callbacks on certain events. Please note that
/// while this extends ITerminalResizeListener and can be attached to a IVirtualTerminal through
/// ITerminal.AddResizeListener(ITerminalResizeListener), in that case only the
/// resize event will fire on the listener.
/// </summary>
public interface IVirtualTerminalListener : ITerminalResizeListener
{
    /// <summary>
    /// Called when the ITerminal.Flush() method is invoked on the IVirtualTerminal
    /// </summary>
    void OnFlush();

    /// <summary>
    /// Called when the ITerminal.Bell() method is invoked on the IVirtualTerminal
    /// </summary>
    void OnBell();

    /// <summary>
    /// Called when the ITerminal.Dispose() method is invoked on the IVirtualTerminal
    /// </summary>
    void OnClose();
}