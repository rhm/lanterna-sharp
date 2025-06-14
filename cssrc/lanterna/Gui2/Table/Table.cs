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
using Lanterna.Core;
using Lanterna.Input;

namespace Lanterna.Gui2.Table;

/// <summary>
/// The table class is an interactable component that displays a grid of cells containing data along with a header of
/// labels. It supports scrolling when the number of rows and/or columns gets too large to fit and also supports
/// user selection which is either row-based or cell-based. User will move the current selection by using the arrow keys
/// on the keyboard.
/// </summary>
/// <typeparam name="V">Type of data to store in the table cells, presented through ToString()</typeparam>
public class Table<V> : AbstractInteractableComponent<Table<V>>
{
    private TableModel<V> _tableModel;
    private TableModel<V>.IListener<V>? _tableModelListener;
    private ITableHeaderRenderer<V> _tableHeaderRenderer;
    private ITableCellRenderer<V> _tableCellRenderer;
    private Action? _selectAction;
    private bool _cellSelection;
    private int _visibleRows;
    private int _visibleColumns;
    private int _selectedRow;
    private int _selectedColumn;
    private bool _escapeByArrowKey;

    /// <summary>
    /// Creates a new Table with the number of columns as specified by the array of labels
    /// </summary>
    /// <param name="columnLabels">Creates one column per label in the array, must be more than one</param>
    public Table(params string[] columnLabels) : this(new TableModel<V>(columnLabels))
    {
    }

    /// <summary>
    /// Creates a new Table with the specified table model
    /// </summary>
    /// <param name="tableModel">Table model</param>
    public Table(TableModel<V> tableModel)
    {
        _tableHeaderRenderer = new DefaultTableHeaderRenderer<V>();
        _tableCellRenderer = new DefaultTableCellRenderer<V>();
        _tableModel = tableModel;

        _selectAction = null;
        _visibleColumns = 0;
        _visibleRows = 0;
        _cellSelection = false;
        _selectedRow = 0;
        _selectedColumn = -1;
        _escapeByArrowKey = true;

        _tableModelListener = new TableModelListener(this);
        _tableModel.AddListener(_tableModelListener);
    }

    /// <summary>
    /// Returns the underlying table model
    /// </summary>
    /// <returns>Underlying table model</returns>
    public TableModel<V> GetTableModel()
    {
        return _tableModel;
    }

    /// <summary>
    /// Updates the table with a new table model, effectively replacing the content of the table completely
    /// </summary>
    /// <param name="tableModel">New table model</param>
    /// <returns>Itself</returns>
    public Table<V> SetTableModel(TableModel<V> tableModel)
    {
        lock (this)
        {
            if (tableModel == null)
            {
                throw new ArgumentException("Cannot assign a null TableModel");
            }
            _tableModel.RemoveListener(_tableModelListener!);
            _tableModel = tableModel;
            _tableModelListener = new TableModelListener(this);
            _tableModel.AddListener(_tableModelListener);
            Invalidate();
            return this;
        }
    }

    /// <summary>
    /// Returns the TableCellRenderer used by this table when drawing cells
    /// </summary>
    /// <returns>TableCellRenderer used by this table when drawing cells</returns>
    public ITableCellRenderer<V> GetTableCellRenderer()
    {
        return _tableCellRenderer;
    }

    /// <summary>
    /// Replaces the TableCellRenderer used by this table when drawing cells
    /// </summary>
    /// <param name="tableCellRenderer">New TableCellRenderer to use</param>
    /// <returns>Itself</returns>
    public Table<V> SetTableCellRenderer(ITableCellRenderer<V> tableCellRenderer)
    {
        lock (this)
        {
            _tableCellRenderer = tableCellRenderer;
            Invalidate();
            return this;
        }
    }

    /// <summary>
    /// Returns the TableHeaderRenderer used by this table when drawing the table's header
    /// </summary>
    /// <returns>TableHeaderRenderer used by this table when drawing the table's header</returns>
    public ITableHeaderRenderer<V> GetTableHeaderRenderer()
    {
        return _tableHeaderRenderer;
    }

