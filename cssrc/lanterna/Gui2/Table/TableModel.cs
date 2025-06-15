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
namespace Lanterna.Gui2.Table;

/// <summary>
/// A TableModel contains the data model behind a table, here is where all the action cell values and header
/// labels are stored.
/// </summary>
/// <typeparam name="V">Value type stored in the table</typeparam>
public class TableModel<V>
{
    /// <summary>
    /// Listener interface for the TableModel class which can be attached to a TableModel to be notified
    /// of changes to the table model.
    /// </summary>
    /// <typeparam name="T">Value type stored in the table</typeparam>
    public interface IListener<T>
    {
        /// <summary>
        /// Called when a new row has been added to the model
        /// </summary>
        /// <param name="model">Model the row was added to</param>
        /// <param name="index">Index of the new row</param>
        void OnRowAdded(TableModel<T> model, int index);

        /// <summary>
        /// Called when a row has been removed from the model
        /// </summary>
        /// <param name="model">Model the row was removed from</param>
        /// <param name="index">Index of the removed row</param>
        /// <param name="oldRow">Content of the row that was removed</param>
        void OnRowRemoved(TableModel<T> model, int index, IList<T> oldRow);

        /// <summary>
        /// Called when a new column has been added to the model
        /// </summary>
        /// <param name="model">Model the column was added to</param>
        /// <param name="index">Index of the new column</param>
        void OnColumnAdded(TableModel<T> model, int index);

        /// <summary>
        /// Called when a column has been removed from the model
        /// </summary>
        /// <param name="model">Model the column was removed from</param>
        /// <param name="index">Index of the removed column</param>
        /// <param name="oldHeader">Header the removed column had</param>
        /// <param name="oldColumn">Values in the removed column</param>
        void OnColumnRemoved(TableModel<T> model, int index, string oldHeader, IList<T> oldColumn);

        /// <summary>
        /// Called when an existing cell had its content updated
        /// </summary>
        /// <param name="model">Model that was modified</param>
        /// <param name="row">Row index of the modified cell</param>
        /// <param name="column">Column index of the modified cell</param>
        /// <param name="oldValue">Previous value of the cell</param>
        /// <param name="newValue">New value of the cell</param>
        void OnCellChanged(TableModel<T> model, int row, int column, T? oldValue, T? newValue);
    }

    private readonly List<string> _columns;
    private readonly List<List<V?>> _rows;
    private readonly List<IListener<V>> _listeners;

    /// <summary>
    /// Default constructor, creates a new model with same number of columns as labels supplied
    /// </summary>
    /// <param name="columnLabels">Variable number of labels for the column headers</param>
    public TableModel(params string[] columnLabels) : this((IList<string>)columnLabels)
    {
    }

    /// <summary>
    /// Creates a new model with same number of columns as labels supplied
    /// </summary>
    /// <param name="columnLabels">List of labels for the column headers</param>
    public TableModel(IList<string> columnLabels)
    {
        if (columnLabels.Count == 0)
        {
            throw new ArgumentException("Table model needs at least one column");
        }
        _columns = new List<string>(columnLabels);
        _rows = new List<List<V?>>();
        _listeners = new List<IListener<V>>();
    }

    /// <summary>
    /// Returns the number of columns in the model
    /// </summary>
    /// <returns>Number of columns in the model</returns>
    public int GetColumnCount()
    {
        lock (this)
        {
            return _columns.Count;
        }
    }

    /// <summary>
    /// Returns number of rows in the model
    /// </summary>
    /// <returns>Number of rows in the model</returns>
    public int GetRowCount()
    {
        lock (this)
        {
            return _rows.Count;
        }
    }

    /// <summary>
    /// Returns all rows in the model as a list of lists containing the data as elements
    /// </summary>
    /// <returns>All rows in the model as a list of lists containing the data as elements</returns>
    public IList<IList<V?>> GetRows()
    {
        lock (this)
        {
            var copy = new List<IList<V?>>();
            foreach (var row in _rows)
            {
                copy.Add(new List<V?>(row));
            }
            return copy;
        }
    }

    /// <summary>
    /// Returns all column header label as a list of strings
    /// </summary>
    /// <returns>All column header label as a list of strings</returns>
    public IList<string> GetColumnLabels()
    {
        lock (this)
        {
            return new List<string>(_columns);
        }
    }

