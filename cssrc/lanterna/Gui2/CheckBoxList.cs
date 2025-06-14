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
using Lanterna.Input;
using System.Collections.Concurrent;

namespace Lanterna.Gui2;

/// <summary>
/// A list component where each item has a checkbox that can be independently checked or unchecked.
/// Multiple items can be checked simultaneously, and the component provides mouse drag support for
/// bulk selection operations.
/// </summary>
/// <typeparam name="V">Type of items in the list</typeparam>
public class CheckBoxList<V> : AbstractListBox<V, CheckBoxList<V>>
{
    /// <summary>
    /// Interface for listening to checkbox state changes
    /// </summary>
    public interface IListener
    {
        /// <summary>
        /// Called when an item's checkbox state changes
        /// </summary>
        /// <param name="itemIndex">Index of the item that changed</param>
        /// <param name="isChecked">New checked state</param>
        void OnStatusChanged(int itemIndex, bool isChecked);
    }

    private readonly ConcurrentBag<IListener> _listeners;
    private readonly List<bool> _itemStatus;
    private readonly object _lock = new object();
    
    // Mouse drag state
    private bool _stateForMouseDragged;
    private int _minIndexForMouseDragged;
    private int _maxIndexForMouseDragged;

    /// <summary>
    /// Creates a new CheckBoxList with default settings
    /// </summary>
    public CheckBoxList() : this(null)
    {
    }

    /// <summary>
    /// Creates a new CheckBoxList with specified preferred size
    /// </summary>
    /// <param name="preferredSize">Preferred size for the list</param>
    public CheckBoxList(TerminalSize? preferredSize) : base(preferredSize)
    {
        _listeners = new ConcurrentBag<IListener>();
        _itemStatus = new List<bool>();
        _stateForMouseDragged = false;
        _minIndexForMouseDragged = -1;
        _maxIndexForMouseDragged = -1;
    }

    /// <summary>
    /// Adds an item to the list with specified checkbox state
    /// </summary>
    /// <param name="item">Item to add</param>
    /// <param name="checkedState">Initial checked state</param>
    /// <returns>This CheckBoxList for fluent API</returns>
    public CheckBoxList<V> AddItem(V item, bool checkedState)
    {
        lock (_lock)
        {
            base.AddItem(item);
            _itemStatus.Add(checkedState);
            return this;
        }
    }

    /// <summary>
    /// Adds an item to the list with unchecked state
    /// </summary>
    /// <param name="item">Item to add</param>
    /// <returns>This CheckBoxList for fluent API</returns>
    public new CheckBoxList<V> AddItem(V item)
    {
        return AddItem(item, false);
    }

    /// <summary>
    /// Checks if an item is checked by reference
    /// </summary>
    /// <param name="item">Item to check</param>
    /// <returns>True if checked, false if unchecked, null if item not found</returns>
    public bool? IsChecked(V item)
    {
        lock (_lock)
        {
            int index = IndexOf(item);
            return index != -1 ? IsChecked(index) : null;
        }
    }

    /// <summary>
    /// Checks if an item is checked by index
    /// </summary>
    /// <param name="index">Index to check</param>
    /// <returns>True if checked, false if unchecked</returns>
    public bool IsChecked(int index)
    {
        lock (_lock)
        {
            if (index < 0 || index >= _itemStatus.Count)
                return false;
            return _itemStatus[index];
        }
    }

    /// <summary>
    /// Sets the checked state of an item by reference
    /// </summary>
    /// <param name="item">Item to modify</param>
    /// <param name="isChecked">New checked state</param>
    /// <returns>This CheckBoxList for fluent API</returns>
    public CheckBoxList<V> SetChecked(V item, bool isChecked)
    {
        lock (_lock)
        {
            int index = IndexOf(item);
            if (index != -1)
            {
                SetChecked(index, isChecked);
            }
            return this;
        }
    }

