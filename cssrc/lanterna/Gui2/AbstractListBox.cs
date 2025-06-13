using Lanterna.Core;
using Lanterna.Graphics;
using Lanterna.Input;

namespace Lanterna.Gui2;

/// <summary>
/// Base class for several list box implementations, this will handle things like list of items and the scrollbar.
/// </summary>
/// <typeparam name="V">Type of items this list box contains</typeparam>
/// <typeparam name="T">Should always be itself, see AbstractComponent</typeparam>
public abstract class AbstractListBox<V, T> : AbstractInteractableComponent<T>
    where T : AbstractListBox<V, T>, IInteractable
{
    private readonly List<V> _items;
    private int _selectedIndex;
    private ListItemRenderer<V, T> _listItemRenderer;
    protected TerminalPosition _scrollOffset = new TerminalPosition(0, 0);
    private readonly object _lock = new object();

    /// <summary>
    /// This constructor sets up the component so it has no preferred size but will ask to be as big as the list is. If
    /// the GUI cannot accommodate this size, scrolling and a vertical scrollbar will be used.
    /// </summary>
    protected AbstractListBox() : this(null)
    {
    }

    /// <summary>
    /// This constructor sets up the component with a preferred size that is will always request, no matter what items
    /// are in the list box. If there are more items than the size can contain, scrolling and a vertical scrollbar will
    /// be used. Calling this constructor with a null value has the same effect as calling the default constructor.
    /// </summary>
    /// <param name="size">Preferred size that the list should be asking for instead of invoking the preferred size calculation,
    ///              or if set to null will ask to be big enough to display all items.</param>
    protected AbstractListBox(TerminalSize? size)
    {
        _items = new List<V>();
        _selectedIndex = -1;
        SetPreferredSize(size);
        SetListItemRenderer(CreateDefaultListItemRenderer());
    }

    protected override IComponentRenderer<T> CreateDefaultRenderer()
    {
        return new DefaultListBoxRenderer<V, T>();
    }

    /// <summary>
    /// Method that constructs the ListItemRenderer that this list box should use to draw the elements of the
    /// list box. This can be overridden to supply a custom renderer. Note that this is not the renderer used for the
    /// entire list box but for each item, called one by one.
    /// </summary>
    /// <returns>ListItemRenderer to use when drawing the items in the list</returns>
    protected virtual ListItemRenderer<V, T> CreateDefaultListItemRenderer()
    {
        return new ListItemRenderer<V, T>();
    }

    internal ListItemRenderer<V, T> GetListItemRenderer()
    {
        return _listItemRenderer;
    }

    /// <summary>
    /// This method overrides the ListItemRenderer that is used to draw each element in the list box. Note that
    /// this is not the renderer used for the entire list box but for each item, called one by one.
    /// </summary>
    /// <param name="listItemRenderer">New renderer to use when drawing the items in the list box</param>
    /// <returns>Itself</returns>
    public T SetListItemRenderer(ListItemRenderer<V, T>? listItemRenderer)
    {
        lock (_lock)
        {
            if (listItemRenderer == null)
            {
                listItemRenderer = CreateDefaultListItemRenderer();
                if (listItemRenderer == null)
                {
                    throw new InvalidOperationException("CreateDefaultListItemRenderer returned null");
                }
            }
            _listItemRenderer = listItemRenderer;
            return Self;
        }
    }

    protected override IInteractable.Result HandleKeyStroke(KeyStroke keyStroke)
    {
        try
        {
            switch (keyStroke.KeyType)
            {
                case KeyType.Tab:
                    return IInteractable.Result.MoveFocusNext;

                case KeyType.ReverseTab:
                    return IInteractable.Result.MoveFocusPrevious;

                case KeyType.ArrowRight:
                    return IInteractable.Result.MoveFocusRight;

                case KeyType.ArrowLeft:
                    return IInteractable.Result.MoveFocusLeft;

                case KeyType.ArrowDown:
                    if (_items.Count == 0 || _selectedIndex == _items.Count - 1)
                    {
                        return IInteractable.Result.MoveFocusDown;
                    }
                    _selectedIndex++;
                    return IInteractable.Result.Handled;

                case KeyType.ArrowUp:
                    if (_items.Count == 0 || _selectedIndex == 0)
                    {
                        return IInteractable.Result.MoveFocusUp;
                    }
                    _selectedIndex--;
                    return IInteractable.Result.Handled;

                case KeyType.Home:
                    _selectedIndex = 0;
                    return IInteractable.Result.Handled;

                case KeyType.End:
                    _selectedIndex = _items.Count - 1;
                    return IInteractable.Result.Handled;

                case KeyType.PageUp:
                    if (Size.Rows > 0)
                    {
                        SetSelectedIndex(GetSelectedIndex() - Size.Rows);
                    }
                    return IInteractable.Result.Handled;

                case KeyType.PageDown:
                    if (Size.Rows > 0)
                    {
                        SetSelectedIndex(GetSelectedIndex() + Size.Rows);
                    }
                    return IInteractable.Result.Handled;

                case KeyType.Character:
                    if (SelectByCharacter(keyStroke.Character))
                    {
                        return IInteractable.Result.Handled;
                    }
                    return IInteractable.Result.Unhandled;

                case KeyType.MouseEvent:
                    var mouseAction = (MouseAction)keyStroke;
                    var actionType = mouseAction.ActionType;
                    if (IsMouseMove(keyStroke))
                    {
                        TakeFocus();
                        _selectedIndex = GetIndexByMouseAction(mouseAction);
                        return IInteractable.Result.Handled;
                    }

                    if (actionType == MouseActionType.ClickRelease)
                    {
                        // do nothing, desired actioning has been performed already on CLICK_DOWN and DRAG
                        return IInteractable.Result.Handled;
                    }
                    else if (actionType == MouseActionType.ScrollUp)
                    {
                        // relying on setSelectedIndex(index) to clip the index to valid values within range
                        SetSelectedIndex(GetSelectedIndex() - 1);
                        return IInteractable.Result.Handled;
                    }
                    else if (actionType == MouseActionType.ScrollDown)
                    {
                        // relying on setSelectedIndex(index) to clip the index to valid values within range
                        SetSelectedIndex(GetSelectedIndex() + 1);
                        return IInteractable.Result.Handled;
                    }

                    _selectedIndex = GetIndexByMouseAction(mouseAction);
                    return base.HandleKeyStroke(keyStroke);

                default:
                    break;
            }
            return IInteractable.Result.Unhandled;
        }
        finally
        {
            Invalidate();
        }
    }

    /// <summary>
    /// By converting TerminalPositions to global coordinates gets index clicked on by mouse action.
    /// </summary>
    /// <param name="click">Mouse action</param>
    /// <returns>index of a item that was clicked on with MouseAction</returns>
    protected int GetIndexByMouseAction(MouseAction click)
    {
        int index = click.Position.Row - GlobalPosition.Row - _scrollOffset.Row;
        return Math.Min(index, _items.Count - 1);
    }

    private bool SelectByCharacter(char? character)
    {
        if (character == null) return false;
        
        char lowerChar = char.ToLower(character.Value);
        int selectedIndex = GetSelectedIndex();
        
        for (int i = 0; i < ItemCount; i++)
        {
            int index = (selectedIndex + i + 1) % ItemCount;
            V? item = GetItemAt(index);
            string? label = item?.ToString();
            if (!string.IsNullOrEmpty(label) && label.Length > 0)
            {
                char firstChar = char.ToLower(label[0]);
                if (firstChar == lowerChar)
                {
                    SetSelectedIndex(index);
                    return true;
                }
            }
        }
        
        return false;
    }

    protected override void AfterEnterFocus(IInteractable.FocusChangeDirection direction, IInteractable? previouslyInFocus)
    {
        if (_items.Count == 0)
        {
            return;
        }

        if (direction == IInteractable.FocusChangeDirection.Down)
        {
            _selectedIndex = 0;
        }
        else if (direction == IInteractable.FocusChangeDirection.Up)
        {
            _selectedIndex = _items.Count - 1;
        }
    }

    /// <summary>
    /// Adds one more item to the list box, at the end.
    /// </summary>
    /// <param name="item">Item to add to the list box</param>
    /// <returns>Itself</returns>
    public T AddItem(V? item)
    {
        if (item == null)
        {
            return Self;
        }

        lock (_lock)
        {
            _items.Add(item);
            if (_selectedIndex == -1)
            {
                _selectedIndex = 0;
            }
            Invalidate();
            return Self;
        }
    }

    /// <summary>
    /// Removes an item from the list box by its index. The current selection in the list box will be adjusted
    /// accordingly.
    /// </summary>
    /// <param name="index">Index of the item to remove</param>
    /// <returns>The item that was removed</returns>
    /// <exception cref="ArgumentOutOfRangeException">if the index is out of bounds in regards to the list of items</exception>
    public virtual V RemoveItem(int index)
    {
        lock (_lock)
        {
            V existing = _items[index];
            _items.RemoveAt(index);
            if (index < _selectedIndex)
            {
                _selectedIndex--;
            }
            while (_selectedIndex >= _items.Count)
            {
                _selectedIndex--;
            }
            Invalidate();
            return existing;
        }
    }

    /// <summary>
    /// Removes all items from the list box
    /// </summary>
    /// <returns>Itself</returns>
    public virtual T ClearItems()
    {
        lock (_lock)
        {
            _items.Clear();
            _selectedIndex = -1;
            Invalidate();
            return Self;
        }
    }

    public override bool IsFocusable
    {
        get
        {
            if (IsEmpty)
            {
                // These dialog boxes are quite weird when they are empty and receive input focus, so try to avoid that
                return false;
            }
            return base.IsFocusable;
        }
    }

    /// <summary>
    /// Looks for the particular item in the list and returns the index within the list (starting from zero) of that item
    /// if it is found, or -1 otherwise
    /// </summary>
    /// <param name="item">What item to search for in the list box</param>
    /// <returns>Index of the item in the list box or -1 if the list box does not contain the item</returns>
    public int IndexOf(V item)
    {
        lock (_lock)
        {
            return _items.IndexOf(item);
        }
    }

    /// <summary>
    /// Retrieves the item at the specified index in the list box
    /// </summary>
    /// <param name="index">Index of the item to fetch</param>
    /// <returns>The item at the specified index</returns>
    /// <exception cref="ArgumentOutOfRangeException">If the index is less than zero or equals/greater than the number of items in the list box</exception>
    public V GetItemAt(int index)
    {
        lock (_lock)
        {
            return _items[index];
        }
    }

    /// <summary>
    /// Checks if the list box has no items
    /// </summary>
    /// <returns>true if the list box has no items, false otherwise</returns>
    public bool IsEmpty
    {
        get
        {
            lock (_lock)
            {
                return _items.Count == 0;
            }
        }
    }

    /// <summary>
    /// Returns the number of items currently in the list box
    /// </summary>
    /// <returns>Number of items in the list box</returns>
    public int ItemCount
    {
        get
        {
            lock (_lock)
            {
                return _items.Count;
            }
        }
    }

    /// <summary>
    /// Returns a copy of the items in the list box as a List
    /// </summary>
    /// <returns>Copy of all the items in this list box</returns>
    public List<V> GetItems()
    {
        lock (_lock)
        {
            return new List<V>(_items);
        }
    }

    /// <summary>
    /// Sets which item in the list box that is currently selected. Please note that in this context, selected simply
    /// means it is the item that currently has input focus. This is not to be confused with list box implementations
    /// such as CheckBoxList where individual items have a certain checked/unchecked state.
    /// This method will clip the supplied index to within 0 to items.Count - 1.
    /// </summary>
    /// <param name="index">Index of the item that should be currently selected</param>
    /// <returns>Itself</returns>
    public T SetSelectedIndex(int index)
    {
        lock (_lock)
        {
            _selectedIndex = Math.Max(0, Math.Min(index, _items.Count - 1));
            Invalidate();
            return Self;
        }
    }

    /// <summary>
    /// Returns the index of the currently selected item in the list box. Please note that in this context, selected
    /// simply means it is the item that currently has input focus. This is not to be confused with list box
    /// implementations such as CheckBoxList where individual items have a certain checked/unchecked state.
    /// </summary>
    /// <returns>The index of the currently selected row in the list box, or -1 if there are no items</returns>
    public int GetSelectedIndex()
    {
        return _selectedIndex;
    }

    /// <summary>
    /// Returns the currently selected item in the list box. Please note that in this context, selected
    /// simply means it is the item that currently has input focus. This is not to be confused with list box
    /// implementations such as CheckBoxList where individual items have a certain checked/unchecked state.
    /// </summary>
    /// <returns>The currently selected item in the list box, or null if there are no items</returns>
    public V? GetSelectedItem()
    {
        lock (_lock)
        {
            if (_selectedIndex == -1 || _selectedIndex >= _items.Count)
            {
                return default(V);
            }
            else
            {
                return _items[_selectedIndex];
            }
        }
    }

    /// <summary>
    /// The default renderer for AbstractListBox and all its subclasses.
    /// </summary>
    /// <typeparam name="VType">Type of the items the list box this renderer is for</typeparam>
    /// <typeparam name="TType">Type of list box</typeparam>
    public class DefaultListBoxRenderer<VType, TType> : IInteractableRenderer<TType>
        where TType : AbstractListBox<VType, TType>, IInteractable
    {
        private readonly ScrollBar _verticalScrollBar;
        private int _scrollTopIndex;

        /// <summary>
        /// Default constructor
        /// </summary>
        public DefaultListBoxRenderer()
        {
            _verticalScrollBar = new ScrollBar(Direction.Vertical);
            _scrollTopIndex = 0;
        }

        public TerminalPosition? GetCursorLocation(TType listBox)
        {
            if (!listBox.ThemeDefinition.CursorVisible)
            {
                return null;
            }
            int selectedIndex = listBox.GetSelectedIndex();
            int columnAccordingToRenderer = listBox.GetListItemRenderer().GetHotSpotPositionOnLine(selectedIndex);
            if (columnAccordingToRenderer == -1)
            {
                return null;
            }
            return new TerminalPosition(columnAccordingToRenderer, selectedIndex - _scrollTopIndex);
        }

        public TerminalSize GetPreferredSize(TType listBox)
        {
            int maxWidth = 5;   //Set it to something...
            int index = 0;
            foreach (var item in listBox.GetItems())
            {
                string itemString = listBox.GetListItemRenderer().GetLabel(listBox, index++, item);
                int stringLengthInColumns = TerminalTextUtils.GetColumnWidth(itemString);
                if (stringLengthInColumns > maxWidth)
                {
                    maxWidth = stringLengthInColumns;
                }
            }
            return new TerminalSize(maxWidth + 1, listBox.ItemCount);
        }

        public void DrawComponent(ITextGUIGraphics graphics, TType listBox)
        {
            //update the page size, used for page up and page down keys
            var themeDefinition = listBox.Theme.GetDefinition(typeof(AbstractListBox<,>));
            int componentHeight = graphics.Size.Rows;
            //int componentWidth = graphics.Size.Columns;
            int selectedIndex = listBox.GetSelectedIndex();
            var items = listBox.GetItems();
            var listItemRenderer = listBox.GetListItemRenderer();

            if (selectedIndex != -1)
            {
                if (selectedIndex < _scrollTopIndex)
                    _scrollTopIndex = selectedIndex;
                else if (selectedIndex >= componentHeight + _scrollTopIndex)
                    _scrollTopIndex = selectedIndex - componentHeight + 1;
            }

            //Do we need to recalculate the scroll position?
            //This code would be triggered by resizing the window when the scroll
            //position is at the bottom
            if (items.Count > componentHeight &&
                items.Count - _scrollTopIndex < componentHeight)
            {
                _scrollTopIndex = items.Count - componentHeight;
            }

            listBox._scrollOffset = new TerminalPosition(0, -_scrollTopIndex);

            graphics.ApplyThemeStyle(themeDefinition.Normal);
            graphics.Fill(' ');

            var itemSize = graphics.Size.WithRows(1);
            for (int i = _scrollTopIndex; i < items.Count; i++)
            {
                if (i - _scrollTopIndex >= componentHeight)
                {
                    break;
                }
                listItemRenderer.DrawItem(
                    graphics.NewTextGraphics(new TerminalPosition(0, i - _scrollTopIndex), itemSize),
                    listBox,
                    i,
                    items[i],
                    selectedIndex == i,
                    listBox.IsFocused);
            }

            graphics.ApplyThemeStyle(themeDefinition.Normal);
            if (items.Count > componentHeight)
            {
                _verticalScrollBar.OnAdded(listBox.Parent);
                _verticalScrollBar.SetViewSize(componentHeight);
                _verticalScrollBar.SetScrollMaximum(items.Count);
                _verticalScrollBar.SetScrollPosition(_scrollTopIndex);
                _verticalScrollBar.Draw(graphics.NewTextGraphics(
                    new TerminalPosition(graphics.Size.Columns - 1, 0),
                    new TerminalSize(1, graphics.Size.Rows)));
            }
        }
    }

    /// <summary>
    /// The default list item renderer class, this can be extended and customized it needed. The instance which is
    /// assigned to the list box will be called once per item in the list when the list box is drawn.
    /// </summary>
    /// <typeparam name="VType">Type of the items in the list box</typeparam>
    /// <typeparam name="TType">Type of the list box class itself</typeparam>
    public class ListItemRenderer<VType, TType>
        where TType : AbstractListBox<VType, TType>, IInteractable
    {
        /// <summary>
        /// Returns where on the line to place the text terminal cursor for a currently selected item. By default this
        /// will return 0, meaning the first character of the selected line. If you extend ListItemRenderer you
        /// can change this by returning a different number. Returning -1 will cause lanterna to hide the cursor.
        /// </summary>
        /// <param name="selectedIndex">Which item is currently selected</param>
        /// <returns>Index of the character in the string we want to place the terminal cursor on, or -1 to hide it</returns>
        public virtual int GetHotSpotPositionOnLine(int selectedIndex)
        {
            return 0;
        }

        /// <summary>
        /// Given a list box, an index of an item within that list box and what the item is, this method should return
        /// what to draw for that item. The default implementation is to return whatever ToString() returns when
        /// called on the item.
        /// </summary>
        /// <param name="listBox">List box the item belongs to</param>
        /// <param name="index">Index of the item</param>
        /// <param name="item">The item itself</param>
        /// <returns>String to draw for this item</returns>
        public virtual string GetLabel(TType listBox, int index, VType? item)
        {
            return item?.ToString() ?? "<null>";
        }

        /// <summary>
        /// This is the main drawing method for a single list box item, it applies the current theme to setup the colors
        /// and then calls GetLabel(..) and draws the result using the supplied ITextGUIGraphics. The
        /// graphics object is created just for this item and is restricted so that it can only draw on the area this
        /// item is occupying. The top-left corner (0x0) should be the starting point when drawing the item.
        /// </summary>
        /// <param name="graphics">Graphics object to draw with</param>
        /// <param name="listBox">List box we are drawing an item from</param>
        /// <param name="index">Index of the item we are drawing</param>
        /// <param name="item">The item we are drawing</param>
        /// <param name="selected">Will be set to true if the item is currently selected, otherwise false, but
        ///                  please notice what context 'selected' refers to here (see SetSelectedIndex)</param>
        /// <param name="focused">Will be set to true if the list box currently has input focus, otherwise false</param>
        public virtual void DrawItem(ITextGUIGraphics graphics, TType listBox, int index, VType? item, bool selected, bool focused)
        {
            var themeDefinition = listBox.Theme.GetDefinition(typeof(AbstractListBox<,>));
            if (selected && focused)
            {
                graphics.ApplyThemeStyle(themeDefinition.Selected);
            }
            else
            {
                graphics.ApplyThemeStyle(themeDefinition.Normal);
            }
            string label = GetLabel(listBox, index, item);
            label = TerminalTextUtils.FitString(label, graphics.Size.Columns);
            while (TerminalTextUtils.GetColumnWidth(label) < graphics.Size.Columns)
            {
                label += " ";
            }
            graphics.PutString(0, 0, label);
        }
    }
}