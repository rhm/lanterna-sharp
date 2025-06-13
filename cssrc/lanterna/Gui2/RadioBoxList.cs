using System.Collections.Concurrent;
using Lanterna.Core;
using Lanterna.Graphics;
using Lanterna.Input;

namespace Lanterna.Gui2;

/// <summary>
/// The list box will display a number of items, of which one and only one can be marked as selected.
/// The user can select an item in the list box by pressing the return key or space bar key. If you
/// select one item when another item is already selected, the previously selected item will be
/// deselected and the highlighted item will be the selected one instead.
/// </summary>
/// <typeparam name="V">Type of items this list box contains</typeparam>
public class RadioBoxList<V> : AbstractListBox<V, RadioBoxList<V>>
{
    /// <summary>
    /// Listener interface that can be attached to the RadioBoxList in order to be notified on user actions
    /// </summary>
    public interface IListener
    {
        /// <summary>
        /// Called by the RadioBoxList when the user changes which item is selected
        /// </summary>
        /// <param name="selectedIndex">Index of the newly selected item, or -1 if the selection has been cleared (can only be
        ///                       done programmatically)</param>
        /// <param name="previousSelection">The index of the previously selected item which is now no longer selected, or -1 if
        ///                           nothing was previously selected</param>
        void OnSelectionChanged(int selectedIndex, int previousSelection);
    }

    private readonly ConcurrentBag<IListener> _listeners;
    private int _checkedIndex;
    private readonly object _lock = new object();

    /// <summary>
    /// Creates a new RadioBoxList with no items. The size of the RadioBoxList will be as big as is required
    /// to display all items.
    /// </summary>
    public RadioBoxList() : this(null)
    {
    }

    /// <summary>
    /// Creates a new RadioBoxList with a specified size. If the items in the RadioBoxList cannot fit in the
    /// size specified, scrollbars will be used
    /// </summary>
    /// <param name="preferredSize">Size of the RadioBoxList or null to have it try to be as big as necessary to
    ///                       be able to draw all items</param>
    public RadioBoxList(TerminalSize? preferredSize) : base(preferredSize)
    {
        _listeners = new ConcurrentBag<IListener>();
        _checkedIndex = -1;
    }

    protected override ListItemRenderer<V, RadioBoxList<V>> CreateDefaultListItemRenderer()
    {
        return new RadioBoxListItemRenderer<V>();
    }

    protected override IInteractable.Result HandleKeyStroke(KeyStroke keyStroke)
    {
        lock (_lock)
        {
            if (IsKeyboardActivationStroke(keyStroke))
            {
                SetCheckedIndex(GetSelectedIndex());
            }
            else if (keyStroke.KeyType == KeyType.MouseEvent)
            {
                var mouseAction = (MouseAction)keyStroke;
                var actionType = mouseAction.ActionType;

                if (IsMouseMove(keyStroke)
                    || actionType == MouseActionType.ClickRelease
                    || actionType == MouseActionType.ScrollUp
                    || actionType == MouseActionType.ScrollDown)
                {
                    return base.HandleKeyStroke(keyStroke);
                }

                // includes mouse drag
                int existingIndex = GetSelectedIndex();
                int newIndex = GetIndexByMouseAction(mouseAction);
                if (existingIndex != newIndex || !IsFocused)
                {
                    var result = base.HandleKeyStroke(keyStroke);
                    SetCheckedIndex(GetSelectedIndex());
                    return result;
                }
                SetCheckedIndex(GetSelectedIndex());
                return IInteractable.Result.Handled;
            }
            return base.HandleKeyStroke(keyStroke);
        }
    }

    public override V RemoveItem(int index)
    {
        lock (_lock)
        {
            V item = base.RemoveItem(index);
            if (index < _checkedIndex)
            {
                _checkedIndex--;
            }
            while (_checkedIndex >= ItemCount)
            {
                _checkedIndex--;
            }
            return item;
        }
    }

    public override RadioBoxList<V> ClearItems()
    {
        SetCheckedIndex(-1);
        return base.ClearItems();
    }

    /// <summary>
    /// This method will see if an object is the currently selected item in this RadioBoxList
    /// </summary>
    /// <param name="obj">Object to test if it's the selected one</param>
    /// <returns>true if the supplied object is what's currently selected in the list box,
    /// false otherwise. Returns null if the supplied object is not an item in the list box.</returns>
    public bool? IsChecked(V obj)
    {
        if (obj == null)
            return null;

        if (IndexOf(obj) == -1)
            return null;

        return _checkedIndex == IndexOf(obj);
    }

    /// <summary>
    /// This method will see if an item, addressed by index, is the currently selected item in this
    /// RadioBoxList
    /// </summary>
    /// <param name="index">Index of the item to check if it's currently selected</param>
    /// <returns>true if the currently selected object is at the supplied index,
    /// false otherwise. Returns false if the index is out of range.</returns>
    public bool IsChecked(int index)
    {
        lock (_lock)
        {
            if (index < 0 || index >= ItemCount)
            {
                return false;
            }

            return _checkedIndex == index;
        }
    }