    /// <summary>
    /// Replaces the TableHeaderRenderer used by this table when drawing the table's header
    /// </summary>
    /// <param name="tableHeaderRenderer">New TableHeaderRenderer to use</param>
    /// <returns>Itself</returns>
    public Table<V> SetTableHeaderRenderer(ITableHeaderRenderer<V> tableHeaderRenderer)
    {
        lock (this)
        {
            _tableHeaderRenderer = tableHeaderRenderer;
            Invalidate();
            return this;
        }
    }

    /// <summary>
    /// Sets the number of columns this table should show. If there are more columns in the table model, a scrollbar will
    /// be used to allow the user to scroll left and right and view all columns.
    /// </summary>
    /// <param name="visibleColumns">Number of columns to display at once</param>
    public void SetVisibleColumns(int visibleColumns)
    {
        lock (this)
        {
            _visibleColumns = visibleColumns;
            Invalidate();
        }
    }

    /// <summary>
    /// Returns the number of columns this table will show. If there are more columns in the table model, a scrollbar
    /// will be used to allow the user to scroll left and right and view all columns.
    /// </summary>
    /// <returns>Number of visible columns for this table</returns>
    public int GetVisibleColumns()
    {
        return _visibleColumns;
    }

    /// <summary>
    /// Sets the number of rows this table will show. If there are more rows in the table model, a scrollbar will be used
    /// to allow the user to scroll up and down and view all rows.
    /// </summary>
    /// <param name="visibleRows">Number of rows to display at once</param>
    public void SetVisibleRows(int visibleRows)
    {
        lock (this)
        {
            _visibleRows = visibleRows;
            Invalidate();
        }
    }

    /// <summary>
    /// Returns the number of rows this table will show. If there are more rows in the table model, a scrollbar will be
    /// used to allow the user to scroll up and down and view all rows.
    /// </summary>
    /// <returns>Number of rows to display at once</returns>
    public int GetVisibleRows()
    {
        return _visibleRows;
    }

    /// <summary>
    /// Returns the index of the first row that is currently visible.
    /// </summary>
    /// <returns>the index of the first row that is currently visible</returns>
    public int GetFirstViewedRowIndex()
    {
        return GetRenderer().GetViewTopRow();
    }

    /// <summary>
    /// Returns the index of the last row that is currently visible.
    /// </summary>
    /// <returns>the index of the last row that is currently visible</returns>
    public int GetLastViewedRowIndex()
    {
        int visibleRows = GetRenderer().GetVisibleRowsOnLastDraw();
        return Math.Min(GetRenderer().GetViewTopRow() + visibleRows - 1, _tableModel.GetRowCount() - 1);
    }

    /// <summary>
    /// Returns the currently selection column index, if in cell-selection mode. Otherwise it returns -1.
    /// </summary>
    /// <returns>In cell-selection mode returns the index of the selected column, otherwise -1</returns>
    public int GetSelectedColumn()
    {
        return _selectedColumn;
    }

    /// <summary>
    /// If in cell selection mode, updates which column is selected and ensures the selected column is visible in the
    /// view. If not in cell selection mode, does nothing.
    /// </summary>
    /// <param name="selectedColumn">Index of the column that should be selected</param>
    /// <returns>Itself</returns>
    public Table<V> SetSelectedColumn(int selectedColumn)
    {
        lock (this)
        {
            if (_cellSelection)
            {
                _selectedColumn = selectedColumn;
            }
            return this;
        }
    }

    /// <summary>
    /// Returns the index of the currently selected row
    /// </summary>
    /// <returns>Index of the currently selected row</returns>
    public int GetSelectedRow()
    {
        return _selectedRow;
    }

    /// <summary>
    /// Sets the index of the selected row and ensures the selected row is visible in the view
    /// </summary>
    /// <param name="selectedRow">Index of the row to select</param>
    /// <returns>Itself</returns>
    public Table<V> SetSelectedRow(int selectedRow)
    {
        lock (this)
        {
            if (selectedRow < 0)
            {
                throw new ArgumentException($"selectedRow must be >= 0 but was {selectedRow}");
            }
            int rowCount = _tableModel.GetRowCount();
            if (rowCount == 0)
            {
                selectedRow = 0;
            }
            else if (selectedRow > rowCount - 1)
            {
                selectedRow = rowCount - 1;
            }
            _selectedRow = selectedRow;
            return this;
        }
    }

