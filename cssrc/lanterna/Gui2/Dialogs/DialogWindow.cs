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
namespace Lanterna.Gui2.Dialogs;

/// <summary>
/// Thin layer on top of the AbstractWindow class that automatically sets properties and hints to the window to
/// make it act more like a modal dialog window
/// </summary>
public abstract class DialogWindow : AbstractWindow
{
    private static readonly ISet<IWindow.Hint> GlobalDialogHints = 
        new HashSet<IWindow.Hint> { IWindow.Hint.Modal };

    /// <summary>
    /// Default constructor, takes a title for the dialog and runs code shared for dialogs
    /// </summary>
    /// <param name="title">Title of the window</param>
    protected DialogWindow(string title) : base(title)
    {
        SetHints(GlobalDialogHints);
    }

    /// <summary>
    /// Opens the dialog by showing it on the GUI and doesn't return until the dialog has been closed
    /// </summary>
    /// <param name="textGUI">Text GUI to add the dialog to</param>
    /// <returns>Depending on the DialogWindow implementation, by default null</returns>
    public virtual object? ShowDialog(IWindowBasedTextGUI textGUI)
    {
        textGUI.AddWindow(this);

        // Wait for the window to close, in case the window manager doesn't honor the MODAL hint
        WaitUntilClosed();
        return null;
    }
}