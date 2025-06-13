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
/// Dialog that displays a text message, an optional spinning indicator and an optional progress bar. There are no buttons
/// in this dialog so it has to be explicitly closed through code.
/// </summary>
public class WaitingDialog : DialogWindow
{
    private WaitingDialog(string title, string text) : base(title)
    {
        var mainPanel = Panels.Horizontal(
            new Label(text),
            AnimatedLabel.CreateClassicSpinningLine());
        Component = mainPanel;
    }

    public override object? ShowDialog(IWindowBasedTextGUI textGUI)
    {
        ShowDialog(textGUI, true);
        return null;
    }

    /// <summary>
    /// Displays the waiting dialog and optionally blocks until another thread closes it
    /// </summary>
    /// <param name="textGUI">GUI to add the dialog to</param>
    /// <param name="blockUntilClosed">If true, the method call will block until another thread calls Close() on
    ///                                the dialog, otherwise the method call returns immediately</param>
    public void ShowDialog(IWindowBasedTextGUI textGUI, bool blockUntilClosed)
    {
        textGUI.AddWindow(this);

        if (blockUntilClosed)
        {
            // Wait for the window to close, in case the window manager doesn't honor the MODAL hint
            WaitUntilClosed();
        }
    }

    /// <summary>
    /// Creates a new waiting dialog
    /// </summary>
    /// <param name="title">Title of the waiting dialog</param>
    /// <param name="text">Text to display on the waiting dialog</param>
    /// <returns>Created waiting dialog</returns>
    public static WaitingDialog CreateDialog(string title, string text)
    {
        return new WaitingDialog(title, text);
    }

    /// <summary>
    /// Creates and displays a waiting dialog without blocking for it to finish
    /// </summary>
    /// <param name="textGUI">GUI to add the dialog to</param>
    /// <param name="title">Title of the waiting dialog</param>
    /// <param name="text">Text to display on the waiting dialog</param>
    /// <returns>Created waiting dialog</returns>
    public static WaitingDialog ShowDialog(IWindowBasedTextGUI textGUI, string title, string text)
    {
        var waitingDialog = CreateDialog(title, text);
        waitingDialog.ShowDialog(textGUI, false);
        return waitingDialog;
    }
}