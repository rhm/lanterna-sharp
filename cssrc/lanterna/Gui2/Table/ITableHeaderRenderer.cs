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
/// This interface can be implemented if you want to customize how table headers are drawn.
/// </summary>
/// <typeparam name="V">Type of data stored in each table cell</typeparam>
public interface ITableHeaderRenderer<V>
{
    /// <summary>
    /// Called by the table when it wants to know how big a particular table header should be
    /// </summary>
    /// <param name="table">Table containing the header</param>
    /// <param name="label">Label for this header</param>
    /// <param name="columnIndex">Column index of the header</param>
    /// <returns>Size this renderer would like the header to have</returns>
    TerminalSize GetPreferredSize(Table<V> table, string label, int columnIndex);

    /// <summary>
    /// Called by the table when it's time to draw a header, you can see how much size is available by checking the size
    /// of the textGUIGraphics. The top-left position of the graphics object is the top-left position of this
    /// header.
    /// </summary>
    /// <param name="table">Table containing the header</param>
    /// <param name="label">Label for this header</param>
    /// <param name="index">Column index of the header</param>
    /// <param name="textGUIGraphics">Graphics object to header with</param>
    void DrawHeader(Table<V> table, string label, int index, ITextGUIGraphics textGUIGraphics);
}