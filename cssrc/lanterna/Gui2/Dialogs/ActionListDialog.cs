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
/// Dialog containing a multiple item action list box
/// </summary>
public class ActionListDialog : DialogWindow
{
    internal ActionListDialog(
        string title,
        string? description,
        TerminalSize? actionListPreferredSize,
        bool canCancel,
        bool closeAutomatically,
        IList<Action> actions) : base(title)
    {
        var listBox = new ActionListBox(actionListPreferredSize);
        foreach (var action in actions)
        {
            var capturedAction = action; // Capture for closure
            listBox.AddItem(action.ToString() ?? "Action", () =>
            {
                capturedAction();
                if (closeAutomatically)
                {
                    Close();
                }
            });
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

    private void OnCancel()
    {
        Close();
    }

    /// <summary>
    /// Helper method for immediately displaying a ActionListDialog, the method will return when the dialog is
    /// closed
    /// </summary>
    /// <param name="textGUI">Text GUI the dialog should be added to</param>
    /// <param name="title">Title of the dialog</param>
    /// <param name="description">Description of the dialog</param>
    /// <param name="items">Items in the ActionListBox, the label will be taken from each Action by calling
    /// ToString() on each one</param>
    public static void ShowDialog(IWindowBasedTextGUI textGUI, string title, string? description, params Action[] items)
    {
        var actionListDialog = new ActionListDialogBuilder()
            .SetTitle(title)
            .SetDescription(description)
            .AddActions(items)
            .Build();
        actionListDialog.ShowDialog(textGUI);
    }

    protected override IWindow Self => this;
}