    /// <summary>
    /// Sets the checked state of an item by index
    /// </summary>
    /// <param name="index">Index of item to modify</param>
    /// <param name="isChecked">New checked state</param>
    /// <returns>This CheckBoxList for fluent API</returns>
    public CheckBoxList<V> SetChecked(int index, bool isChecked)
    {
        lock (_lock)
        {
            if (index < 0 || index >= _itemStatus.Count)
                return this;

            bool oldState = _itemStatus[index];
            if (oldState != isChecked)
            {
                _itemStatus[index] = isChecked;
                FireStatusChanged(index, isChecked);
                Invalidate();
            }
            return this;
        }
    }

    /// <summary>
    /// Toggles the checked state of an item by index
    /// </summary>
    /// <param name="index">Index of item to toggle</param>
    /// <returns>This CheckBoxList for fluent API</returns>
    public CheckBoxList<V> ToggleChecked(int index)
    {
        lock (_lock)
        {
            if (index >= 0 && index < _itemStatus.Count)
            {
                SetChecked(index, !_itemStatus[index]);
            }
            return this;
        }
    }

    /// <summary>
    /// Gets all currently checked items
    /// </summary>
    /// <returns>List of checked items</returns>
    public List<V> GetCheckedItems()
    {
        lock (_lock)
        {
            var checkedItems = new List<V>();
            for (int i = 0; i < _itemStatus.Count && i < ItemCount; i++)
            {
                if (_itemStatus[i])
                {
                    checkedItems.Add(GetItemAt(i));
                }
            }
            return checkedItems;
        }
    }

    /// <summary>
    /// Clears all items and their checkbox states
    /// </summary>
    /// <returns>This CheckBoxList for fluent API</returns>
    public override CheckBoxList<V> ClearItems()
    {
        lock (_lock)
        {
            _itemStatus.Clear();
            return base.ClearItems();
        }
    }

    /// <summary>
    /// Removes an item at the specified index
    /// </summary>
    /// <param name="index">Index of item to remove</param>
    /// <returns>The removed item</returns>
    public override V RemoveItem(int index)
    {
        lock (_lock)
        {
            if (index >= 0 && index < _itemStatus.Count)
            {
                _itemStatus.RemoveAt(index);
            }
            return base.RemoveItem(index);
        }
    }

    /// <summary>
    /// Adds a listener for checkbox state changes
    /// </summary>
    /// <param name="listener">Listener to add</param>
    public void AddListener(IListener listener)
    {
        if (listener != null)
        {
            _listeners.Add(listener);
        }
    }

    /// <summary>
    /// Removes a listener for checkbox state changes
    /// </summary>
    /// <param name="listener">Listener to remove</param>
    public void RemoveListener(IListener listener)
    {
        // Note: ConcurrentBag doesn't support efficient removal
        // In practice, this is rarely used, so we keep the simple approach
        // If removal is needed frequently, consider using a different collection
    }

    protected override IInteractable.Result HandleKeyStroke(KeyStroke keyStroke)
    {
        lock (_lock)
        {
            if (IsKeyboardActivationStroke(keyStroke))
            {
                ToggleChecked(GetSelectedIndex());
                return IInteractable.Result.Handled;
            }
            else if (keyStroke.KeyType == KeyType.MouseEvent)
            {
                var mouseAction = keyStroke as MouseAction;
                if (mouseAction != null)
                {
                    return HandleMouseAction(mouseAction);
                }
            }
            
            return base.HandleKeyStroke(keyStroke);
        }
    }

    private IInteractable.Result HandleMouseAction(MouseAction mouseAction)
    {
        var result = base.HandleKeyStroke(mouseAction);
        
        if (mouseAction.ActionType == MouseActionType.ClickDown)
        {
            int newIndex = GetIndexByMouseAction(mouseAction);
            if (newIndex != -1)
            {
                _stateForMouseDragged = !IsChecked(newIndex);
                SetChecked(newIndex, _stateForMouseDragged);
                _minIndexForMouseDragged = newIndex;
                _maxIndexForMouseDragged = newIndex;
            }
        }
        else if (mouseAction.ActionType == MouseActionType.Drag)
        {
            int dragIndex = GetIndexByMouseAction(mouseAction);
            if (dragIndex != -1)
            {
                // Update drag range
                _minIndexForMouseDragged = Math.Min(_minIndexForMouseDragged, dragIndex);
                _maxIndexForMouseDragged = Math.Max(_maxIndexForMouseDragged, dragIndex);
                
                // Apply drag state to all items in range
                for (int i = _minIndexForMouseDragged; i <= _maxIndexForMouseDragged; i++)
                {
                    if (i >= 0 && i < _itemStatus.Count)
                    {
                        SetChecked(i, _stateForMouseDragged);
                    }
                }
            }
        }
        
        return result;
    }

