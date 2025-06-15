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
using Lanterna.Graphics;

namespace Lanterna.Gui2.Table;

/// <summary>
/// Default implementation of ITableRenderer with full scrolling and border support
/// </summary>
/// <typeparam name="V">Type of data stored in each table cell</typeparam>
public class DefaultTableRenderer<V> : ITableRenderer<V>
{
    private readonly ScrollBar _verticalScrollBar;
    private readonly ScrollBar _horizontalScrollBar;

    private TableCellBorderStyle _headerVerticalBorderStyle;
    private TableCellBorderStyle _headerHorizontalBorderStyle;
    private TableCellBorderStyle _cellVerticalBorderStyle;
    private TableCellBorderStyle _cellHorizontalBorderStyle;

    private int _viewTopRow;
    private int _viewLeftColumn;
    private int _visibleRowsOnLastDraw;

    private TerminalSize? _cachedSize;
    private readonly List<int> _preferredColumnSizes;
    private readonly List<int> _preferredRowSizes;
    private readonly HashSet<int> _expandableColumns;
    private int _headerSizeInRows;
    private bool _allowPartialColumn;
    private bool _scrollBarsHidden;

    public DefaultTableRenderer()
    {
        _verticalScrollBar = new ScrollBar(Direction.Vertical);
        _horizontalScrollBar = new ScrollBar(Direction.Horizontal);

        _headerVerticalBorderStyle = TableCellBorderStyle.None;
        _headerHorizontalBorderStyle = TableCellBorderStyle.EmptySpace;
        _cellVerticalBorderStyle = TableCellBorderStyle.None;
        _cellHorizontalBorderStyle = TableCellBorderStyle.EmptySpace;

        _viewTopRow = 0;
        _viewLeftColumn = 0;
        _visibleRowsOnLastDraw = 0;
        _allowPartialColumn = false;
        _scrollBarsHidden = false;

        _cachedSize = null;
        _preferredColumnSizes = new List<int>();
        _preferredRowSizes = new List<int>();
        _expandableColumns = new HashSet<int>();
        _headerSizeInRows = 0;
    }

    public bool IsScrollBarsHidden
    {
        get => _scrollBarsHidden;
        set => _scrollBarsHidden = value;
    }

    public bool AllowPartialColumn
    {
        get => _allowPartialColumn;
        set => _allowPartialColumn = value;
    }

    public int GetVisibleRowsOnLastDraw() => _visibleRowsOnLastDraw;
    
    public int GetViewTopRow() => _viewTopRow;
    
    public void SetViewTopRow(int viewTopRow) => _viewTopRow = Math.Max(0, viewTopRow);
    
    public int GetViewLeftColumn() => _viewLeftColumn;
    
    public void SetViewLeftColumn(int viewLeftColumn) => _viewLeftColumn = Math.Max(0, viewLeftColumn);

    public TerminalPosition? GetCursorLocation(Table<V> component) => null;

    public TerminalSize GetPreferredSize(Table<V> table)
    {
        // Quick bypass if the table hasn't changed
        if (!table.IsInvalid && _cachedSize != null)
        {
            return _cachedSize.Value;
        }

        var tableModel = table.GetTableModel();
        if (tableModel.GetColumnCount() == 0)
        {
            return TerminalSize.Zero;
        }

        CalculatePreferredSizes(table);
        return CalculateTotalPreferredSize(table);
    }

