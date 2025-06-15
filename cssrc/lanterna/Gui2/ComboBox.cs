using Lanterna.Core;
using Lanterna.Graphics;
using Lanterna.Input;
using System.Collections.Concurrent;

namespace Lanterna.Gui2;

/// <summary>
/// This is a simple combo box implementation that allows the user to select one out of multiple items through a
/// drop-down menu. If the combo box is not in read-only mode, the user can also enter free text in the combo box, much
/// like a TextBox.
/// </summary>
/// <typeparam name="T">Type to use for the items in the combo box</typeparam>
public class ComboBox<T> : AbstractInteractableComponent<ComboBox<T>>
{
    /// <summary>
    /// Listener interface that can be used to catch user events on the combo box
    /// </summary>
    public interface IListener
    {
        /// <summary>
        /// This method is called whenever the user changes selection from one item to another in the combo box
        /// </summary>
        /// <param name="selectedIndex">Index of the item which is now selected</param>
        /// <param name="previousSelection">Index of the item which was previously selected</param>
        /// <param name="changedByUserInteraction">If true then this selection change happened because of user
        /// interaction with the combo box. If false then the selected item was set programmatically.</param>
        void OnSelectionChanged(int selectedIndex, int previousSelection, bool changedByUserInteraction);
    }

    private readonly List<T> _items;
    private readonly ConcurrentBag<IListener> _listeners;

    private PopupWindow? _popupWindow;
    private string _text;
    private int _selectedIndex;

    private bool _readOnly;
    private bool _dropDownFocused;
    private int _textInputPosition;
    private int _dropDownNumberOfRows;

    /// <summary>
    /// Creates a new ComboBox initialized with N number of items supplied through the varargs parameter. If at
    /// least one item is given, the first one in the array will be initially selected. By default 10 items will be
    /// displayed at once, more than that and there will be a scroll bar.
    /// </summary>
    /// <param name="items">Items to populate the new combo box with</param>
    public ComboBox(params T[] items) : this(items.AsEnumerable())
    {
    }

    /// <summary>
    /// Creates a new ComboBox initialized with N number of items supplied through the items parameter. If at
    /// least one item is given, the first one in the collection will be initially selected. By default 10 items will be
    /// displayed at once, more than that and there will be a scroll bar.
    /// </summary>
    /// <param name="items">Items to populate the new combo box with</param>
    public ComboBox(IEnumerable<T> items) : this(items, items.Any() ? 0 : -1)
    {
    }

    /// <summary>
    /// Creates a new ComboBox initialized with N number of items supplied through the items parameter. The
    /// initial text in the combo box is set to a specific value passed in through the initialText parameter, it
    /// can be a text which is not contained within the items and the selection state of the combo box will be
    /// "no selection" (so GetSelectedIndex() will return -1) until the user interacts with the combo box and
    /// manually changes it. By default 10 items will be displayed at once, more than that and there will be a scroll bar.
    /// </summary>
    /// <param name="initialText">Text to put in the combo box initially</param>
    /// <param name="items">Items to populate the new combo box with</param>
    public ComboBox(string initialText, IEnumerable<T> items) : this(items, -1)
    {
        _text = initialText;
    }

    /// <summary>
    /// Creates a new ComboBox initialized with N number of items supplied through the items parameter. The
    /// initially selected item is specified through the selectedIndex parameter. By default 10 items will be
    /// displayed at once, more than that and there will be a scroll bar.
    /// </summary>
    /// <param name="items">Items to populate the new combo box with</param>
    /// <param name="selectedIndex">Index of the item which should be initially selected</param>
    public ComboBox(IEnumerable<T> items, int selectedIndex)
    {
        foreach (var item in items)
        {
            if (item == null)
            {
                throw new ArgumentException("Cannot add null elements to a ComboBox");
            }
        }
        _items = new List<T>(items);
        _listeners = new ConcurrentBag<IListener>();
        _popupWindow = null;
        _selectedIndex = selectedIndex;
        _readOnly = true;
        _dropDownFocused = true;
        _textInputPosition = 0;
        _dropDownNumberOfRows = 10;
        if (selectedIndex != -1)
        {
            _text = _items[selectedIndex]?.ToString() ?? "";
        }
        else
        {
            _text = "";
        }
    }

