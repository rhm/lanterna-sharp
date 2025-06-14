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
/// Implementation of a drop-down menu contained in a MenuBar and also a sub-menu inside another Menu.
/// </summary>
public class Menu : MenuItem
{
    private readonly List<MenuItem> _subItems;

    /// <summary>
    /// Creates a menu with the specified label
    /// </summary>
    /// <param name="label">Label to use for the menu item that will trigger this menu to pop up</param>
    public Menu(string label) : base(label)
    {
        _subItems = new List<MenuItem>();
    }

    /// <summary>
    /// Adds a new menu item to this menu, this can be either a regular MenuItem or another Menu
    /// </summary>
    /// <param name="menuItem">The item to add to this menu</param>
    /// <returns>Itself</returns>
    public Menu Add(MenuItem menuItem)
    {
        lock (_subItems)
        {
            _subItems.Add(menuItem);
        }
        return this;
    }

    public new bool OnActivated()
    {
        bool result = true;
        if (_subItems.Count == 0)
        {
            return result;
        }

        var popupMenu = new MenuPopupWindow(this);
        var popupCancelled = false;

        foreach (var menuItem in _subItems)
        {
            popupMenu.AddMenuItem(menuItem);
        }

        if (Parent is MenuBar menuBar)
        {
            popupMenu.AddWindowListener(new MenuBarWindowListener(this, menuBar, popupMenu));
        }

        popupMenu.AddWindowListener(new EscapeWindowListener(() => popupCancelled = true, popupMenu));
        
        if (TextGUI is IWindowBasedTextGUI windowBasedTextGUI)
        {
            windowBasedTextGUI.AddWindow(popupMenu);
            windowBasedTextGUI.WaitForWindowToClose(popupMenu);
        }
        
        result = !popupCancelled;
        return result;
    }

    private class MenuBarWindowListener : IWindowListener
    {
        private readonly Menu _menu;
        private readonly MenuBar _menuBar;
        private readonly MenuPopupWindow _popupMenu;

        public MenuBarWindowListener(Menu menu, MenuBar menuBar, MenuPopupWindow popupMenu)
        {
            _menu = menu;
            _menuBar = menuBar;
            _popupMenu = popupMenu;
        }

        public void OnResized(IWindow window, TerminalSize oldSize, TerminalSize newSize) { }
        public void OnMoved(IWindow window, TerminalPosition oldPosition, TerminalPosition newPosition) { }
        public void OnInput(IWindow window, KeyStroke keyStroke, ref bool handled) { }

        public void OnUnhandledInput(IWindow basePane, KeyStroke keyStroke, ref bool hasBeenHandled)
        {
            if (keyStroke.KeyType == KeyType.ArrowLeft)
            {
                int thisMenuIndex = _menuBar.GetChildrenList().ToList().IndexOf(_menu);
                if (thisMenuIndex > 0)
                {
                    _popupMenu.Close();
                    var nextSelectedMenu = _menuBar.GetMenu(thisMenuIndex - 1);
                    nextSelectedMenu.TakeFocus();
                    nextSelectedMenu.OnActivated();
                }
            }
            else if (keyStroke.KeyType == KeyType.ArrowRight)
            {
                int thisMenuIndex = _menuBar.GetChildrenList().ToList().IndexOf(_menu);
                if (thisMenuIndex >= 0 && thisMenuIndex < _menuBar.GetMenuCount() - 1)
                {
                    _popupMenu.Close();
                    var nextSelectedMenu = _menuBar.GetMenu(thisMenuIndex + 1);
                    nextSelectedMenu.TakeFocus();
                    nextSelectedMenu.OnActivated();
                }
            }
        }
    }

    private class EscapeWindowListener : IWindowListener
    {
        private readonly Action _setCancelled;
        private readonly MenuPopupWindow _popupMenu;

        public EscapeWindowListener(Action setCancelled, MenuPopupWindow popupMenu)
        {
            _setCancelled = setCancelled;
            _popupMenu = popupMenu;
        }

        public void OnResized(IWindow window, TerminalSize oldSize, TerminalSize newSize) { }
        public void OnMoved(IWindow window, TerminalPosition oldPosition, TerminalPosition newPosition) { }
        public void OnInput(IWindow window, KeyStroke keyStroke, ref bool handled) { }

        public void OnUnhandledInput(IWindow basePane, KeyStroke keyStroke, ref bool hasBeenHandled)
        {
            if (keyStroke.KeyType == KeyType.Escape)
            {
                _setCancelled();
                _popupMenu.Close();
            }
        }
    }
}