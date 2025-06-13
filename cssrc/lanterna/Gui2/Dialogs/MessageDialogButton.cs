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
namespace Lanterna.Gui2.Dialogs;

/// <summary>
/// This enum has the available selection of buttons that you can add to a MessageDialog. They are used both for
/// specifying which buttons the dialog will have but is also returned when the user makes a selection
/// </summary>
public enum MessageDialogButton
{
    /// <summary>
    /// "OK"
    /// </summary>
    OK,
    
    /// <summary>
    /// "Cancel"
    /// </summary>
    Cancel,
    
    /// <summary>
    /// "Yes"
    /// </summary>
    Yes,
    
    /// <summary>
    /// "No"
    /// </summary>
    No,
    
    /// <summary>
    /// "Close"
    /// </summary>
    Close,
    
    /// <summary>
    /// "Abort"
    /// </summary>
    Abort,
    
    /// <summary>
    /// "Ignore"
    /// </summary>
    Ignore,
    
    /// <summary>
    /// "Retry"
    /// </summary>
    Retry,
    
    /// <summary>
    /// "Continue"
    /// </summary>
    Continue
}

/// <summary>
/// Extension methods for MessageDialogButton to get localized labels
/// </summary>
public static class MessageDialogButtonExtensions
{
    /// <summary>
    /// Gets the localized string for this button
    /// </summary>
    /// <param name="button">The button to get the label for</param>
    /// <returns>Localized string representation of the button</returns>
    public static string GetLabel(this MessageDialogButton button)
    {
        return button switch
        {
            MessageDialogButton.OK => LocalizedString.OK.ToString(),
            MessageDialogButton.Cancel => LocalizedString.Cancel.ToString(),
            MessageDialogButton.Yes => LocalizedString.Yes.ToString(),
            MessageDialogButton.No => LocalizedString.No.ToString(),
            MessageDialogButton.Close => LocalizedString.Close.ToString(),
            MessageDialogButton.Abort => LocalizedString.Abort.ToString(),
            MessageDialogButton.Ignore => LocalizedString.Ignore.ToString(),
            MessageDialogButton.Retry => LocalizedString.Retry.ToString(),
            MessageDialogButton.Continue => LocalizedString.Continue.ToString(),
            _ => button.ToString()
        };
    }
}