    /// <summary>
    /// Returns a row from the table as a list of the cell data
    /// </summary>
    /// <param name="index">Index of the row to return</param>
    /// <returns>Row from the table as a list of the cell data</returns>
    public IList<V?> GetRow(int index)
    {
        lock (this)
        {
            return new List<V?>(_rows[index]);
        }
    }

    /// <summary>
    /// Returns a column from the table as a list of the cell data
    /// </summary>
    /// <param name="index">Index of the column to return</param>
    /// <returns>Column from the table as a list of the cell data</returns>
    public IList<V?> GetColumn(int index)
    {
        lock (this)
        {
            var columnData = new List<V?>();
            for (int row = 0; row < GetRowCount(); row++)
            {
                columnData.Add(GetCell(index, row));
            }
            return columnData;
        }
    }

    /// <summary>
    /// Adds a new row to the table model at the end. This may update the selection to make sure the same row is selected.
    /// </summary>
    /// <param name="values">Data to associate with the new row, mapped column by column in order</param>
    /// <returns>Itself</returns>
    public TableModel<V> AddRow(params V?[] values)
    {
        AddRow((ICollection<V?>)values);
        return this;
    }

    /// <summary>
    /// Adds a new row to the table model at the end. This may update the selection to make sure the same row is selected.
    /// </summary>
    /// <param name="values">Data to associate with the new row, mapped column by column in order</param>
    /// <returns>Itself</returns>
    public TableModel<V> AddRow(ICollection<V?> values)
    {
        InsertRow(GetRowCount(), values);
        return this;
    }

    /// <summary>
    /// Inserts a new row to the table model at a particular index. This may update the selection to make sure the same row is selected.
    /// </summary>
    /// <param name="index">Index the new row should have, 0 means the first row and row count will append the row at the end</param>
    /// <param name="values">Data to associate with the new row, mapped column by column in order</param>
    /// <returns>Itself</returns>
    public TableModel<V> InsertRow(int index, ICollection<V?> values)
    {
        lock (this)
        {
            var list = new List<V?>(values);
            _rows.Insert(index, list);
            foreach (var listener in _listeners)
            {
                listener.OnRowAdded(this, index);
            }
        }
        return this;
    }

    /// <summary>
    /// Removes a row at a particular index from the table model. This may update the selection to make sure the same row is selected.
    /// </summary>
    /// <param name="index">Index of the row to remove</param>
    /// <returns>Itself</returns>
    public TableModel<V> RemoveRow(int index)
    {
        lock (this)
        {
            var removedRow = _rows[index];
            _rows.RemoveAt(index);
            foreach (var listener in _listeners)
            {
                listener.OnRowRemoved(this, index, removedRow.OfType<V>().ToList());
            }
        }
        return this;
    }

    /// <summary>
    /// Removes all rows from the table, this will trigger listeners for each row
    /// </summary>
    /// <returns>Itself</returns>
    public TableModel<V> Clear()
    {
        lock (this)
        {
            while (_rows.Count > 0)
            {
                RemoveRow(0);
            }
        }
        return this;
    }

    /// <summary>
    /// Returns the label of a column header
    /// </summary>
    /// <param name="index">Index of the column to retrieve the header label for</param>
    /// <returns>Label of the column selected</returns>
    public string GetColumnLabel(int index)
    {
        lock (this)
        {
            return _columns[index];
        }
    }

    /// <summary>
    /// Updates the label of a column header
    /// </summary>
    /// <param name="index">Index of the column to update the header label for</param>
    /// <param name="newLabel">New label to assign to the column header</param>
    /// <returns>Itself</returns>
    public TableModel<V> SetColumnLabel(int index, string newLabel)
    {
        lock (this)
        {
            _columns[index] = newLabel;
        }
        return this;
    }

    /// <summary>
    /// Adds a new column into the table model as the last column. You can optionally supply values for the existing rows
    /// through the newColumnValues.
    /// </summary>
    /// <param name="label">Label for the header of the new column</param>
    /// <param name="newColumnValues">Optional values to assign to the existing rows, where the first element in the array will be the value of the first row and so on...</param>
    /// <returns>Itself</returns>
    public TableModel<V> AddColumn(string label, V?[]? newColumnValues = null)
    {
        return InsertColumn(GetColumnCount(), label, newColumnValues);
    }

