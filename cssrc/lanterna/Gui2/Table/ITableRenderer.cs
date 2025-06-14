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
/// Formalized interactable renderer for tables
/// </summary>
/// <typeparam name="V">Type of data stored in each table cell</typeparam>
public interface ITableRenderer<V> : IInteractableRenderer<Table<V>>
{
    /// <summary>
    /// Gets or sets whether scroll bars are hidden
    /// </summary>
    bool IsScrollBarsHidden { get; set; }

    /// <summary>
    /// Returns the number of rows visible in the table cell area on the last draw operation
    /// </summary>
    /// <returns>The number of rows visible in the table cell area on the last draw operation</returns>
    int GetVisibleRowsOnLastDraw();

    /// <summary>
    /// Returns the index of the first visible row with the renderers current state
    /// </summary>
    /// <returns>Index of the first visible row of the table</returns>
    int GetViewTopRow();

    /// <summary>
    /// Modifies which row is the first visible, this may be overwritten depending on the circumstances when drawing the
    /// table.
    /// </summary>
    /// <param name="viewTopRow">First row to be displayed when drawing the table</param>
    void SetViewTopRow(int viewTopRow);

    /// <summary>
    /// Returns the index of the first visible column with the renderers current state
    /// </summary>
    /// <returns>Index of the first visible column of the table</returns>
    int GetViewLeftColumn();

    /// <summary>
    /// Modifies which column is the first visible, this may be overwritten depending on the circumstances when drawing the
    /// table.
    /// </summary>
    /// <param name="viewLeftColumn">First column to be displayed when drawing the table</param>
    void SetViewLeftColumn(int viewLeftColumn);

    /// <summary>
    /// Gets or sets whether partial columns are allowed when not all columns fit on the screen
    /// </summary>
    /// <param name="allowPartialColumn">when not all columns fit on the screen, whether to render part of a column, or skip rendering that column entirely</param>
    bool AllowPartialColumn { get; set; }
}