    private void CalculatePreferredSizes(Table<V> table)
    {
        var tableModel = table.GetTableModel();
        var tableHeaderRenderer = table.GetTableHeaderRenderer();
        var tableCellRenderer = table.GetTableCellRenderer();
        var rows = tableModel.GetRows();
        var columnHeaders = tableModel.GetColumnLabels();

        _preferredColumnSizes.Clear();
        _preferredRowSizes.Clear();

        // Calculate column sizes based on headers
        for (int columnIndex = 0; columnIndex < columnHeaders.Count; columnIndex++)
        {
            int columnSize = tableHeaderRenderer.GetPreferredSize(table, columnHeaders[columnIndex], columnIndex).Columns;
            _preferredColumnSizes.Add(columnSize);
        }

        // Calculate sizes based on cell content
        for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            var row = rows[rowIndex];
            int maxRowHeight = 1;
            
            for (int columnIndex = 0; columnIndex < row.Count && columnIndex < _preferredColumnSizes.Count; columnIndex++)
            {
                var cell = row[columnIndex];
                var cellSize = cell != null ? tableCellRenderer.GetPreferredSize(table, cell, columnIndex, rowIndex) : TerminalSize.Zero;
                
                // Update column width
                if (_preferredColumnSizes[columnIndex] < cellSize.Columns)
                {
                    _preferredColumnSizes[columnIndex] = cellSize.Columns;
                }
                
                // Track row height
                maxRowHeight = Math.Max(maxRowHeight, cellSize.Rows);
            }
            
            _preferredRowSizes.Add(maxRowHeight);
        }

