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
using System.Text.RegularExpressions;

namespace Lanterna.Gui2.Dialogs;

/// <summary>
/// Dialog builder for the TextInputDialog class, use this to create instances of that class and to customize
/// them
/// </summary>
public class TextInputDialogBuilder : AbstractDialogBuilder<TextInputDialogBuilder, TextInputDialog>
{
    private string? _initialContent;
    private TerminalSize? _textBoxSize;
    private ITextInputDialogResultValidator? _validator;
    private bool _passwordInput;

    /// <summary>
    /// Default constructor
    /// </summary>
    public TextInputDialogBuilder() : base("TextInputDialog")
    {
        _initialContent = "";
        _textBoxSize = null;
        _validator = null;
        _passwordInput = false;
    }

    protected override TextInputDialogBuilder Self()
    {
        return this;
    }

    protected override TextInputDialog BuildDialog()
    {
        var size = _textBoxSize;
        if ((string.IsNullOrWhiteSpace(_initialContent)) && size == null)
        {
            size = new TerminalSize(40, 1);
        }
        return new TextInputDialog(
            _title,
            _description,
            size,
            _initialContent,
            _validator,
            _passwordInput);
    }

    /// <summary>
    /// Sets the initial content the dialog will have
    /// </summary>
    /// <param name="initialContent">Initial content the dialog will have</param>
    /// <returns>Itself</returns>
    public TextInputDialogBuilder SetInitialContent(string? initialContent)
    {
        _initialContent = initialContent;
        return this;
    }

    /// <summary>
    /// Returns the initial content the dialog will have
    /// </summary>
    /// <returns>Initial content the dialog will have</returns>
    public string? GetInitialContent()
    {
        return _initialContent;
    }

    /// <summary>
    /// Sets the size of the text box the dialog will have
    /// </summary>
    /// <param name="textBoxSize">Size of the text box the dialog will have</param>
    /// <returns>Itself</returns>
    public TextInputDialogBuilder SetTextBoxSize(TerminalSize textBoxSize)
    {
        _textBoxSize = textBoxSize;
        return this;
    }

    /// <summary>
    /// Returns the size of the text box the dialog will have
    /// </summary>
    /// <returns>Size of the text box the dialog will have</returns>
    public TerminalSize? GetTextBoxSize()
    {
        return _textBoxSize;
    }

    /// <summary>
    /// Sets the validator that will be attached to the text box in the dialog
    /// </summary>
    /// <param name="validator">Validator that will be attached to the text box in the dialog</param>
    /// <returns>Itself</returns>
    public TextInputDialogBuilder SetValidator(ITextInputDialogResultValidator? validator)
    {
        _validator = validator;
        return this;
    }

    /// <summary>
    /// Returns the validator that will be attached to the text box in the dialog
    /// </summary>
    /// <returns>validator that will be attached to the text box in the dialog</returns>
    public ITextInputDialogResultValidator? GetValidator()
    {
        return _validator;
    }

    /// <summary>
    /// Helper method that assigned a validator to the text box the dialog will have which matches the pattern supplied
    /// </summary>
    /// <param name="pattern">Pattern to validate the text box</param>
    /// <param name="errorMessage">Error message to show when the pattern doesn't match</param>
    /// <returns>Itself</returns>
    public TextInputDialogBuilder SetValidationPattern(Regex pattern, string? errorMessage)
    {
        return SetValidator(new RegexValidator(pattern, errorMessage));
    }

    /// <summary>
    /// Sets if the text box the dialog will have contains a password and should be masked (default: false)
    /// </summary>
    /// <param name="passwordInput">true if the text box should be password masked, false otherwise</param>
    /// <returns>Itself</returns>
    public TextInputDialogBuilder SetPasswordInput(bool passwordInput)
    {
        _passwordInput = passwordInput;
        return this;
    }

    /// <summary>
    /// Returns true if the text box the dialog will have contains a password and should be masked
    /// </summary>
    /// <returns>true if the text box the dialog will have contains a password and should be masked</returns>
    public bool IsPasswordInput()
    {
        return _passwordInput;
    }

    private class RegexValidator : ITextInputDialogResultValidator
    {
        private readonly Regex _pattern;
        private readonly string? _errorMessage;

        public RegexValidator(Regex pattern, string? errorMessage)
        {
            _pattern = pattern;
            _errorMessage = errorMessage;
        }

        public string? Validate(string content)
        {
            if (!_pattern.IsMatch(content))
            {
                return _errorMessage ?? "Invalid input";
            }
            return null;
        }
    }
}