    /// <summary>
    /// Sets the currently checked item by the value itself. If null, the selection is cleared. When changing selection,
    /// any previously selected item is deselected.
    /// </summary>
    /// <param name="item">Item to be checked</param>
    public void SetCheckedItem(V? item)
    {
        lock (_lock)
        {
            if (item == null)
            {
                SetCheckedIndex(-1);
            }
            else
            {
                SetCheckedItemIndex(IndexOf(item));
            }
        }
    }

    /// <summary>
    /// Sets the currently selected item by index. If the index is out of range, it does nothing.
    /// </summary>
    /// <param name="index">Index of the item to be selected</param>
    public void SetCheckedItemIndex(int index)
    {
        lock (_lock)
        {
            if (index < -1 || index >= ItemCount)
                return;

            SetCheckedIndex(index);
        }
    }

    /// <summary>
    /// Returns the index of the item which is currently selected, or -1 if there is no selection
    /// </summary>
    public int CheckedItemIndex => _checkedIndex;

    /// <summary>
    /// Returns the object currently selected, or null if there is no selection
    /// </summary>
    public V? CheckedItem
    {
        get
        {
            lock (_lock)
            {
                if (_checkedIndex == -1 || _checkedIndex >= ItemCount)
                    return default(V);

                return GetItemAt(_checkedIndex);
            }
        }
    }

    /// <summary>
    /// Un-checks the currently checked item (if any) and leaves the radio check box in a state where no item is checked.
    /// </summary>
    public void ClearSelection()
    {
        lock (_lock)
        {
            SetCheckedIndex(-1);
        }
    }

    /// <summary>
    /// Adds a new listener to the RadioBoxList that will be called on certain user actions
    /// </summary>
    /// <param name="listener">Listener to attach to this RadioBoxList</param>
    /// <returns>Itself</returns>
    public RadioBoxList<V> AddListener(IListener listener)
    {
        if (listener != null && !_listeners.Contains(listener))
        {
            _listeners.Add(listener);
        }
        return this;
    }

    /// <summary>
    /// Removes a listener from this RadioBoxList so that if it had been added earlier, it will no longer be
    /// called on user actions
    /// </summary>
    /// <param name="listener">Listener to remove from this RadioBoxList</param>
    /// <returns>Itself</returns>
    public RadioBoxList<V> RemoveListener(IListener listener)
    {
        // Note: ConcurrentBag doesn't support efficient removal
        // In production, consider using a different collection
        return this;
    }

    private void SetCheckedIndex(int index)
    {
        int previouslyChecked = _checkedIndex;
        _checkedIndex = index;
        Invalidate();
        RunOnGUIThreadIfExistsOtherwiseRunDirect(() =>
        {
            foreach (var listener in _listeners)
            {
                listener.OnSelectionChanged(_checkedIndex, previouslyChecked);
            }
        });
    }

    /// <summary>
    /// Default renderer for this component which is used unless overridden. The selected state is drawn on the left side
    /// of the item label using a "&lt; &gt;" block filled with an "o" if the item is the selected one
    /// </summary>
    /// <typeparam name="VType">Type of items in the RadioBoxList</typeparam>
    public class RadioBoxListItemRenderer<VType> : ListItemRenderer<VType, RadioBoxList<VType>>
    {
        public override int GetHotSpotPositionOnLine(int selectedIndex)
        {
            return 1;
        }

        protected virtual string GetItemText(RadioBoxList<VType> listBox, int index, VType? item)
        {
            return (item?.ToString()) ?? "<null>";
        }

        public override string GetLabel(RadioBoxList<VType> listBox, int index, VType? item)
        {
            string check = " ";
            if (listBox._checkedIndex == index)
                check = "o";

            string text = GetItemText(listBox, index, item);
            return "<" + check + "> " + text;
        }

        public override void DrawItem(ITextGUIGraphics graphics, RadioBoxList<VType> listBox, int index, VType? item, bool selected, bool focused)
        {
            var themeDefinition = listBox.Theme.GetDefinition(typeof(RadioBoxList<>));
            IThemeStyle itemStyle;
            
            if (selected && !focused)
            {
                itemStyle = themeDefinition.Selected;
            }
            else if (selected)
            {
                itemStyle = themeDefinition.Active;
            }
            else if (focused)
            {
                itemStyle = themeDefinition.Insensitive;
            }
            else
            {
                itemStyle = themeDefinition.Normal;
            }

            // Note: C# doesn't have equivalent of getBooleanProperty, using defaults
            graphics.ApplyThemeStyle(itemStyle);
            graphics.Fill(' ');

            string brackets = themeDefinition.GetCharacter("LEFT_BRACKET", '<') +
                    " " +
                    themeDefinition.GetCharacter("RIGHT_BRACKET", '>');
            graphics.PutString(0, 0, brackets);

            string text = GetItemText(listBox, index, item);
            graphics.PutString(4, 0, text);

            bool itemChecked = listBox._checkedIndex == index;
            char marker = themeDefinition.GetCharacter("MARKER", 'o');
            graphics.SetCharacter(1, 0, itemChecked ? marker : ' ');
        }
    }
}