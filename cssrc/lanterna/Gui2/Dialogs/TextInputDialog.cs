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
using System.Numerics;
using System.Text.RegularExpressions;

namespace Lanterna.Gui2.Dialogs;

/// <summary>
/// TextInputDialog is a modal text input dialog that prompts the user to enter a text string. The class supports
/// validation and password masking. The builder class to help setup TextInputDialogs is
/// TextInputDialogBuilder.
/// </summary>
public class TextInputDialog : DialogWindow
{
    private readonly TextBox _textBox;
    private readonly ITextInputDialogResultValidator? _validator;
    private string? _result;

    internal TextInputDialog(
        string title,
        string? description,
        TerminalSize? textBoxPreferredSize,
        string? initialContent,
        ITextInputDialogResultValidator? validator,
        bool password) : base(title)
    {
        _result = null;
        _textBox = new TextBox(textBoxPreferredSize, initialContent);
        _validator = validator;

        if (password)
        {
            _textBox.SetMask('*');
        }

        var buttonPanel = new Panel();
        buttonPanel.SetLayoutManager(new GridLayout(2).SetHorizontalSpacing(1));
        buttonPanel.AddComponent(new Button(LocalizedString.OK.ToString(), OnOK).SetLayoutData(GridLayout.CreateLayoutData(GridLayout.Alignment.Center, GridLayout.Alignment.Center, true, false)));
        buttonPanel.AddComponent(new Button(LocalizedString.Cancel.ToString(), OnCancel));

        var mainPanel = new Panel();
        mainPanel.SetLayoutManager(
            new GridLayout(1)
                .SetLeftMarginSize(1)
                .SetRightMarginSize(1));
        if (description != null)
        {
            mainPanel.AddComponent(new Label(description));
        }
        mainPanel.AddComponent(new EmptySpace(TerminalSize.One));
        _textBox.SetLayoutData(
            GridLayout.CreateLayoutData(
                GridLayout.Alignment.Fill,
                GridLayout.Alignment.Center,
                true,
                false))
            .AddTo(mainPanel);
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

    private void OnOK()
    {
        string text = _textBox.GetText();
        if (_validator != null)
        {
            string? errorMessage = _validator.Validate(text);
            if (errorMessage != null)
            {
                MessageDialog.ShowMessageDialog((IWindowBasedTextGUI)TextGUI!, Title, errorMessage, MessageDialogButton.OK);
                return;
            }
        }
        _result = text;
        Close();
    }

    private void OnCancel()
    {
        Close();
    }

    /// <summary>
    /// Opens the dialog by showing it on the GUI and doesn't return until the dialog has been closed
    /// </summary>
    /// <param name="textGUI">Text GUI to add the dialog to</param>
    /// <returns>The string the user typed into the text box, or null if the dialog was cancelled</returns>
    public override object? ShowDialog(IWindowBasedTextGUI textGUI)
    {
        _result = null;
        base.ShowDialog(textGUI);
        return _result;
    }

    /// <summary>
    /// Shortcut for quickly showing a TextInputDialog
    /// </summary>
    /// <param name="textGUI">GUI to show the dialog on</param>
    /// <param name="title">Title of the dialog</param>
    /// <param name="description">Description of the dialog</param>
    /// <param name="initialContent">What content to place in the text box initially</param>
    /// <returns>The string the user typed into the text box, or null if the dialog was cancelled</returns>
    public static string? ShowDialog(IWindowBasedTextGUI textGUI, string title, string? description, string? initialContent)
    {
        var textInputDialog = new TextInputDialogBuilder()
            .SetTitle(title)
            .SetDescription(description)
            .SetInitialContent(initialContent)
            .Build();
        return textInputDialog.ShowDialog(textGUI) as string;
    }

    /// <summary>
    /// Shortcut for quickly showing a TextInputDialog that only accepts numbers
    /// </summary>
    /// <param name="textGUI">GUI to show the dialog on</param>
    /// <param name="title">Title of the dialog</param>
    /// <param name="description">Description of the dialog</param>
    /// <param name="initialContent">What content to place in the text box initially</param>
    /// <returns>The number the user typed into the text box, or null if the dialog was cancelled</returns>
    public static BigInteger? ShowNumberDialog(IWindowBasedTextGUI textGUI, string title, string? description, string? initialContent)
    {
        var textInputDialog = new TextInputDialogBuilder()
            .SetTitle(title)
            .SetDescription(description)
            .SetInitialContent(initialContent)
            .SetValidationPattern(new Regex(@"[0-9]+"), "Not a number")
            .Build();
        string? numberString = textInputDialog.ShowDialog(textGUI) as string;
        return numberString != null ? BigInteger.Parse(numberString) : null;
    }

    /// <summary>
    /// Shortcut for quickly showing a TextInputDialog with password masking
    /// </summary>
    /// <param name="textGUI">GUI to show the dialog on</param>
    /// <param name="title">Title of the dialog</param>
    /// <param name="description">Description of the dialog</param>
    /// <param name="initialContent">What content to place in the text box initially</param>
    /// <returns>The string the user typed into the text box, or null if the dialog was cancelled</returns>
    public static string? ShowPasswordDialog(IWindowBasedTextGUI textGUI, string title, string? description, string? initialContent)
    {
        var textInputDialog = new TextInputDialogBuilder()
            .SetTitle(title)
            .SetDescription(description)
            .SetInitialContent(initialContent)
            .SetPasswordInput(true)
            .Build();
        return textInputDialog.ShowDialog(textGUI) as string;
    }
}