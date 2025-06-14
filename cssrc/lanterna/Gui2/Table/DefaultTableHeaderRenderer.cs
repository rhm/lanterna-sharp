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
/// Default implementation of ITableHeaderRenderer that draws a simple header with padding
/// </summary>
/// <typeparam name="V">Type of data stored in each table cell</typeparam>
public class DefaultTableHeaderRenderer<V> : ITableHeaderRenderer<V>
{
    public TerminalSize GetPreferredSize(Table<V> table, string label, int columnIndex)
    {
        return new TerminalSize(Math.Max(label.Length + 2, 8), 1);
    }

    public void DrawHeader(Table<V> table, string label, int index, ITextGUIGraphics textGUIGraphics)
    {
        var themeDefinition = table.Theme.GetDefinition(table.GetType());
        textGUIGraphics.ApplyThemeStyle(themeDefinition.Normal);
        textGUIGraphics.Fill(' ');
        textGUIGraphics.PutString(1, 0, label);
    }
}