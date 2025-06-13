using Lanterna.Core;
using Lanterna.Input;

namespace Lanterna.Gui2;

/// <summary>
/// Interface that represents an action that can be executed
/// </summary>
public interface IActionItem
{
    void Execute();
}

/// <summary>
/// This class is a list box implementation that displays a number of items that has actions associated with them. You
/// can activate this action by pressing the Enter or Space keys on the keyboard and the action associated with the
/// currently selected item will fire.
/// </summary>
public class ActionListBox : AbstractListBox<IActionItem, ActionListBox>
{
    /// <summary>
    /// Default constructor, creates an ActionListBox with no pre-defined size that will request to be big enough
    /// to display all items
    /// </summary>
    public ActionListBox() : this(null)
    {
    }

    /// <summary>
    /// Creates a new ActionListBox with a pre-set size. If the items don't fit in within this size, scrollbars
    /// will be used to accommodate. Calling new ActionListBox(null) has the same effect as calling
    /// new ActionListBox().
    /// </summary>
    /// <param name="preferredSize">Preferred size of this ActionListBox</param>
    public ActionListBox(TerminalSize? preferredSize) : base(preferredSize)
    {
    }

    /// <summary>
    /// The label of the item in the list box will be the result of calling .ToString() on the action, which
    /// might not be what you want to have unless you explicitly declare it. Consider using
    /// AddItem(string label, Action action) instead, if you want to just set the label easily without having
    /// to override .ToString().
    /// </summary>
    /// <param name="item">IActionItem to execute when the action was selected and fired in the list</param>
    /// <returns>Itself</returns>
    public new ActionListBox AddItem(IActionItem? item)
    {
        return (ActionListBox)base.AddItem(item);
    }

    /// <summary>
    /// Adds a new item to the list, which is displayed in the list using a supplied label.
    /// </summary>
    /// <param name="label">Label to use in the list for the new item</param>
    /// <param name="action">Action to invoke when this action is selected and then triggered</param>
    /// <returns>Itself</returns>
    public ActionListBox AddItem(string label, Action action)
    {
        return AddItem(new LabeledActionItem(label, action));
    }

    public override TerminalPosition? CursorLocation => null;

    protected override IInteractable.Result HandleKeyStroke(KeyStroke keyStroke)
    {
        if (IsKeyboardActivationStroke(keyStroke))
        {
            RunSelectedItem();
            return IInteractable.Result.Handled;
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
            if (existingIndex != newIndex || !IsFocused || actionType == MouseActionType.ClickDown)
            {
                // the index has changed, or the focus needs to be obtained, or the user is clicking on the current selection to perform the action again
                var result = base.HandleKeyStroke(keyStroke);
                RunSelectedItem();
                return result;
            }
            return IInteractable.Result.Handled;
        }
        else
        {
            var result = base.HandleKeyStroke(keyStroke);
            //RunSelectedItem();
            return result;
        }
    }

    public void RunSelectedItem()
    {
        var selectedItem = GetSelectedItem();
        selectedItem?.Execute();
    }

    /// <summary>
    /// Helper class to wrap an Action with a custom label for display purposes
    /// </summary>
    private class LabeledActionItem : IActionItem
    {
        private readonly string _label;
        private readonly Action _action;

        public LabeledActionItem(string label, Action action)
        {
            _label = label;
            _action = action;
        }

        public void Execute()
        {
            _action?.Invoke();
        }

        public override string ToString()
        {
            return _label;
        }
    }
}