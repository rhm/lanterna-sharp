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
using Lanterna.Graphics;
using Lanterna.Input;

namespace Lanterna.Gui2.Menu;

/// <summary>
/// This class is a single item that appears in a Menu with an optional action attached to it
/// </summary>
public class MenuItem : AbstractInteractableComponent<MenuItem>
{
    private string _label;
    private readonly Action _action;

    /// <summary>
    /// Creates a MenuItem with a label that does nothing when activated
    /// </summary>
    /// <param name="label">Label of the new MenuItem</param>
    public MenuItem(string label) : this(label, () => { })
    {
    }

    /// <summary>
    /// Creates a new MenuItem with a label and an action that will run on the GUI thread when activated. When
    /// the action has finished, the Menu containing this item will close.
    /// </summary>
    /// <param name="label">Label of the new MenuItem</param>
    /// <param name="action">Action to invoke on the GUI thread when the menu item is activated</param>
    public MenuItem(string label, Action action)
    {
        _action = action;
        if (string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentException("Menu label is not allowed to be null or empty");
        }
        _label = label.Trim();
    }

    /// <summary>
    /// Returns the label of this menu item
    /// </summary>
    /// <returns>Label of this menu item</returns>
    public string GetLabel()
    {
        return _label;
    }

    protected override IInteractableRenderer<MenuItem> CreateDefaultRenderer()
    {
        return new DefaultMenuItemRenderer();
    }

    /// <summary>
    /// Method to invoke when a menu item is "activated" by pressing the Enter key.
    /// </summary>
    /// <returns>Returns true if the action was performed successfully, otherwise false, which will not
    /// automatically close the popup window itself.</returns>
    protected virtual bool OnActivated()
    {
        _action();
        return true;
    }

    protected override IInteractable.Result HandleKeyStroke(KeyStroke keyStroke)
    {
        if (IsActivationStroke(keyStroke))
        {
            if (OnActivated())
            {
                var basePane = BasePane;
                if (basePane is IWindow window && window.Hints.Contains(IWindow.Hint.MenuPopup))
                {
                    window.Close();
                }
            }
            return IInteractable.Result.Handled;
        }
        else if (IsMouseMove(keyStroke))
        {
            TakeFocus();
            return IInteractable.Result.Handled;
        }
        else
        {
            return base.HandleKeyStroke(keyStroke);
        }
    }

    /// <summary>
    /// Helper interface that doesn't add any new methods but makes coding new menu renderers a little bit more clear
    /// </summary>
    public abstract class MenuItemRenderer : IInteractableRenderer<MenuItem>
    {
        public abstract TerminalPosition? GetCursorLocation(MenuItem component);
        public abstract TerminalSize GetPreferredSize(MenuItem component);
        public abstract void DrawComponent(ITextGUIGraphics graphics, MenuItem component);
    }

    /// <summary>
    /// Default renderer for menu items (both sub-menus and regular items)
    /// </summary>
    public class DefaultMenuItemRenderer : MenuItemRenderer
    {
        public override TerminalPosition? GetCursorLocation(MenuItem component)
        {
            return null;
        }

        public override TerminalSize GetPreferredSize(MenuItem component)
        {
            int preferredWidth = TerminalTextUtils.GetColumnWidth(component.GetLabel()) + 2;
            if (component is Menu && !(component.Parent is MenuBar))
            {
                preferredWidth += 2;
            }
            return TerminalSize.One.WithColumns(preferredWidth);
        }

        public override void DrawComponent(ITextGUIGraphics graphics, MenuItem menuItem)
        {
            var themeDefinition = menuItem.Theme.GetDefinition(menuItem.GetType());
            if (menuItem.IsFocused)
            {
                graphics.ApplyThemeStyle(themeDefinition.Selected);
            }
            else
            {
                graphics.ApplyThemeStyle(themeDefinition.Normal);
            }

            string label = menuItem.GetLabel();
            string leadingCharacter = label.Length > 0 ? label.Substring(0, 1) : "";

            graphics.Fill(' ');
            graphics.PutString(1, 0, label);
            if (menuItem is Menu && !(menuItem.Parent is MenuBar))
            {
                graphics.PutString(graphics.Size.Columns - 2, 0, Symbols.Triangle_Right_Pointing_Black.ToString());
            }
            if (!string.IsNullOrEmpty(label))
            {
                if (menuItem.IsFocused)
                {
                    graphics.ApplyThemeStyle(themeDefinition.Active);
                }
                else
                {
                    graphics.ApplyThemeStyle(themeDefinition.PreLight);
                }
                graphics.PutString(1, 0, leadingCharacter);
            }
        }
    }
}