    /// <summary>
    /// Adds a new column into the table model at a specified index. You can optionally supply values for the existing
    /// rows through the newColumnValues.
    /// </summary>
    /// <param name="index">Index for the new column</param>
    /// <param name="label">Label for the header of the new column</param>
    /// <param name="newColumnValues">Optional values to assign to the existing rows, where the first element in the array will be the value of the first row and so on...</param>
    /// <returns>Itself</returns>
    public TableModel<V> InsertColumn(int index, string label, V?[]? newColumnValues = null)
    {
        lock (this)
        {
            _columns.Insert(index, label);
            for (int i = 0; i < _rows.Count; i++)
            {
                var row = _rows[i];

                // Pad row with null if necessary
                for (int j = row.Count; j < index; j++)
                {
                    row.Add(default(V));
                }

                if (newColumnValues != null && i < newColumnValues.Length && newColumnValues[i] != null)
                {
                    row.Insert(index, newColumnValues[i]);
                }
                else
                {
                    row.Insert(index, default(V));
                }
            }

            foreach (var listener in _listeners)
            {
                listener.OnColumnAdded(this, index);
            }
        }
        return this;
    }

    /// <summary>
    /// Removes a column from the table model
    /// </summary>
    /// <param name="index">Index of the column to remove</param>
    /// <returns>Itself</returns>
    public TableModel<V> RemoveColumn(int index)
    {
        lock (this)
        {
            string removedColumnHeader = _columns[index];
            _columns.RemoveAt(index);
            var removedColumn = new List<V?>();
            foreach (var row in _rows)
            {
                removedColumn.Add(row[index]);
                row.RemoveAt(index);
            }
            foreach (var listener in _listeners)
            {
                listener.OnColumnRemoved(this, index, removedColumnHeader, removedColumn.OfType<V>().ToList());
            }
        }
        return this;
    }

    /// <summary>
    /// Returns the cell value stored at a specific column/row coordinate.
    /// </summary>
    /// <param name="columnIndex">Column index of the cell</param>
    /// <param name="rowIndex">Row index of the cell</param>
    /// <returns>The data value stored in this cell</returns>
    public V? GetCell(int columnIndex, int rowIndex)
    {
        lock (this)
        {
            if (rowIndex < 0 || columnIndex < 0)
            {
                throw new IndexOutOfRangeException($"Invalid row or column index: {rowIndex} {columnIndex}");
            }
            else if (rowIndex >= GetRowCount())
            {
                throw new IndexOutOfRangeException($"TableModel has {GetRowCount()} rows, invalid access at rowIndex {rowIndex}");
            }
            if (columnIndex >= GetColumnCount())
            {
                throw new IndexOutOfRangeException($"TableModel has {GetColumnCount()} columns, invalid access at columnIndex {columnIndex}");
            }
            return _rows[rowIndex][columnIndex];
        }
    }

    /// <summary>
    /// Updates the cell value stored at a specific column/row coordinate.
    /// </summary>
    /// <param name="columnIndex">Column index of the cell</param>
    /// <param name="rowIndex">Row index of the cell</param>
    /// <param name="value">New value to assign to the cell</param>
    /// <returns>Itself</returns>
    public TableModel<V> SetCell(int columnIndex, int rowIndex, V? value)
    {
        lock (this)
        {
            GetCell(columnIndex, rowIndex); // Validate indexes
            var row = _rows[rowIndex];

            // Pad row with null if necessary
            for (int j = row.Count; j < columnIndex; j++)
            {
                row.Add(default(V));
            }

            var existingValue = row[columnIndex];
            if (EqualityComparer<V?>.Default.Equals(existingValue, value))
            {
                return this;
            }
            row[columnIndex] = value;
            foreach (var listener in _listeners)
            {
                listener.OnCellChanged(this, rowIndex, columnIndex, existingValue, value);
            }
        }
        return this;
    }

    /// <summary>
    /// Adds a listener to this table model that will be notified whenever the model changes
    /// </summary>
    /// <param name="listener">Listener to register with this model</param>
    /// <returns>Itself</returns>
    public TableModel<V> AddListener(IListener<V> listener)
    {
        _listeners.Add(listener);
        return this;
    }

    /// <summary>
    /// Removes a listener from this model so that it will no longer receive any notifications when the model changes
    /// </summary>
    /// <param name="listener">Listener to deregister from this model</param>
    /// <returns>Itself</returns>
    public TableModel<V> RemoveListener(IListener<V> listener)
    {
        _listeners.Remove(listener);
        return this;
    }
}