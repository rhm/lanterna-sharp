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

namespace Lanterna.Gui2.Table;

/// <summary>
/// Default implementation of ITableCellRenderer that draws cell content with selection highlighting
/// </summary>
/// <typeparam name="V">Type of data stored in each table cell</typeparam>
public class DefaultTableCellRenderer<V> : ITableCellRenderer<V>
{
    public TerminalSize GetPreferredSize(Table<V> table, V cell, int columnIndex, int rowIndex)
    {
        string cellString = cell?.ToString() ?? "";
        return new TerminalSize(Math.Max(cellString.Length + 2, 8), 1);
    }

    public void DrawCell(Table<V> table, V cell, int columnIndex, int rowIndex, ITextGUIGraphics textGUIGraphics)
    {
        var themeDefinition = table.Theme.GetDefinition(table.GetType());
        bool isSelected = table.GetSelectedRow() == rowIndex && 
                         (!table.IsCellSelection() || table.GetSelectedColumn() == columnIndex);
        
        if (isSelected && table.IsFocused)
        {
            textGUIGraphics.ApplyThemeStyle(themeDefinition.Selected);
        }
        else
        {
            textGUIGraphics.ApplyThemeStyle(themeDefinition.Normal);
        }
        
        textGUIGraphics.Fill(' ');
        string cellString = cell?.ToString() ?? "";
        textGUIGraphics.PutString(1, 0, cellString);
    }
}