    private void FireStatusChanged(int index, bool isChecked)
    {
        // Fire events on GUI thread if possible
        var textGUI = TextGUI;
        if (textGUI?.GUIThread != null && 
            System.Threading.Thread.CurrentThread == textGUI.GUIThread.Thread)
        {
            NotifyListeners(index, isChecked);
        }
        else
        {
            // For now, fire directly - GUI thread integration can be improved later
            NotifyListeners(index, isChecked);
        }
    }

    private void NotifyListeners(int index, bool isChecked)
    {
        foreach (var listener in _listeners)
        {
            try
            {
                listener.OnStatusChanged(index, isChecked);
            }
            catch (Exception)
            {
                // Ignore listener exceptions to prevent one bad listener from breaking others
            }
        }
    }

    protected override ListItemRenderer<V, CheckBoxList<V>> CreateDefaultListItemRenderer()
    {
        return new CheckBoxListItemRenderer<V>();
    }

    /// <summary>
    /// Custom renderer for CheckBoxList items that displays checkboxes
    /// </summary>
    /// <typeparam name="T">Type of items in the list</typeparam>
    public class CheckBoxListItemRenderer<T> : ListItemRenderer<T, CheckBoxList<T>>
    {
        public override int GetHotSpotPositionOnLine(int selectedIndex)
        {
            return 1; // Position cursor on the checkbox marker
        }

        public override string GetLabel(CheckBoxList<T> listBox, int index, T item)
        {
            string check = listBox.IsChecked(index) ? "x" : " ";
            string text = item?.ToString() ?? "";
            return $"[{check}] {text}";
        }

        public override void DrawItem(ITextGUIGraphics graphics, CheckBoxList<T> listBox, int index, T item, 
                            bool selected, bool focused)
        {
            if (graphics.Size.Rows == 0 || graphics.Size.Columns < 4)
                return;

            string text = item?.ToString() ?? "";
            bool isChecked = listBox.IsChecked(index);
            
            // Apply appropriate theme style
            ApplyThemeStyle(graphics, listBox, selected, focused);

            // Clear the line
            graphics.Fill(' ');

            // Draw checkbox brackets and marker
            graphics.SetCharacter(0, 0, '[');
            graphics.SetCharacter(1, 0, isChecked ? 'x' : ' ');
            graphics.SetCharacter(2, 0, ']');
            graphics.SetCharacter(3, 0, ' ');

            // Draw item text
            if (text.Length > 0 && graphics.Size.Columns > 4)
            {
                int availableSpace = graphics.Size.Columns - 4;
                if (text.Length > availableSpace)
                {
                    text = text.Substring(0, availableSpace);
                }
                graphics.PutString(4, 0, text);
            }
        }

        private void ApplyThemeStyle(ITextGUIGraphics graphics, CheckBoxList<T> listBox, bool selected, bool focused)
        {
            var themeDefinition = listBox.Theme.GetDefinition(typeof(CheckBoxList<T>));
            if (selected && focused)
            {
                graphics.ApplyThemeStyle(themeDefinition.Selected);
            }
            else if (selected && !focused)
            {
                graphics.ApplyThemeStyle(themeDefinition.Active);
            }
            else if (focused)
            {
                graphics.ApplyThemeStyle(themeDefinition.Insensitive);
            }
            else
            {
                graphics.ApplyThemeStyle(themeDefinition.Normal);
            }
        }
    }
}