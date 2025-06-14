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

namespace Lanterna.Gui2.Table;

/// <summary>
/// The main interface to implement when you need to customize the way table cells are drawn
/// </summary>
/// <typeparam name="V">Type of data in the table cells</typeparam>
public interface ITableCellRenderer<V>
{
    /// <summary>
    /// Called by the table when it wants to know how big a particular table cell should be
    /// </summary>
    /// <param name="table">Table containing the cell</param>
    /// <param name="cell">Data stored in the cell</param>
    /// <param name="columnIndex">Column index of the cell</param>
    /// <param name="rowIndex">Row index of the cell</param>
    /// <returns>Size this renderer would like the cell to have</returns>
    TerminalSize GetPreferredSize(Table<V> table, V cell, int columnIndex, int rowIndex);

    /// <summary>
    /// Called by the table when it's time to draw a cell, you can see how much size is available by checking the size of
    /// the textGUIGraphics. The top-left position of the graphics object is the top-left position of this cell.
    /// </summary>
    /// <param name="table">Table containing the cell</param>
    /// <param name="cell">Data stored in the cell</param>
    /// <param name="columnIndex">Column index of the cell</param>
    /// <param name="rowIndex">Row index of the cell</param>
    /// <param name="textGUIGraphics">Graphics object to draw with</param>
    void DrawCell(Table<V> table, V cell, int columnIndex, int rowIndex, ITextGUIGraphics textGUIGraphics);
}