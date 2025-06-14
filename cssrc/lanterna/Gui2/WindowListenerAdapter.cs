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
using Lanterna.Input;

namespace Lanterna.Gui2;

/// <summary>
/// Adapter class for IWindowListener to make it easier to create listeners without having to implement every
/// interface method.
/// </summary>
public class WindowListenerAdapter : IWindowListener
{
    public virtual void OnResized(IWindow window, TerminalSize oldSize, TerminalSize newSize)
    {
        // Empty default implementation
    }

    public virtual void OnMoved(IWindow window, TerminalPosition oldPosition, TerminalPosition newPosition)
    {
        // Empty default implementation
    }

    public virtual void OnInput(IWindow basePane, KeyStroke keyStroke, ref bool deliverEvent)
    {
        // Empty default implementation
    }

    public virtual void OnUnhandledInput(IWindow basePane, KeyStroke keyStroke, ref bool hasBeenHandled)
    {
        // Empty default implementation
    }
}