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
/// Abstract class for dialog building, containing much shared code between different kinds of dialogs
/// </summary>
/// <typeparam name="B">The real type of the builder class</typeparam>
/// <typeparam name="T">Type of dialog this builder is building</typeparam>
public abstract class AbstractDialogBuilder<B, T> where B : AbstractDialogBuilder<B, T> where T : DialogWindow
{
    protected string _title;
    protected string? _description;
    protected ISet<IWindow.Hint> _extraWindowHints;

    /// <summary>
    /// Default constructor for a dialog builder
    /// </summary>
    /// <param name="title">Title to assign to the dialog</param>
    public AbstractDialogBuilder(string title)
    {
        _title = title;
        _description = null;
        _extraWindowHints = new HashSet<IWindow.Hint> { IWindow.Hint.Centered };
    }

    /// <summary>
    /// Changes the title of the dialog
    /// </summary>
    /// <param name="title">New title</param>
    /// <returns>Itself</returns>
    public B SetTitle(string title)
    {
        if (title == null)
        {
            title = "";
        }
        _title = title;
        return Self();
    }

    /// <summary>
    /// Returns the title that the built dialog will have
    /// </summary>
    /// <returns>Title that the built dialog will have</returns>
    public string GetTitle()
    {
        return _title;
    }

    /// <summary>
    /// Changes the description of the dialog
    /// </summary>
    /// <param name="description">New description</param>
    /// <returns>Itself</returns>
    public B SetDescription(string? description)
    {
        _description = description;
        return Self();
    }

    /// <summary>
    /// Returns the description that the built dialog will have
    /// </summary>
    /// <returns>Description that the built dialog will have</returns>
    public string? GetDescription()
    {
        return _description;
    }

    /// <summary>
    /// Assigns a set of extra window hints that you want the built dialog to have
    /// </summary>
    /// <param name="extraWindowHints">Window hints to assign to the window in addition to the ones the builder will put</param>
    /// <returns>Itself</returns>
    public B SetExtraWindowHints(ISet<IWindow.Hint> extraWindowHints)
    {
        _extraWindowHints = extraWindowHints;
        return Self();
    }

    /// <summary>
    /// Returns the list of extra window hints that will be assigned to the window when built
    /// </summary>
    /// <returns>List of extra window hints that will be assigned to the window when built</returns>
    public ISet<IWindow.Hint> GetExtraWindowHints()
    {
        return _extraWindowHints;
    }

    /// <summary>
    /// Helper method for casting this to type parameter B
    /// </summary>
    /// <returns>this as B</returns>
    protected abstract B Self();

    /// <summary>
    /// Builds the dialog according to the builder implementation
    /// </summary>
    /// <returns>New dialog object</returns>
    protected abstract T BuildDialog();

    /// <summary>
    /// Builds a new dialog following the specifications of this builder
    /// </summary>
    /// <returns>New dialog built following the specifications of this builder</returns>
    public T Build()
    {
        T dialog = BuildDialog();
        if (_extraWindowHints.Count > 0)
        {
            var combinedHints = new HashSet<IWindow.Hint>(dialog.Hints);
            foreach (var hint in _extraWindowHints)
            {
                combinedHints.Add(hint);
            }
            dialog.SetHints(combinedHints);
        }
        return dialog;
    }
}