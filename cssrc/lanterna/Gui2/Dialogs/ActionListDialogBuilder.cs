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
/// Dialog builder for the ActionListDialog class, use this to create instances of that class and to customize
/// them
/// </summary>
public class ActionListDialogBuilder : AbstractDialogBuilder<ActionListDialogBuilder, ActionListDialog>
{
    private readonly List<Action> _actions;
    private TerminalSize? _listBoxSize;
    private bool _canCancel;
    private bool _closeAutomatically;

    /// <summary>
    /// Default constructor
    /// </summary>
    public ActionListDialogBuilder() : base("ActionListDialogBuilder")
    {
        _listBoxSize = null;
        _canCancel = true;
        _closeAutomatically = true;
        _actions = new List<Action>();
    }

    protected override ActionListDialogBuilder Self()
    {
        return this;
    }

    protected override ActionListDialog BuildDialog()
    {
        return new ActionListDialog(
            _title,
            _description,
            _listBoxSize,
            _canCancel,
            _closeAutomatically,
            _actions);
    }

    /// <summary>
    /// Sets the size of the internal ActionListBox in columns and rows, forcing scrollbars to appear if the
    /// space isn't big enough to contain all the items
    /// </summary>
    /// <param name="listBoxSize">Size of the ActionListBox</param>
    /// <returns>Itself</returns>
    public ActionListDialogBuilder SetListBoxSize(TerminalSize listBoxSize)
    {
        _listBoxSize = listBoxSize;
        return this;
    }

    /// <summary>
    /// Returns the specified size of the internal ActionListBox or null if there is no size and the list
    /// box will attempt to take up enough size to draw all items
    /// </summary>
    /// <returns>Specified size of the internal ActionListBox or null if there is no size</returns>
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
    public ActionListDialogBuilder SetCanCancel(bool canCancel)
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
    /// Adds an additional action to the ActionListBox that is to be displayed when the dialog is opened
    /// </summary>
    /// <param name="label">Label of the new action</param>
    /// <param name="action">Action to perform if the user selects this item</param>
    /// <returns>Itself</returns>
    public ActionListDialogBuilder AddAction(string label, Action action)
    {
        return AddAction(new LabeledAction(label, action));
    }

    /// <summary>
    /// Adds an additional action to the ActionListBox that is to be displayed when the dialog is opened. The
    /// label of this item will be derived by calling ToString() on the action
    /// </summary>
    /// <param name="action">Action to perform if the user selects this item</param>
    /// <returns>Itself</returns>
    public ActionListDialogBuilder AddAction(Action action)
    {
        _actions.Add(action);
        return this;
    }

    /// <summary>
    /// Adds additional actions to the ActionListBox that is to be displayed when the dialog is opened. The
    /// label of the items will be derived by calling ToString() on each action
    /// </summary>
    /// <param name="actions">Items to add to the ActionListBox</param>
    /// <returns>Itself</returns>
    public ActionListDialogBuilder AddActions(params Action[] actions)
    {
        _actions.AddRange(actions);
        return this;
    }

    /// <summary>
    /// Returns a copy of the internal list of actions currently inside this builder that will be assigned to the
    /// ActionListBox in the dialog when built
    /// </summary>
    /// <returns>Copy of the internal list of actions currently inside this builder</returns>
    public IList<Action> GetActions()
    {
        return new List<Action>(_actions);
    }

    /// <summary>
    /// Sets if clicking on an action automatically closes the dialog after the action is finished (default: true)
    /// </summary>
    /// <param name="closeAutomatically">if true dialog will be automatically closed after choosing and finish any of the action</param>
    /// <returns>Itself</returns>
    public ActionListDialogBuilder SetCloseAutomaticallyOnAction(bool closeAutomatically)
    {
        _closeAutomatically = closeAutomatically;
        return this;
    }

    /// <summary>
    /// Helper class to wrap an action with a custom label
    /// </summary>
    private class LabeledAction
    {
        private readonly string _label;
        private readonly Action _action;

        public LabeledAction(string label, Action action)
        {
            _label = label;
            _action = action;
        }

        public void Invoke()
        {
            _action();
        }

        public override string ToString()
        {
            return _label;
        }

        public static implicit operator Action(LabeledAction labeledAction)
        {
            return labeledAction.Invoke;
        }
    }
}