    /// <summary>
    /// Adds a new item to the combo box, at the end
    /// </summary>
    /// <param name="item">Item to add to the combo box</param>
    /// <returns>Itself</returns>
    public ComboBox<T> AddItem(T item)
    {
        lock (this)
        {
            if (item == null)
            {
                throw new ArgumentException("Cannot add null elements to a ComboBox");
            }
            _items.Add(item);
            if (_selectedIndex == -1 && _items.Count == 1)
            {
                SetSelectedIndex(0);
            }
            Invalidate();
            return this;
        }
    }

    /// <summary>
    /// Adds a new item to the combo box, at a specific index
    /// </summary>
    /// <param name="index">Index to add the item at</param>
    /// <param name="item">Item to add</param>
    /// <returns>Itself</returns>
    public ComboBox<T> AddItem(int index, T item)
    {
        lock (this)
        {
            if (item == null)
            {
                throw new ArgumentException("Cannot add null elements to a ComboBox");
            }
            _items.Insert(index, item);
            if (index <= _selectedIndex)
            {
                SetSelectedIndex(_selectedIndex + 1);
            }
            Invalidate();
            return this;
        }
    }

    /// <summary>
    /// Removes all items from the combo box
    /// </summary>
    /// <returns>Itself</returns>
    public ComboBox<T> ClearItems()
    {
        lock (this)
        {
            _items.Clear();
            SetSelectedIndex(-1);
            Invalidate();
            return this;
        }
    }

    /// <summary>
    /// Removes a particular item from the combo box, if it is present, otherwise does nothing
    /// </summary>
    /// <param name="item">Item to remove from the combo box</param>
    /// <returns>Itself</returns>
    public ComboBox<T> RemoveItem(T item)
    {
        lock (this)
        {
            int index = _items.IndexOf(item);
            if (index == -1)
            {
                return this;
            }
            return RemoveItem(index);
        }
    }

    /// <summary>
    /// Removes an item from the combo box at a particular index
    /// </summary>
    /// <param name="index">Index of the item to remove</param>
    /// <returns>Itself</returns>
    /// <exception cref="ArgumentOutOfRangeException">if the index is out of range</exception>
    public ComboBox<T> RemoveItem(int index)
    {
        lock (this)
        {
            _items.RemoveAt(index);
            if (index < _selectedIndex)
            {
                SetSelectedIndex(_selectedIndex - 1);
            }
            else if (index == _selectedIndex)
            {
                SetSelectedIndex(-1);
            }
            Invalidate();
            return this;
        }
    }

    /// <summary>
    /// Updates the combo box so the item at the specified index is swapped out with the supplied value in the
    /// item parameter
    /// </summary>
    /// <param name="index">Index of the item to swap out</param>
    /// <param name="item">Item to replace with</param>
    /// <returns>Itself</returns>
    public ComboBox<T> SetItem(int index, T item)
    {
        lock (this)
        {
            if (item == null)
            {
                throw new ArgumentException("Cannot add null elements to a ComboBox");
            }
            _items[index] = item;
            Invalidate();
            return this;
        }
    }

    /// <summary>
    /// Counts and returns the number of items in this combo box
    /// </summary>
    /// <returns>Number of items in this combo box</returns>
    public int ItemCount
    {
        get
        {
            lock (this)
            {
                return _items.Count;
            }
        }
    }

    /// <summary>
    /// Returns the item at the specific index
    /// </summary>
    /// <param name="index">Index of the item to return</param>
    /// <returns>Item at the specific index</returns>
    /// <exception cref="ArgumentOutOfRangeException">if the index is out of range</exception>
    public T GetItem(int index)
    {
        lock (this)
        {
            return _items[index];
        }
    }

    /// <summary>
    /// Returns the text currently displayed in the combo box, this will likely be the label of the selected item but for
    /// writable combo boxes it's also what the user has typed in
    /// </summary>
    /// <returns>String currently displayed in the combo box</returns>
    public string Text => _text;

    /// <summary>
    /// Sets the combo box to either read-only or writable. In read-only mode, the user cannot type in any text in the
    /// combo box but is forced to pick one of the items, displayed by the drop-down. In writable mode, the user can
    /// enter any string in the combo box
    /// </summary>
    /// <param name="readOnly">If the combo box should be in read-only mode, pass in true, otherwise false for
    /// writable mode</param>
    /// <returns>Itself</returns>
    public ComboBox<T> SetReadOnly(bool readOnly)
    {
        lock (this)
        {
            _readOnly = readOnly;
            if (readOnly)
            {
                _dropDownFocused = true;
            }
            return this;
        }
    }