    /// <summary>
    /// If true, the user will be able to select and navigate individual cells, otherwise the user can only
    /// select full rows.
    /// </summary>
    /// <param name="cellSelection">true if cell selection should be enabled, false for row selection</param>
    /// <returns>Itself</returns>
    public Table<V> SetCellSelection(bool cellSelection)
    {
        lock (this)
        {
            _cellSelection = cellSelection;
            if (cellSelection && _selectedColumn == -1)
            {
                _selectedColumn = 0;
            }
            else if (!cellSelection)
            {
                _selectedColumn = -1;
            }
            return this;
        }
    }

    /// <summary>
    /// Returns true if this table is in cell-selection mode, otherwise false
    /// </summary>
    /// <returns>true if this table is in cell-selection mode, otherwise false</returns>
    public bool IsCellSelection()
    {
        return _cellSelection;
    }

    /// <summary>
    /// Assigns an action to run whenever the user presses the enter or space key while focused on the table. If called with
    /// null, no action will be run.
    /// </summary>
    /// <param name="selectAction">Action to perform when user presses the enter or space key</param>
    /// <returns>Itself</returns>
    public Table<V> SetSelectAction(Action? selectAction)
    {
        lock (this)
        {
            _selectAction = selectAction;
            return this;
        }
    }

    /// <summary>
    /// Returns true if this table can be navigated away from when the selected row is at one of the extremes and
    /// the user presses the array key to continue in that direction.
    /// </summary>
    /// <returns>true if user can switch focus away from the table using arrow keys, false otherwise</returns>
    public bool IsEscapeByArrowKey()
    {
        return _escapeByArrowKey;
    }

    /// <summary>
    /// Sets the flag for if this table can be navigated away from when the selected row is at one of the extremes and
    /// the user presses the array key to continue in that direction.
    /// </summary>
    /// <param name="escapeByArrowKey">true if user can switch focus away from the table using arrow keys, false otherwise</param>
    /// <returns>Itself</returns>
    public Table<V> SetEscapeByArrowKey(bool escapeByArrowKey)
    {
        lock (this)
        {
            _escapeByArrowKey = escapeByArrowKey;
            return this;
        }
    }

    protected override IInteractableRenderer<Table<V>> CreateDefaultRenderer()
    {
        return new DefaultTableRenderer<V>();
    }

    public new ITableRenderer<V> GetRenderer()
    {
        return (ITableRenderer<V>)Renderer;
    }

    protected override IInteractable.Result HandleKeyStroke(KeyStroke keyStroke)
    {
        switch (keyStroke.KeyType)
        {
            case KeyType.ArrowUp:
                if (_selectedRow > 0)
                {
                    _selectedRow--;
                }
                else if (_escapeByArrowKey)
                {
                    return IInteractable.Result.MoveFocusUp;
                }
                break;
            case KeyType.ArrowDown:
                if (_selectedRow < _tableModel.GetRowCount() - 1)
                {
                    _selectedRow++;
                }
                else if (_escapeByArrowKey)
                {
                    return IInteractable.Result.MoveFocusDown;
                }
                break;
            case KeyType.PageUp:
                if (GetRenderer().GetVisibleRowsOnLastDraw() > 0 && _selectedRow > 0)
                {
                    _selectedRow -= Math.Min(GetRenderer().GetVisibleRowsOnLastDraw() - 1, _selectedRow);
                }
                break;
            case KeyType.PageDown:
                if (GetRenderer().GetVisibleRowsOnLastDraw() > 0 && _selectedRow < _tableModel.GetRowCount() - 1)
                {
                    int toEndDistance = _tableModel.GetRowCount() - 1 - _selectedRow;
                    _selectedRow += Math.Min(GetRenderer().GetVisibleRowsOnLastDraw() - 1, toEndDistance);
                }
                break;
            case KeyType.Home:
                _selectedRow = 0;
                break;
            case KeyType.End:
                _selectedRow = _tableModel.GetRowCount() - 1;
                break;
            case KeyType.ArrowLeft:
                if (_cellSelection && _selectedColumn > 0)
                {
                    _selectedColumn--;
                }
                else if (_escapeByArrowKey)
                {
                    return IInteractable.Result.MoveFocusLeft;
                }
                break;
            case KeyType.ArrowRight:
                if (_cellSelection && _selectedColumn < _tableModel.GetColumnCount() - 1)
                {
                    _selectedColumn++;
                }
                else if (_escapeByArrowKey)
                {
                    return IInteractable.Result.MoveFocusRight;
                }
                break;
            case KeyType.Character:
            case KeyType.Enter:
                if (IsKeyboardActivationStroke(keyStroke))
                {
                    var runnable = _selectAction;
                    if (runnable != null)
                    {
                        runnable();
                    }
                    else
                    {
                        return IInteractable.Result.Handled;
                    }
                    break;
                }
                else
                {
                    return base.HandleKeyStroke(keyStroke);
                }
            case KeyType.MouseEvent:
                var action = (MouseAction)keyStroke;
                var actionType = action.ActionType;
                if (actionType == MouseActionType.Move)
                {
                    return IInteractable.Result.Unhandled;
                }
                if (!IsFocused)
                {
                    base.HandleKeyStroke(keyStroke);
                }
                int mouseRow = GetRowByMouseAction(action);
                int mouseColumn = GetColumnByMouseAction(action);
                bool isDifferentCell = mouseRow != _selectedRow || mouseColumn != _selectedColumn;
                _selectedRow = mouseRow;
                _selectedColumn = mouseColumn;
                if (isDifferentCell)
                {
                    return HandleKeyStroke(new KeyStroke(KeyType.Enter));
                }
                break;
            default:
                return base.HandleKeyStroke(keyStroke);
        }
        Invalidate();
        return IInteractable.Result.Handled;
    }

