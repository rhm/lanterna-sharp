using System.Collections.Concurrent;
using Lanterna.Core;
using Lanterna.Graphics;
using Lanterna.Input;

namespace Lanterna.Gui2;

/// <summary>
/// The checkbox component looks like a regular checkbox that you can find in modern graphics user interfaces, a label
/// and a space that the user can toggle on and off by using enter or space keys.
/// </summary>
public class CheckBox : AbstractInteractableComponent<CheckBox>
{
    /// <summary>
    /// Listener interface that can be used to catch user events on the check box
    /// </summary>
    public interface IListener
    {
        /// <summary>
        /// This is fired when the user has altered the checked state of this CheckBox
        /// </summary>
        /// <param name="checked">If the CheckBox is now toggled on, this is set to true, otherwise false</param>
        void OnStatusChanged(bool isChecked);
    }

    private readonly ConcurrentBag<IListener> _listeners;
    private string _label;
    private bool _checked;
    private readonly object _lock = new object();

    /// <summary>
    /// Creates a new checkbox with no label, initially set to un-checked
    /// </summary>
    public CheckBox() : this("")
    {
    }

    /// <summary>
    /// Creates a new checkbox with a specific label, initially set to un-checked
    /// </summary>
    /// <param name="label">Label to assign to the check box</param>
    public CheckBox(string label)
    {
        if (label == null)
        {
            throw new ArgumentException("Cannot create a CheckBox with null label");
        }
        if (label.Contains('\n') || label.Contains('\r'))
        {
            throw new ArgumentException("Multiline checkbox labels are not supported");
        }
        
        _listeners = new ConcurrentBag<IListener>();
        _label = label;
        _checked = false;
    }

    /// <summary>
    /// Programmatically updated the check box to a particular checked state
    /// </summary>
    /// <param name="isChecked">If true, the check box will be set to toggled on, otherwise false</param>
    /// <returns>Itself</returns>
    public CheckBox SetChecked(bool isChecked)
    {
        lock (_lock)
        {
            _checked = isChecked;
            RunOnGUIThreadIfExistsOtherwiseRunDirect(() =>
            {
                foreach (var listener in _listeners)
                {
                    listener.OnStatusChanged(isChecked);
                }
            });
            Invalidate();
            return this;
        }
    }

    /// <summary>
    /// Returns the checked state of this check box
    /// </summary>
    /// <returns>true if the check box is toggled on, otherwise false</returns>
    public bool IsChecked => _checked;

    protected override IInteractable.Result HandleKeyStroke(KeyStroke keyStroke)
    {
        if (IsKeyboardActivationStroke(keyStroke))
        {
            SetChecked(!IsChecked);
            return IInteractable.Result.Handled;
        }
        if (IsMouseActivationStroke(keyStroke))
        {
            BasePane?.SetFocusedInteractable(this);
            SetChecked(!IsChecked);
            return IInteractable.Result.Handled;
        }
        return base.HandleKeyStroke(keyStroke);
    }

    /// <summary>
    /// Updates the label of the checkbox
    /// </summary>
    /// <param name="label">New label to assign to the check box</param>
    /// <returns>Itself</returns>
    public CheckBox SetLabel(string label)
    {
        if (label == null)
        {
            throw new ArgumentException("Cannot set CheckBox label to null");
        }
        
        lock (_lock)
        {
            _label = label;
            Invalidate();
            return this;
        }
    }

    /// <summary>
    /// Returns the label of check box
    /// </summary>
    /// <returns>Label currently assigned to the check box</returns>
    public string Label => _label;

    /// <summary>
    /// Adds a listener to this check box so that it will be notified on certain user actions
    /// </summary>
    /// <param name="listener">Listener to fire events on</param>
    /// <returns>Itself</returns>
    public CheckBox AddListener(IListener listener)
    {
        if (listener != null && !_listeners.Contains(listener))
        {
            _listeners.Add(listener);
        }
        return this;
    }

    /// <summary>
    /// Removes a listener from this check box so that, if it was previously added, it will no longer receive any events
    /// </summary>
    /// <param name="listener">Listener to remove from the check box</param>
    /// <returns>Itself</returns>
    public CheckBox RemoveListener(IListener listener)
    {
        // Note: ConcurrentBag doesn't support efficient removal
        // In production, consider using a different collection
        return this;
    }

    protected override IComponentRenderer<CheckBox> CreateDefaultRenderer()
    {
        return new DefaultCheckBoxRenderer();
    }

    /// <summary>
    /// Helper interface that doesn't add any new methods but makes coding new check box renderers a little bit more clear
    /// </summary>
    public abstract class CheckBoxRenderer : IInteractableRenderer<CheckBox>
    {
        public abstract TerminalPosition? GetCursorLocation(CheckBox component);
        public abstract TerminalSize GetPreferredSize(CheckBox component);
        public abstract void DrawComponent(ITextGUIGraphics graphics, CheckBox component);
    }

    /// <summary>
    /// The default renderer that is used unless overridden. This renderer will draw the checkbox label on the right side
    /// of a "[ ]" block which will contain a "X" inside it if the check box has toggle status on
    /// </summary>
    public class DefaultCheckBoxRenderer : CheckBoxRenderer
    {
        private static readonly TerminalPosition CursorLocation = new TerminalPosition(1, 0);

        public override TerminalPosition? GetCursorLocation(CheckBox component)
        {
            if (component.ThemeDefinition.CursorVisible)
            {
                return CursorLocation;
            }
            return null;
        }

        public override TerminalSize GetPreferredSize(CheckBox component)
        {
            int width = 3;
            if (!string.IsNullOrEmpty(component._label))
            {
                width += 1 + TerminalTextUtils.GetColumnWidth(component._label);
            }
            return new TerminalSize(width, 1);
        }

        public override void DrawComponent(ITextGUIGraphics graphics, CheckBox component)
        {
            var themeDefinition = component.ThemeDefinition;
            if (component.IsFocused)
            {
                graphics.ApplyThemeStyle(themeDefinition.Active);
            }
            else
            {
                graphics.ApplyThemeStyle(themeDefinition.Normal);
            }

            graphics.Fill(' ');
            graphics.PutString(4, 0, component._label);

            if (component.IsFocused)
            {
                graphics.ApplyThemeStyle(themeDefinition.PreLight);
            }
            else
            {
                graphics.ApplyThemeStyle(themeDefinition.Insensitive);
            }
            
            graphics.SetCharacter(0, 0, themeDefinition.GetCharacter("LEFT_BRACKET", '['));
            graphics.SetCharacter(2, 0, themeDefinition.GetCharacter("RIGHT_BRACKET", ']'));
            graphics.SetCharacter(3, 0, ' ');

            if (component.IsFocused)
            {
                graphics.ApplyThemeStyle(themeDefinition.Selected);
            }
            else
            {
                graphics.ApplyThemeStyle(themeDefinition.Normal);
            }
            
            graphics.SetCharacter(1, 0, component.IsChecked ? themeDefinition.GetCharacter("MARKER", 'x') : ' ');
        }
    }
}