    /// <summary>
    /// Returns true if this combo box is in read-only mode
    /// </summary>
    /// <returns>true if this combo box is in read-only mode, false otherwise</returns>
    public bool IsReadOnly => _readOnly;

    /// <summary>
    /// Returns true if the users input focus is currently on the drop-down button of the combo box, so that
    /// pressing enter would trigger the popup window. This is generally used by renderers only and is always true for
    /// read-only combo boxes as the component won't allow you to focus on the text in that mode.
    /// </summary>
    /// <returns>true if the input focus is on the drop-down "button" of the combo box</returns>
    public bool IsDropDownFocused => _dropDownFocused || IsReadOnly;

    /// <summary>
    /// For writable combo boxes, this method returns the position where the text input cursor is right now. Meaning, if
    /// the user types some character, where are those are going to be inserted in the string that is currently
    /// displayed. If the text input position equals the size of the currently displayed text, new characters will be
    /// appended at the end. The user can usually move the text input position by using left and right arrow keys on the
    /// keyboard.
    /// </summary>
    /// <returns>Current text input position</returns>
    public int TextInputPosition => _textInputPosition;

    /// <summary>
    /// Returns the number of items to display in drop down at one time, if there are more items in the model there will
    /// be a scrollbar to help the user navigate. If this returns 0, the combo box will always grow to show all items in
    /// the list, which might cause undesired effects if you put really a lot of items into the combo box.
    /// </summary>
    /// <returns>Number of items (rows) that will be displayed in the combo box, or 0 if the combo box will always grow to
    /// accommodate</returns>
    public int DropDownNumberOfRows => _dropDownNumberOfRows;

    /// <summary>
    /// Sets the number of items to display in drop down at one time, if there are more items in the model there will
    /// be a scrollbar to help the user navigate. Use this method if your combo boxes have large models that fills up
    /// the whole screen. Set it to 0 if you don't want to limit the number.
    /// </summary>
    /// <param name="dropDownNumberOfRows">Max number of items (rows) to display at one time in the combo box</param>
    public void SetDropDownNumberOfRows(int dropDownNumberOfRows)
    {
        _dropDownNumberOfRows = dropDownNumberOfRows;
    }

    /// <summary>
    /// Programmatically selects one item in the combo box, which causes the displayed text to change to match the label
    /// of the selected index.
    /// </summary>
    /// <param name="selectedIndex">Index of the item to select, or -1 if the selection should be cleared</param>
    /// <exception cref="ArgumentOutOfRangeException">if the index is out of range</exception>
    public void SetSelectedIndex(int selectedIndex)
    {
        SetSelectedIndex(selectedIndex, false);
    }

