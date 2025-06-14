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

namespace Lanterna.Gui2.Dialogs;

/// <summary>
/// Simple message dialog that displays a message and has optional selection/confirmation buttons
/// </summary>
public class MessageDialog : DialogWindow
{
    private MessageDialogButton? _result;

    internal MessageDialog(string title, string text, params MessageDialogButton[] buttons) : base(title)
    {
        _result = null;
        if (buttons == null || buttons.Length == 0)
        {
            buttons = new[] { MessageDialogButton.OK };
        }

        var buttonPanel = new Panel();
        buttonPanel.SetLayoutManager(new GridLayout(buttons.Length).SetHorizontalSpacing(1));
        foreach (var button in buttons)
        {
            var capturedButton = button; // Capture for closure
            buttonPanel.AddComponent(new Button(button.GetLabel(), () =>
            {
                _result = capturedButton;
                Close();
            }));
        }

        var mainPanel = new Panel();
        mainPanel.SetLayoutManager(
            new GridLayout(1)
                .SetLeftMarginSize(1)
                .SetRightMarginSize(1));
        mainPanel.AddComponent(new Label(text));
        mainPanel.AddComponent(new EmptySpace(TerminalSize.One));
        buttonPanel.SetLayoutData(
            GridLayout.CreateLayoutData(
                GridLayout.Alignment.End,
                GridLayout.Alignment.Center,
                false,
                false))
            .AddTo(mainPanel);
        Component = mainPanel;
    }

    /// <summary>
    /// Opens the dialog by showing it on the GUI and doesn't return until the dialog has been closed
    /// </summary>
    /// <param name="textGUI">Text GUI to add the dialog to</param>
    /// <returns>The selected button's enum value</returns>
    public override object? ShowDialog(IWindowBasedTextGUI textGUI)
    {
        _result = null;
        base.ShowDialog(textGUI);
        return _result;
    }

    /// <summary>
    /// Shortcut for quickly displaying a message box
    /// </summary>
    /// <param name="textGUI">The GUI to display the message box on</param>
    /// <param name="title">Title of the message box</param>
    /// <param name="text">Main message of the message box</param>
    /// <param name="buttons">Buttons that the user can confirm the message box with</param>
    /// <returns>Which button the user selected</returns>
    public static MessageDialogButton? ShowMessageDialog(
        IWindowBasedTextGUI textGUI,
        string title,
        string text,
        params MessageDialogButton[] buttons)
    {
        var builder = new MessageDialogBuilder()
            .SetTitle(title)
            .SetText(text);
        if (buttons.Length == 0)
        {
            builder.AddButton(MessageDialogButton.OK);
        }
        foreach (var button in buttons)
        {
            builder.AddButton(button);
        }
        return builder.Build().ShowDialog(textGUI) as MessageDialogButton?;
    }

    protected override IWindow Self => this;
}