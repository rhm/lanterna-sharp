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
/// Dialog builder for the ListSelectDialog class, use this to create instances of that class and to customize
/// them
/// </summary>
/// <typeparam name="T">Type of items in the dialog</typeparam>
public class ListSelectDialogBuilder<T> : AbstractDialogBuilder<ListSelectDialogBuilder<T>, ListSelectDialog<T>>
{
    private readonly List<T> _content;
    private TerminalSize? _listBoxSize;
    private bool _canCancel;

    /// <summary>
    /// Default constructor
    /// </summary>
    public ListSelectDialogBuilder() : base("ListSelectDialog")
    {
        _listBoxSize = null;
        _canCancel = true;
        _content = new List<T>();
    }

    protected override ListSelectDialogBuilder<T> Self()
    {
        return this;
    }

    protected override ListSelectDialog<T> BuildDialog()
    {
        return new ListSelectDialog<T>(
            _title,
            _description,
            _listBoxSize,
            _canCancel,
            _content);
    }

    /// <summary>
    /// Sets the size of the list box in the dialog, scrollbars will be used if there is not enough space to draw all
    /// items. If set to null, the dialog will ask for enough space to be able to draw all items.
    /// </summary>
    /// <param name="listBoxSize">Size of the list box in the dialog</param>
    /// <returns>Itself</returns>
    public ListSelectDialogBuilder<T> SetListBoxSize(TerminalSize? listBoxSize)
    {
        _listBoxSize = listBoxSize;
        return this;
    }

    /// <summary>
    /// Size of the list box in the dialog or null if the dialog will ask for enough space to draw all items
    /// </summary>
    /// <returns>Size of the list box in the dialog or null if the dialog will ask for enough space to draw all items</returns>
    public TerminalSize? GetListBoxSize()
    {
        return _listBoxSize;
    }

    /// <summary>
    /// Sets if the dialog can be cancelled or not (default: true)
    /// </summary>
    /// <param name="canCancel">If true, the user has the option to cancel the dialog, if false there is no such
    /// button in the dialog</param>
    /// <returns>Itself</returns>
    public ListSelectDialogBuilder<T> SetCanCancel(bool canCancel)
    {
        _canCancel = canCancel;
        return this;
    }

    /// <summary>
    /// Returns true if the dialog can be cancelled once it's opened
    /// </summary>
    /// <returns>true if the dialog can be cancelled once it's opened</returns>
    public bool IsCanCancel()
    {
        return _canCancel;
    }

    /// <summary>
    /// Adds an item to the list box at the end
    /// </summary>
    /// <param name="item">Item to add to the list box</param>
    /// <returns>Itself</returns>
    public ListSelectDialogBuilder<T> AddListItem(T item)
    {
        _content.Add(item);
        return this;
    }

    /// <summary>
    /// Adds a list of items to the list box at the end, in the order they are passed in
    /// </summary>
    /// <param name="items">Items to add to the list box</param>
    /// <returns>Itself</returns>
    public ListSelectDialogBuilder<T> AddListItems(params T[] items)
    {
        _content.AddRange(items);
        return this;
    }

    /// <summary>
    /// Returns a copy of the list of items in the list box
    /// </summary>
    /// <returns>Copy of the list of items in the list box</returns>
    public IList<T> GetListItems()
    {
        return new List<T>(_content);
    }
}