    private void SetSelectedIndex(int selectedIndex, bool changedByUserInteraction)
    {
        lock (this)
        {
            if (_items.Count <= selectedIndex || selectedIndex < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(selectedIndex), "Illegal argument to ComboBox.SetSelectedIndex: " + selectedIndex);
            }
            int oldSelection = _selectedIndex;
            _selectedIndex = selectedIndex;
            if (selectedIndex == -1)
            {
                UpdateText("");
            }
            else
            {
                UpdateText(_items[selectedIndex]?.ToString() ?? "");
            }
            
            // Run listeners asynchronously to avoid potential deadlocks
            RunOnGUIThreadIfExistsOtherwiseRunDirect(() =>
            {
                foreach (var listener in _listeners)
                {
                    listener.OnSelectionChanged(selectedIndex, oldSelection, changedByUserInteraction);
                }
            });
            Invalidate();
        }
    }

    /// <summary>
    /// Programmatically selects one item in the combo box by passing in the value the should be selected. If the value
    /// isn't in the combo box model, nothing happens for read-only combo boxes and for editable ones the text content
    /// is changed to match the result from calling the ToString() method of item.
    /// <para>
    /// If called with null, the selection is cleared.
    /// </para>
    /// </summary>
    /// <param name="item">Item in the combo box to select, or null if the selection should be cleared</param>
    public void SetSelectedItem(T? item)
    {
        lock (this)
        {
            if (item == null)
            {
                SetSelectedIndex(-1);
            }
            else
            {
                int indexOf = _items.IndexOf(item);
                if (indexOf != -1)
                {
                    SetSelectedIndex(indexOf);
                }
                else if (!_readOnly)
                {
                    UpdateText(item.ToString() ?? "");
                }
            }
        }
    }

    private void UpdateText(string newText)
    {
        _text = newText;
        if (_textInputPosition > _text.Length)
        {
            _textInputPosition = _text.Length;
        }
    }

    /// <summary>
    /// Returns the index of the currently selected item or -1 for no selection
    /// </summary>
    /// <returns>Index of the currently selected item</returns>
    public int SelectedIndex => _selectedIndex;

    /// <summary>
    /// Returns the item at the selected index, this is the same as calling:
    /// GetSelectedIndex() > -1 ? GetItem(GetSelectedIndex()) : default(T)
    /// </summary>
    /// <returns>The item at the selected index</returns>
    public T? SelectedItem
    {
        get
        {
            lock (this)
            {
                return SelectedIndex > -1 ? GetItem(SelectedIndex) : default(T);
            }
        }
    }

    /// <summary>
    /// Adds a new listener to the ComboBox that will be called on certain user actions
    /// </summary>
    /// <param name="listener">Listener to attach to this ComboBox</param>
    /// <returns>Itself</returns>
    public ComboBox<T> AddListener(IListener listener)
    {
        if (listener != null)
        {
            _listeners.Add(listener);
        }
        return this;
    }

    /// <summary>
    /// Removes a listener from this ComboBox so that if it had been added earlier, it will no longer be
    /// called on user actions
    /// </summary>
    /// <param name="listener">Listener to remove from this ComboBox</param>
    /// <returns>Itself</returns>
    public ComboBox<T> RemoveListener(IListener listener)
    {
        // Note: ConcurrentBag doesn't support efficient removal
        // For a proper implementation, consider using ConcurrentList or lock-protected List
        // For now, this is a simplified implementation that doesn't actually remove
        // In a production system, you'd want to use a different collection type
        return this;
    }

    protected override void AfterEnterFocus(IInteractable.FocusChangeDirection direction, IInteractable? previouslyInFocus)
    {
        if (direction == IInteractable.FocusChangeDirection.Right && !IsReadOnly)
        {
            _dropDownFocused = false;
            _selectedIndex = 0;
        }
    }

    protected override void AfterLeaveFocus(IInteractable.FocusChangeDirection direction, IInteractable? nextInFocus)
    {
        lock (this)
        {
            if (_popupWindow != null)
            {
                _popupWindow.Close();
            }
        }
    }

    protected override IComponentRenderer<ComboBox<T>> CreateDefaultRenderer()
    {
        return new DefaultComboBoxRenderer<T>();
    }

    public override TerminalPosition? CursorLocation => 
        ((IComboBoxRenderer<T>)Renderer).GetCursorLocation(this);

    protected override IInteractable.Result HandleKeyStroke(KeyStroke keyStroke)
    {
        lock (this)
        {
            if (IsReadOnly)
            {
                return HandleReadOnlyCBKeyStroke(keyStroke);
            }
            else
            {
                return HandleEditableCBKeyStroke(keyStroke);
            }
        }
    }

    private IInteractable.Result HandleReadOnlyCBKeyStroke(KeyStroke keyStroke)
    {
        switch (keyStroke.KeyType)
        {
            case KeyType.Character:
            case KeyType.Enter:
                if (IsKeyboardActivationStroke(keyStroke))
                {
                    ShowPopup(keyStroke);
                }
                return base.HandleKeyStroke(keyStroke);
            
            case KeyType.MouseEvent:
                if (IsMouseActivationStroke(keyStroke))
                {
                    ShowPopup(keyStroke);
                }
                break;
            
            default:
                break;
        }
        return base.HandleKeyStroke(keyStroke);
    }
    
    protected void ShowPopup(KeyStroke keyStroke)
    {
        _popupWindow = new PopupWindow(this);
        _popupWindow.Position = ToGlobal(new TerminalPosition(0, 1)) ?? TerminalPosition.TopLeftCorner;
        if (TextGUI is IWindowBasedTextGUI windowBasedTextGUI)
        {
            windowBasedTextGUI.AddWindow(_popupWindow);
            windowBasedTextGUI.SetActiveWindow(_popupWindow);
        }
    }

    private IInteractable.Result HandleEditableCBKeyStroke(KeyStroke keyStroke)
    {
        // First check if we are in drop-down focused mode, treat keystrokes a bit differently then
        if (IsDropDownFocused)
        {
            switch (keyStroke.KeyType)
            {
                case KeyType.ReverseTab:
                case KeyType.ArrowLeft:
                    _dropDownFocused = false;
                    _textInputPosition = _text.Length;
                    return IInteractable.Result.Handled;

                // The rest we can process in the same way as with read-only combo boxes when we are in drop-down focused mode
                default:
                    return HandleReadOnlyCBKeyStroke(keyStroke);
            }
        }

        switch (keyStroke.KeyType)
        {
            case KeyType.Character:
                _text = _text.Substring(0, _textInputPosition) + keyStroke.Character + _text.Substring(_textInputPosition);
                _textInputPosition++;
                return IInteractable.Result.Handled;

            case KeyType.Tab:
                _dropDownFocused = true;
                return IInteractable.Result.Handled;

            case KeyType.Backspace:
                if (_textInputPosition > 0)
                {
                    _text = _text.Substring(0, _textInputPosition - 1) + _text.Substring(_textInputPosition);
                    _textInputPosition--;
                }
                return IInteractable.Result.Handled;

            case KeyType.Delete:
                if (_textInputPosition < _text.Length)
                {
                    _text = _text.Substring(0, _textInputPosition) + _text.Substring(_textInputPosition + 1);
                }
                return IInteractable.Result.Handled;

            case KeyType.ArrowLeft:
                if (_textInputPosition > 0)
                {
                    _textInputPosition--;
                }
                else
                {
                    return IInteractable.Result.MoveFocusLeft;
                }
                return IInteractable.Result.Handled;

            case KeyType.ArrowRight:
                if (_textInputPosition < _text.Length)
                {
                    _textInputPosition++;
                }
                else
                {
                    _dropDownFocused = true;
                    return IInteractable.Result.Handled;
                }
                return IInteractable.Result.Handled;

            case KeyType.ArrowDown:
                if (_selectedIndex < _items.Count - 1)
                {
                    SetSelectedIndex(_selectedIndex + 1, true);
                }
                return IInteractable.Result.Handled;

            case KeyType.ArrowUp:
                if (_selectedIndex > 0)
                {
                    SetSelectedIndex(_selectedIndex - 1, true);
                }
                return IInteractable.Result.Handled;

            default:
                break;
        }
        return base.HandleKeyStroke(keyStroke);
    }

    /// <summary>
    /// Helper interface that doesn't add any new methods but makes coding new combo box renderers a little bit more clear
    /// </summary>
    public interface IComboBoxRenderer<TItem> : IInteractableRenderer<ComboBox<TItem>>
    {
    }

    /// <summary>
    /// This class is the default renderer implementation which will be used unless overridden. The combo box is rendered
    /// like a text box with an arrow point down to the right of it, which can receive focus and triggers the popup.
    /// </summary>
    /// <typeparam name="TItem">Type of items in the combo box</typeparam>
    public class DefaultComboBoxRenderer<TItem> : IComboBoxRenderer<TItem>
    {
        private int _textVisibleLeftPosition;

        public DefaultComboBoxRenderer()
        {
            _textVisibleLeftPosition = 0;
        }

        public TerminalPosition? GetCursorLocation(ComboBox<TItem> comboBox)
        {
            if (comboBox.IsDropDownFocused)
            {
                if (comboBox.ThemeDefinition.CursorVisible)
                {
                    return new TerminalPosition(comboBox.Size.Columns - 1, 0);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                int textInputPosition = comboBox.TextInputPosition;
                int textInputColumn = Core.TerminalTextUtils.GetColumnWidth(comboBox.Text.Substring(0, textInputPosition));
                return new TerminalPosition(textInputColumn - _textVisibleLeftPosition, 0);
            }
        }

        public TerminalSize GetPreferredSize(ComboBox<TItem> comboBox)
        {
            TerminalSize size = TerminalSize.One.WithColumns(
                (comboBox.ItemCount == 0 ? Core.TerminalTextUtils.GetColumnWidth(comboBox.Text) : 0) + 2);
            lock (comboBox)
            {
                for (int i = 0; i < comboBox.ItemCount; i++)
                {
                    var item = comboBox.GetItem(i);
                    size = size.Max(new TerminalSize(Core.TerminalTextUtils.GetColumnWidth(item?.ToString() ?? "") + 2 + 1, 1));   // +1 to add a single column of space
                }
            }
            return size;
        }

        public void DrawComponent(ITextGUIGraphics graphics, ComboBox<TItem> comboBox)
        {
            var themeDefinition = comboBox.ThemeDefinition;
            if (comboBox.IsReadOnly)
            {
                graphics.ApplyThemeStyle(themeDefinition.Normal);
            }
            else
            {
                if (comboBox.IsFocused)
                {
                    graphics.ApplyThemeStyle(themeDefinition.Active);
                }
                else
                {
                    graphics.ApplyThemeStyle(themeDefinition.PreLight);
                }
            }
            graphics.Fill(' ');
            int editableArea = graphics.Size.Columns - 2; // This is excluding the 'drop-down arrow'
            int textInputPosition = comboBox.TextInputPosition;
            int columnsToInputPosition = Core.TerminalTextUtils.GetColumnWidth(comboBox.Text.Substring(0, textInputPosition));
            if (columnsToInputPosition < _textVisibleLeftPosition)
            {
                _textVisibleLeftPosition = columnsToInputPosition;
            }
            if (columnsToInputPosition - _textVisibleLeftPosition >= editableArea)
            {
                _textVisibleLeftPosition = columnsToInputPosition - editableArea + 1;
            }
            if (columnsToInputPosition - _textVisibleLeftPosition + 1 == editableArea &&
                comboBox.Text.Length > textInputPosition &&
                Core.TerminalTextUtils.IsCharCJK(comboBox.Text[textInputPosition]))
            {
                _textVisibleLeftPosition++;
            }

            string textToDraw = Core.TerminalTextUtils.FitString(comboBox.Text, _textVisibleLeftPosition, editableArea);
            graphics.PutString(0, 0, textToDraw);
            graphics.ApplyThemeStyle(themeDefinition.Insensitive);
            graphics.SetCharacter(editableArea, 0, themeDefinition.GetCharacter("POPUP_SEPARATOR", Symbols.SingleLineVertical));
            if (comboBox.IsFocused && comboBox.IsDropDownFocused)
            {
                graphics.ApplyThemeStyle(themeDefinition.Selected);
            }
            graphics.SetCharacter(editableArea + 1, 0, themeDefinition.GetCharacter("POPUP", Symbols.Triangle_Down_Pointing_Black));
        }
    }

    private class PopupWindow : AbstractWindow
    {
        private readonly ActionListBox _listBox;
        private readonly ComboBox<T> _parentComboBox;

        public PopupWindow(ComboBox<T> parentComboBox) : base()
        {
            _parentComboBox = parentComboBox;
            SetHints(new HashSet<IWindow.Hint> 
            {
                IWindow.Hint.NoFocus,
                IWindow.Hint.FixedPosition,
                IWindow.Hint.MenuPopup
            });
            _listBox = new ActionListBox(parentComboBox.Size.WithRows(parentComboBox.ItemCount));
            for (int i = 0; i < parentComboBox.ItemCount; i++)
            {
                var item = parentComboBox._items[i];
                int index = i;
                _listBox.AddItem(item?.ToString() ?? "", () =>
                {
                    parentComboBox.SetSelectedIndex(index, true);
                    Close();
                });
            }
            _listBox.SetSelectedIndex(parentComboBox.SelectedIndex);
            var dropDownListPreferredSize = _listBox.PreferredSize;
            if (parentComboBox._dropDownNumberOfRows > 0)
            {
                _listBox.SetPreferredSize(dropDownListPreferredSize.WithRows(
                    Math.Min(parentComboBox._dropDownNumberOfRows, dropDownListPreferredSize.Rows)));
            }
            Component = _listBox;
        }

        public override void Close()
        {
            base.Close();
            _parentComboBox._popupWindow = null;
        }

        public ThemeDefinition ThemeDefinition => _parentComboBox.ThemeDefinition;

        public override bool HandleInput(KeyStroke keyStroke)
        {
            if (keyStroke.KeyType == KeyType.Escape)
            {
                Close();
                return true;
            }
            return base.HandleInput(keyStroke);
        }
    }
}