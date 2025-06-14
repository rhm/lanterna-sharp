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

using Lanterna.Input;

namespace Lanterna.Gui2;

/// <summary>
/// Base listener interface having callback methods for events relating to IBasePane (and IWindow, which
/// extends IBasePane) so that you can be notified by a callback when certain events happen. Assume it is the GUI
/// thread that will call these methods. You typically use this through IWindowListener and calling
/// IWindow.AddWindowListener(IWindowListener)
/// </summary>
/// <typeparam name="T">Type of base pane this listener handles</typeparam>
public interface IBasePaneListener<in T> where T : IBasePane
{
    /// <summary>
    /// Called when a user input is about to be delivered to the focused IInteractable inside the
    /// IBasePane, but before it is actually delivered. You can catch it and prevent it from being passed into
    /// the component by using the deliverEvent parameter and setting it to false.
    /// </summary>
    /// <param name="basePane">Base pane that got the input event</param>
    /// <param name="keyStroke">The actual input event</param>
    /// <param name="deliverEvent">Set to true automatically, if you change it to false it will prevent the GUI
    /// from passing the input event on to the focused IInteractable</param>
    void OnInput(T basePane, KeyStroke keyStroke, ref bool deliverEvent);

    /// <summary>
    /// Called when a user entered some input which wasn't handled by the focused component. This allows you to catch it
    /// at a IBasePane (or IWindow) level and prevent it from being reported to the ITextGUI as an
    /// unhandled input event.
    /// </summary>
    /// <param name="basePane">IBasePane that got the input event</param>
    /// <param name="keyStroke">The unhandled input event</param>
    /// <param name="hasBeenHandled">Initially set to false, if you change it to true then the event
    /// will not be reported as an unhandled input to the ITextGUI</param>
    void OnUnhandledInput(T basePane, KeyStroke keyStroke, ref bool hasBeenHandled);
}