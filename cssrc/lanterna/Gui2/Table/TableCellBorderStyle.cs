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
/// Describing how table cells are separated when drawn
/// </summary>
public enum TableCellBorderStyle
{
    /// <summary>
    /// There is no separation between table cells, they are drawn immediately next to each other
    /// </summary>
    None = 0,
    
    /// <summary>
    /// There is a single space of separation between the cells, drawn as a single line
    /// </summary>
    SingleLine = 1,
    
    /// <summary>
    /// There is a single space of separation between the cells, drawn as a double line
    /// </summary>
    DoubleLine = 2,
    
    /// <summary>
    /// There is a single space of separation between the cells, kept empty
    /// </summary>
    EmptySpace = 3
}

/// <summary>
/// Extension methods for TableCellBorderStyle
/// </summary>
public static class TableCellBorderStyleExtensions
{
    /// <summary>
    /// Returns the number of rows (for vertical borders) or columns (for horizontal borders) this table cell border will
    /// take up when used.
    /// </summary>
    /// <param name="style">The border style</param>
    /// <returns>Size of the border, in rows or columns depending on the context</returns>
    public static int GetSize(this TableCellBorderStyle style)
    {
        return style switch
        {
            TableCellBorderStyle.None => 0,
            TableCellBorderStyle.SingleLine => 1,
            TableCellBorderStyle.DoubleLine => 1,
            TableCellBorderStyle.EmptySpace => 1,
            _ => 0
        };
    }
}