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
/// Dialog that allows the user to select an item from a list
/// </summary>
/// <typeparam name="T">Type of elements in the list</typeparam>
public class ListSelectDialog<T> : DialogWindow
{
    private T? _result;

    internal ListSelectDialog(
        string title,
        string? description,
        TerminalSize? listBoxPreferredSize,
        bool canCancel,
        IList<T> content) : base(title)
    {
        _result = default(T);
        if (content.Count == 0)
        {
            throw new InvalidOperationException("ListSelectDialog needs at least one item");
        }

        var listBox = new ActionListBox(listBoxPreferredSize);
        foreach (var item in content)
        {
            var capturedItem = item; // Capture for closure
            listBox.AddItem(item?.ToString() ?? "", () => OnSelect(capturedItem));
        }

        var mainPanel = new Panel();
        mainPanel.SetLayoutManager(
            new GridLayout(1)
                .SetLeftMarginSize(1)
                .SetRightMarginSize(1));
        if (description != null)
        {
            mainPanel.AddComponent(new Label(description));
            mainPanel.AddComponent(new EmptySpace(TerminalSize.One));
        }
        listBox.SetLayoutData(
            GridLayout.CreateLayoutData(
                GridLayout.Alignment.Fill,
                GridLayout.Alignment.Center,
                true,
                false))
            .AddTo(mainPanel);
        mainPanel.AddComponent(new EmptySpace(TerminalSize.One));

        if (canCancel)
        {
            var buttonPanel = new Panel();
            buttonPanel.SetLayoutManager(new GridLayout(2).SetHorizontalSpacing(1));
            buttonPanel.AddComponent(new Button(LocalizedString.Cancel.ToString(), OnCancel).SetLayoutData(GridLayout.CreateLayoutData(GridLayout.Alignment.Center, GridLayout.Alignment.Center, true, false)));
            buttonPanel.SetLayoutData(
                GridLayout.CreateLayoutData(
                    GridLayout.Alignment.End,
                    GridLayout.Alignment.Center,
                    false,
                    false))
                .AddTo(mainPanel);
        }
        Component = mainPanel;
    }

    private void OnSelect(T item)
    {
        _result = item;
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
    /// <returns>The item in the list that was selected or null if the dialog was cancelled</returns>
    public override object? ShowDialog(IWindowBasedTextGUI textGUI)
    {
        _result = default(T);
        base.ShowDialog(textGUI);
        return _result;
    }

    /// <summary>
    /// Shortcut for quickly creating a new dialog
    /// </summary>
    /// <param name="textGUI">Text GUI to add the dialog to</param>
    /// <param name="title">Title of the dialog</param>
    /// <param name="description">Description of the dialog</param>
    /// <param name="items">Items in the dialog</param>
    /// <typeparam name="T">Type of items in the dialog</typeparam>
    /// <returns>The selected item or null if cancelled</returns>
    public static TItem? ShowDialog<TItem>(IWindowBasedTextGUI textGUI, string title, string? description, params TItem[] items)
    {
        return ShowDialog<TItem>(textGUI, title, description, (TerminalSize?)null, items);
    }

    /// <summary>
    /// Shortcut for quickly creating a new dialog
    /// </summary>
    /// <param name="textGUI">Text GUI to add the dialog to</param>
    /// <param name="title">Title of the dialog</param>
    /// <param name="description">Description of the dialog</param>
    /// <param name="listBoxHeight">Maximum height of the list box, scrollbars will be used if there are more items</param>
    /// <param name="items">Items in the dialog</param>
    /// <typeparam name="T">Type of items in the dialog</typeparam>
    /// <returns>The selected item or null if cancelled</returns>
    public static TItem? ShowDialog<TItem>(IWindowBasedTextGUI textGUI, string title, string? description, int listBoxHeight, params TItem[] items)
    {
        int width = 0;
        foreach (var item in items)
        {
            width = Math.Max(width, TerminalTextUtils.GetColumnWidth(item?.ToString() ?? ""));
        }
        width += 2;
        return ShowDialog<TItem>(textGUI, title, description, new TerminalSize(width, listBoxHeight), items);
    }

    /// <summary>
    /// Shortcut for quickly creating a new dialog
    /// </summary>
    /// <param name="textGUI">Text GUI to add the dialog to</param>
    /// <param name="title">Title of the dialog</param>
    /// <param name="description">Description of the dialog</param>
    /// <param name="listBoxSize">Maximum size of the list box, scrollbars will be used if the items cannot fit</param>
    /// <param name="items">Items in the dialog</param>
    /// <typeparam name="T">Type of items in the dialog</typeparam>
    /// <returns>The selected item or null if cancelled</returns>
    public static TItem? ShowDialog<TItem>(IWindowBasedTextGUI textGUI, string title, string? description, TerminalSize? listBoxSize, params TItem[] items)
    {
        var listSelectDialog = new ListSelectDialogBuilder<TItem>()
            .SetTitle(title)
            .SetDescription(description)
            .SetListBoxSize(listBoxSize)
            .AddListItems(items)
            .Build();
        return (TItem?)listSelectDialog.ShowDialog(textGUI);
    }
}