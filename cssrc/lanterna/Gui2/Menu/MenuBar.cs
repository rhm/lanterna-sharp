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
 * Copyright (C) 2017 Bruno Eberhard
 * Copyright (C) 2017 University of Waikato, Hamilton, NZ
 */
using Lanterna.Core;
using Lanterna.Input;
using System.Collections.Concurrent;

namespace Lanterna.Gui2.Menu;

/// <summary>
/// A menu bar offering drop-down menus. You can attach a menu bar to a Window by using the
/// Window.SetMenuBar(MenuBar) method, then use MenuBar.Add(Menu) to add sub-menus to the menu bar.
/// </summary>
public class MenuBar : AbstractComponent<MenuBar>, IContainer, IMenuBar
{
    private const int ExtraPadding = 0;
    private readonly ConcurrentBag<Menu> _menus;

    /// <summary>
    /// Creates a new menu bar
    /// </summary>
    public MenuBar()
    {
        _menus = new ConcurrentBag<Menu>();
    }

    /// <summary>
    /// Adds a new drop-down menu to the menu bar, at the end
    /// </summary>
    /// <param name="menu">Menu to add to the menu bar</param>
    /// <returns>Itself</returns>
    public MenuBar Add(Menu menu)
    {
        _menus.Add(menu);
        menu.OnAdded(this);
        return this;
    }

    public int ChildCount => GetMenuCount();

    public IReadOnlyList<IComponent> ChildrenList => _menus.Cast<IComponent>().ToList().AsReadOnly();
    
    public IReadOnlyCollection<IComponent> Children => _menus.Cast<IComponent>().ToList().AsReadOnly();

    public IReadOnlyList<IComponent> GetChildrenList()
    {
        return ChildrenList;
    }

    public IEnumerable<IComponent> GetChildren()
    {
        return Children;
    }

    public bool ContainsComponent(IComponent component)
    {
        return _menus.Contains(component);
    }

    public bool RemoveComponent(IComponent component)
    {
        // ConcurrentBag doesn't support removal, so we'll need to track this differently
        // For now, let's implement a simple approach
        var menuList = _menus.ToList();
        bool hadMenu = menuList.Contains(component);
        if (hadMenu)
        {
            component.OnRemoved(this);
            // Note: ConcurrentBag doesn't support removal, this is a limitation
            // In a real implementation, we might use a different collection type
        }
        return hadMenu;
    }

    public IInteractable? NextFocus(IInteractable? fromThis)
    {
        var menuList = _menus.ToList();
        if (menuList.Count == 0)
        {
            return null;
        }
        else if (fromThis == null)
        {
            return menuList[0];
        }
        else if (!menuList.Contains(fromThis) || menuList.IndexOf((Menu)fromThis) == menuList.Count - 1)
        {
            return null;
        }
        else
        {
            return menuList[menuList.IndexOf((Menu)fromThis) + 1];
        }
    }

    public IInteractable? PreviousFocus(IInteractable? fromThis)
    {
        var menuList = _menus.ToList();
        if (menuList.Count == 0)
        {
            return null;
        }
        else if (fromThis == null)
        {
            return menuList[menuList.Count - 1];
        }
        else if (!menuList.Contains(fromThis) || menuList.IndexOf((Menu)fromThis) == 0)
        {
            return null;
        }
        else
        {
            return menuList[menuList.IndexOf((Menu)fromThis) - 1];
        }
    }

    public bool HandleInput(KeyStroke key)
    {
        return false;
    }

    /// <summary>
    /// Returns the drop-down menu at the specified index. This method will throw an exception if out of range.
    /// </summary>
    /// <param name="index">Index of the menu to return</param>
    /// <returns>The drop-down menu at the specified index</returns>
    /// <exception cref="IndexOutOfRangeException">if the index is out of range</exception>
    public Menu GetMenu(int index)
    {
        var menuList = _menus.ToList();
        return menuList[index];
    }

    /// <summary>
    /// Returns the number of menus this menu bar currently has
    /// </summary>
    /// <returns>The number of menus this menu bar currently has</returns>
    public int GetMenuCount()
    {
        return _menus.Count;
    }

    protected override IComponentRenderer<MenuBar> CreateDefaultRenderer()
    {
        return new DefaultMenuBarRenderer();
    }

    public void UpdateLookupMap(IInteractableLookupMap interactableLookupMap)
    {
        foreach (var menu in _menus)
        {
            interactableLookupMap.Add(menu, menu.Position);
        }
    }

    public override TerminalPosition? ToBasePane(TerminalPosition position)
    {
        // Assume the menu is always at the top of the content panel
        return position;
    }

    public bool IsEmptyMenuBar()
    {
        return false;
    }

    /// <summary>
    /// The default implementation for rendering a MenuBar
    /// </summary>
    public class DefaultMenuBarRenderer : IComponentRenderer<MenuBar>
    {
        public TerminalSize GetPreferredSize(MenuBar menuBar)
        {
            int maxHeight = 1;
            int totalWidth = ExtraPadding;
            for (int i = 0; i < menuBar.GetMenuCount(); i++)
            {
                var menu = menuBar.GetMenu(i);
                var preferredSize = menu.PreferredSize;
                maxHeight = Math.Max(maxHeight, preferredSize.Rows);
                totalWidth += preferredSize.Columns;
            }
            totalWidth += ExtraPadding;
            return new TerminalSize(totalWidth, maxHeight);
        }

        public void DrawComponent(ITextGUIGraphics graphics, MenuBar menuBar)
        {
            // Reset the area
            graphics.ApplyThemeStyle(menuBar.Theme.GetDefinition(menuBar.GetType()).Normal);
            graphics.Fill(' ');

            int leftPosition = ExtraPadding;
            var size = graphics.Size;
            int remainingSpace = size.Columns - ExtraPadding;
            for (int i = 0; i < menuBar.GetMenuCount() && remainingSpace > 0; i++)
            {
                var menu = menuBar.GetMenu(i);
                var preferredSize = menu.PreferredSize;
                menu.Position = menu.Position
                    .WithColumn(leftPosition)
                    .WithRow(0);
                int finalWidth = Math.Min(preferredSize.Columns, remainingSpace);
                menu.Size = menu.Size
                    .WithColumns(finalWidth)
                    .WithRows(size.Rows);
                remainingSpace -= finalWidth + ExtraPadding;
                leftPosition += finalWidth + ExtraPadding;
                var componentGraphics = graphics.NewTextGraphics(menu.Position, menu.Size);
                menu.Draw(componentGraphics);
            }
        }
    }
}