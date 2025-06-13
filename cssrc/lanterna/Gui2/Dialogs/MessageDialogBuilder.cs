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
/// Dialog builder for the MessageDialog class, use this to create instances of that class and to customize
/// them
/// </summary>
public class MessageDialogBuilder
{
    private string _title;
    private string _text;
    private readonly List<MessageDialogButton> _buttons;
    private readonly ISet<IWindow.Hint> _extraWindowHints;

    /// <summary>
    /// Default constructor
    /// </summary>
    public MessageDialogBuilder()
    {
        _title = "MessageDialog";
        _text = "Text";
        _buttons = new List<MessageDialogButton>();
        _extraWindowHints = new HashSet<IWindow.Hint> { IWindow.Hint.Centered };
    }

    /// <summary>
    /// Builds a new MessageDialog from the properties in the builder
    /// </summary>
    /// <returns>Newly build MessageDialog</returns>
    public MessageDialog Build()
    {
        var messageDialog = new MessageDialog(
            _title,
            _text,
            _buttons.ToArray());
        messageDialog.SetHints(_extraWindowHints);
        return messageDialog;
    }

    /// <summary>
    /// Sets the title of the MessageDialog
    /// </summary>
    /// <param name="title">New title of the message dialog</param>
    /// <returns>Itself</returns>
    public MessageDialogBuilder SetTitle(string title)
    {
        if (title == null)
        {
            title = "";
        }
        _title = title;
        return this;
    }

    /// <summary>
    /// Sets the main text of the MessageDialog
    /// </summary>
    /// <param name="text">Main text of the MessageDialog</param>
    /// <returns>Itself</returns>
    public MessageDialogBuilder SetText(string text)
    {
        if (text == null)
        {
            text = "";
        }
        _text = text;
        return this;
    }

    /// <summary>
    /// Assigns a set of extra window hints that you want the built dialog to have
    /// </summary>
    /// <param name="extraWindowHints">Window hints to assign to the window in addition to the ones the builder will put</param>
    /// <returns>Itself</returns>
    public MessageDialogBuilder SetExtraWindowHints(ICollection<IWindow.Hint> extraWindowHints)
    {
        _extraWindowHints.Clear();
        foreach (var hint in extraWindowHints)
        {
            _extraWindowHints.Add(hint);
        }
        return this;
    }

    /// <summary>
    /// Adds a button to the dialog
    /// </summary>
    /// <param name="button">Button to add to the dialog</param>
    /// <returns>Itself</returns>
    public MessageDialogBuilder AddButton(MessageDialogButton button)
    {
        _buttons.Add(button);
        return this;
    }
}