        // Calculate header height
        _headerSizeInRows = 0;
        for (int columnIndex = 0; columnIndex < columnHeaders.Count; columnIndex++)
        {
            int headerRows = tableHeaderRenderer.GetPreferredSize(table, columnHeaders[columnIndex], columnIndex).Rows;
            _headerSizeInRows = Math.Max(_headerSizeInRows, headerRows);
        }
    }

    private TerminalSize CalculateTotalPreferredSize(Table<V> table)
    {
        var tableModel = table.GetTableModel();
        int visibleColumns = table.GetVisibleColumns();
        int visibleRows = table.GetVisibleRows();

        if (visibleColumns == 0) visibleColumns = tableModel.GetColumnCount();
        if (visibleRows == 0) visibleRows = tableModel.GetRowCount();

        int preferredWidth = 0;
        int preferredHeight = _headerSizeInRows;

        // Calculate width
        int columnsToInclude = Math.Min(_preferredColumnSizes.Count, visibleColumns);
        for (int i = 0; i < columnsToInclude; i++)
        {
            preferredWidth += _preferredColumnSizes[i];
        }

        // Calculate height
        int rowsToInclude = Math.Min(_preferredRowSizes.Count, visibleRows);
        for (int i = 0; i < rowsToInclude; i++)
        {
            preferredHeight += _preferredRowSizes[i];
        }

        // Add border spacing
        if (_headerVerticalBorderStyle != TableCellBorderStyle.None)
        {
            preferredHeight += _headerVerticalBorderStyle.GetSize();
        }
        if (_cellVerticalBorderStyle != TableCellBorderStyle.None && _preferredRowSizes.Count > 0)
        {
            preferredHeight += (_preferredRowSizes.Count - 1) * _cellVerticalBorderStyle.GetSize();
        }
        if (IsHorizontallySpaced() && _preferredColumnSizes.Count > 0)
        {
            preferredWidth += (_preferredColumnSizes.Count - 1);
        }

        // Add scrollbar space
        if (!_scrollBarsHidden)
        {
            preferredWidth++; // Vertical scrollbar
            if (visibleColumns < tableModel.GetColumnCount())
            {
                preferredHeight++; // Horizontal scrollbar
            }
        }

        _cachedSize = new TerminalSize(preferredWidth, preferredHeight);
        return _cachedSize.Value;
    }

    private bool IsHorizontallySpaced()
    {
        return _headerHorizontalBorderStyle != TableCellBorderStyle.None ||
               _cellHorizontalBorderStyle != TableCellBorderStyle.None;
    }

    public void DrawComponent(ITextGUIGraphics graphics, Table<V> table)
    {
        var area = graphics.Size;
        if (area.Rows == 0 || area.Columns == 0)
        {
            return;
        }

        var tableModel = table.GetTableModel();
        if (tableModel.GetRowCount() == 0 || tableModel.GetColumnCount() == 0)
        {
            graphics.Fill(' ');
            return;
        }

        // Get preferred size if table has changed
        if (table.IsInvalid)
        {
            GetPreferredSize(table);
        }

        UpdateViewport(table, area);
        DrawTable(graphics, table, area);
    }

    private void UpdateViewport(Table<V> table, TerminalSize area)
    {
        int selectedColumn = table.GetSelectedColumn();
        int selectedRow = table.GetSelectedRow();
        
        // Update viewport based on selection
        if (selectedColumn != -1 && _viewLeftColumn > selectedColumn)
        {
            _viewLeftColumn = selectedColumn;
        }
        if (_viewTopRow > selectedRow)
        {
            _viewTopRow = selectedRow;
        }
        if (_viewTopRow >= table.GetTableModel().GetRowCount())
        {
            _viewTopRow = Math.Max(0, table.GetTableModel().GetRowCount() - 1);
        }
    }

    private void DrawTable(ITextGUIGraphics graphics, Table<V> table, TerminalSize area)
    {
        var tableModel = table.GetTableModel();
        
        // Draw header
        DrawHeader(graphics, table);
        
        // Draw data rows
        var dataArea = graphics.NewTextGraphics(
            new TerminalPosition(0, _headerSizeInRows + (_headerVerticalBorderStyle != TableCellBorderStyle.None ? 1 : 0)),
            area.WithRelativeRows(-(_headerSizeInRows + (_headerVerticalBorderStyle != TableCellBorderStyle.None ? 1 : 0))));
        
        DrawRows(dataArea, table);
        
        _visibleRowsOnLastDraw = Math.Min(dataArea.Size.Rows, tableModel.GetRowCount() - _viewTopRow);
    }

    private void DrawHeader(ITextGUIGraphics graphics, Table<V> table)
    {
        var tableModel = table.GetTableModel();
        var tableHeaderRenderer = table.GetTableHeaderRenderer();
        var columnHeaders = tableModel.GetColumnLabels();
        
        int leftPosition = 0;
        int visibleColumns = Math.Min(columnHeaders.Count - _viewLeftColumn, 
                                      _preferredColumnSizes.Count - _viewLeftColumn);
        
        for (int i = 0; i < visibleColumns && leftPosition < graphics.Size.Columns; i++)
        {
            int columnIndex = _viewLeftColumn + i;
            if (columnIndex >= columnHeaders.Count) break;
            
            string label = columnHeaders[columnIndex];
            int columnWidth = Math.Min(_preferredColumnSizes[columnIndex], graphics.Size.Columns - leftPosition);
            if (columnWidth <= 0) break;
            
            var headerGraphics = graphics.NewTextGraphics(
                new TerminalPosition(leftPosition, 0),
                new TerminalSize(columnWidth, _headerSizeInRows));
            
            tableHeaderRenderer.DrawHeader(table, label, columnIndex, headerGraphics);
            leftPosition += columnWidth;
            
            // Draw horizontal border
            if (_headerHorizontalBorderStyle != TableCellBorderStyle.None && i < visibleColumns - 1)
            {
                if (leftPosition < graphics.Size.Columns)
                {
                    var themeDefinition = table.Theme.GetDefinition(table.GetType());
                    graphics.ApplyThemeStyle(themeDefinition.Normal);
                    graphics.SetCharacter(leftPosition, 0, GetVerticalCharacter(_headerHorizontalBorderStyle));
                    leftPosition++;
                }
            }
        }
        
        // Draw header-body separator
        if (_headerVerticalBorderStyle != TableCellBorderStyle.None)
        {
            var themeDefinition = table.Theme.GetDefinition(table.GetType());
            graphics.ApplyThemeStyle(themeDefinition.Normal);
            graphics.DrawLine(0, _headerSizeInRows, graphics.Size.Columns - 1, _headerSizeInRows,
                            GetHorizontalCharacter(_headerVerticalBorderStyle));
        }
    }

    private void DrawRows(ITextGUIGraphics graphics, Table<V> table)
    {
        var tableModel = table.GetTableModel();
        var tableCellRenderer = table.GetTableCellRenderer();
        var rows = tableModel.GetRows();
        
        int topPosition = 0;
        int maxRows = Math.Min(graphics.Size.Rows, rows.Count - _viewTopRow);
        
        for (int rowOffset = 0; rowOffset < maxRows && topPosition < graphics.Size.Rows; rowOffset++)
        {
            int rowIndex = _viewTopRow + rowOffset;
            if (rowIndex >= rows.Count) break;
            
            var row = rows[rowIndex];
            int rowHeight = _preferredRowSizes.Count > rowIndex ? _preferredRowSizes[rowIndex] : 1;
            
            DrawRowCells(graphics, table, row, rowIndex, topPosition, rowHeight);
            
            topPosition += rowHeight;
            
            // Draw row separator
            if (_cellVerticalBorderStyle != TableCellBorderStyle.None && rowOffset < maxRows - 1)
            {
                if (topPosition < graphics.Size.Rows)
                {
                    var themeDefinition = table.Theme.GetDefinition(table.GetType());
                    graphics.ApplyThemeStyle(themeDefinition.Normal);
                    graphics.DrawLine(0, topPosition, graphics.Size.Columns - 1, topPosition,
                                    GetHorizontalCharacter(_cellVerticalBorderStyle));
                    topPosition++;
                }
            }
        }
    }

    private void DrawRowCells(ITextGUIGraphics graphics, Table<V> table, IList<V?> row, int rowIndex, int topPosition, int rowHeight)
    {
        var tableCellRenderer = table.GetTableCellRenderer();
        
        int leftPosition = 0;
        int visibleColumns = Math.Min(row.Count - _viewLeftColumn, _preferredColumnSizes.Count - _viewLeftColumn);
        
        for (int i = 0; i < visibleColumns && leftPosition < graphics.Size.Columns; i++)
        {
            int columnIndex = _viewLeftColumn + i;
            if (columnIndex >= row.Count) break;
            
            var cell = row[columnIndex];
            int columnWidth = Math.Min(_preferredColumnSizes[columnIndex], graphics.Size.Columns - leftPosition);
            if (columnWidth <= 0) break;
            
            var cellGraphics = graphics.NewTextGraphics(
                new TerminalPosition(leftPosition, topPosition),
                new TerminalSize(columnWidth, rowHeight));
            
            if (cell != null)
            {
                tableCellRenderer.DrawCell(table, cell, columnIndex, rowIndex, cellGraphics);
            }
            leftPosition += columnWidth;
            
            // Draw vertical cell border
            if (_cellHorizontalBorderStyle != TableCellBorderStyle.None && i < visibleColumns - 1)
            {
                if (leftPosition < graphics.Size.Columns)
                {
                    var themeDefinition = table.Theme.GetDefinition(table.GetType());
                    bool isSelected = table.GetSelectedRow() == rowIndex && !table.IsCellSelection();
                    
                    if (isSelected && table.IsFocused)
                    {
                        graphics.ApplyThemeStyle(themeDefinition.Selected);
                    }
                    else
                    {
                        graphics.ApplyThemeStyle(themeDefinition.Normal);
                    }
                    
                    graphics.SetCharacter(leftPosition, topPosition, GetVerticalCharacter(_cellHorizontalBorderStyle));
                    leftPosition++;
                }
            }
        }
    }

    private char GetHorizontalCharacter(TableCellBorderStyle style)
    {
        return style switch
        {
            TableCellBorderStyle.SingleLine => Gui2.Symbols.SingleLineHorizontal,
            TableCellBorderStyle.DoubleLine => Gui2.Symbols.DoubleLineHorizontal,
            TableCellBorderStyle.EmptySpace => ' ',
            TableCellBorderStyle.None => ' ',
            _ => ' '
        };
    }

    private char GetVerticalCharacter(TableCellBorderStyle style)
    {
        return style switch
        {
            TableCellBorderStyle.SingleLine => Gui2.Symbols.SingleLineVertical,
            TableCellBorderStyle.DoubleLine => Gui2.Symbols.DoubleLineVertical,
            TableCellBorderStyle.EmptySpace => ' ',
            TableCellBorderStyle.None => ' ',
            _ => ' '
        };
    }
}