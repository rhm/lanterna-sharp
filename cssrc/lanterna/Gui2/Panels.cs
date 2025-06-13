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
namespace Lanterna.Gui2;

/// <summary>
/// Utility class for quickly bunching up components in a panel, arranged in a particular pattern
/// </summary>
public static class Panels
{
    /// <summary>
    /// Creates a new Panel with a LinearLayout layout manager in horizontal mode and adds all the
    /// components passed in
    /// </summary>
    /// <param name="components">Components to be added to the new Panel, in order</param>
    /// <returns>The new Panel</returns>
    public static Panel Horizontal(params IComponent[] components)
    {
        var panel = new Panel();
        panel.SetLayoutManager(new LinearLayout(Direction.Horizontal));
        foreach (var component in components)
        {
            panel.AddComponent(component);
        }
        return panel;
    }

    /// <summary>
    /// Creates a new Panel with a LinearLayout layout manager in vertical mode and adds all the
    /// components passed in
    /// </summary>
    /// <param name="components">Components to be added to the new Panel, in order</param>
    /// <returns>The new Panel</returns>
    public static Panel Vertical(params IComponent[] components)
    {
        var panel = new Panel();
        panel.SetLayoutManager(new LinearLayout(Direction.Vertical));
        foreach (var component in components)
        {
            panel.AddComponent(component);
        }
        return panel;
    }

    /// <summary>
    /// Creates a new Panel with a GridLayout layout manager and adds all the components passed in
    /// </summary>
    /// <param name="columns">Number of columns in the grid</param>
    /// <param name="components">Components to be added to the new Panel, in order</param>
    /// <returns>The new Panel</returns>
    public static Panel Grid(int columns, params IComponent[] components)
    {
        var panel = new Panel();
        panel.SetLayoutManager(new GridLayout(columns));
        foreach (var component in components)
        {
            panel.AddComponent(component);
        }
        return panel;
    }
}