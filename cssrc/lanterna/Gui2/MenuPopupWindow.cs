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
using Lanterna.Gui2.Menu;

namespace Lanterna.Gui2;

/// <summary>
/// This class is a Window implementation that automatically sets some common settings that you'd want on
/// specifically popup windows with menu items. It ensures that the window is modal and has a fixed position (rather than
/// letting the window manager choose).
/// </summary>
public class MenuPopupWindow : AbstractWindow
{
    private readonly Panel _menuItemPanel;

    /// <summary>
    /// Creates a new popup window with a relative position to the component supplied.
    /// </summary>
    /// <param name="parent">Component that this popup menu is triggered from</param>
    public MenuPopupWindow(IComponent? parent) : base()
    {
        SetHints(new HashSet<IWindow.Hint> { IWindow.Hint.Modal, IWindow.Hint.MenuPopup, IWindow.Hint.FixedPosition });
        if (parent != null)
        {
            var menuPositionGlobal = parent.ToGlobal(TerminalPosition.TopLeftCorner);
            Position = menuPositionGlobal?.WithRelative(0, 1) ?? TerminalPosition.TopLeftCorner;
        }
        _menuItemPanel = new Panel(new LinearLayout(Direction.Vertical));
        Component = _menuItemPanel;
    }

    /// <summary>
    /// Adds a new menu item to this popup window. The item will automatically be selected if it's the first one added.
    /// </summary>
    /// <param name="menuItem">Menu item to add to the popup window.</param>
    public void AddMenuItem(MenuItem menuItem)
    {
        _menuItemPanel.AddComponent(menuItem);
        menuItem.SetLayoutData(LinearLayout.CreateLayoutData(LinearLayout.Alignment.Fill));
        if (_menuItemPanel.ChildCount == 1)
        {
            SetFocusedInteractable(menuItem);
        }
        Invalidate();
    }
}