    /// <summary>
    /// Gets row clicked on by mouse action.
    /// </summary>
    /// <param name="mouseAction">Mouse action</param>
    /// <returns>row of a table that was clicked on with MouseAction</returns>
    protected int GetRowByMouseAction(MouseAction mouseAction)
    {
        int minPossible = GetFirstViewedRowIndex();
        int maxPossible = GetLastViewedRowIndex();
        int mouseSpecified = mouseAction.Position.Row - GlobalPosition.Row - 1;
        
        return Math.Max(minPossible, Math.Min(mouseSpecified, maxPossible));
    }

    /// <summary>
    /// Gets column clicked on by mouse action.
    /// </summary>
    /// <param name="mouseAction">Mouse action</param>
    /// <returns>column of a table that was clicked on with MouseAction</returns>
    protected int GetColumnByMouseAction(MouseAction mouseAction)
    {
        int maxColumnIndex = _tableModel.GetColumnCount() - 1;
        int column = 0;
        int columnSize = _tableHeaderRenderer.GetPreferredSize(this, _tableModel.GetColumnLabel(column), column).Columns;
        int globalColumnMoused = mouseAction.Position.Column - GlobalPosition.Column;
        while (globalColumnMoused - columnSize - 1 >= 0 && column < maxColumnIndex)
        {
            globalColumnMoused -= columnSize;
            column++;
            columnSize = _tableHeaderRenderer.GetPreferredSize(this, _tableModel.GetColumnLabel(column), column).Columns;
        }
        return column;
    }

    private class TableModelListener : TableModel<V>.IListener<V>
    {
        private readonly Table<V> _table;

        public TableModelListener(Table<V> table)
        {
            _table = table;
        }

        public void OnRowAdded(TableModel<V> model, int index)
        {
            if (index <= _table._selectedRow)
            {
                _table._selectedRow = Math.Min(model.GetRowCount() - 1, _table._selectedRow + 1);
            }
            _table.Invalidate();
        }

        public void OnRowRemoved(TableModel<V> model, int index, IList<V> oldRow)
        {
            if (index < _table._selectedRow)
            {
                _table._selectedRow = Math.Max(0, _table._selectedRow - 1);
            }
            else
            {
                int rowCount = model.GetRowCount();
                if (_table._selectedRow > rowCount - 1)
                {
                    _table._selectedRow = Math.Max(0, rowCount - 1);
                }
            }
            _table.Invalidate();
        }

        public void OnColumnAdded(TableModel<V> model, int index)
        {
            _table.Invalidate();
        }

        public void OnColumnRemoved(TableModel<V> model, int index, string oldHeader, IList<V> oldColumn)
        {
            _table.Invalidate();
        }

        public void OnCellChanged(TableModel<V> model, int row, int column, V? oldValue, V? newValue)
        {
            _table.Invalidate();
        }
    }
}

