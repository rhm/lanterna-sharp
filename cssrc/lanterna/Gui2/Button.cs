using Lanterna.Core;
using Lanterna.Graphics;
using Lanterna.Input;
using System.Collections.Concurrent;

namespace Lanterna.Gui2;

public class Button : AbstractInteractableComponent<Button>
{
    public interface IListener
    {
        void OnTriggered(Button button);
    }

    private readonly ConcurrentBag<IListener> _listeners;
    private string _label;

    public Button(string label)
    {
        _listeners = new ConcurrentBag<IListener>();
        Label = label;
    }

    public Button(string label, Action action) : this(label)
    {
        _listeners.Add(new ActionListener(action));
    }

    protected override IComponentRenderer<Button> CreateDefaultRenderer()
    {
        return new DefaultButtonRenderer();
    }

    public override TerminalPosition? CursorLocation => 
        ((IButtonRenderer)Renderer).GetCursorLocation(this);

    protected override IInteractable.Result HandleKeyStroke(KeyStroke keyStroke)
    {
        if (IsActivationStroke(keyStroke))
        {
            TriggerActions();
            return IInteractable.Result.Handled;
        }
        return base.HandleKeyStroke(keyStroke);
    }

    protected void TriggerActions()
    {
        lock (this)
        {
            foreach (var listener in _listeners)
            {
                listener.OnTriggered(this);
            }
        }
    }

    public string Label
    {
        get => _label;
        set
        {
            lock (this)
            {
                if (value == null)
                {
                    throw new ArgumentException("null label to a button is not allowed");
                }
                if (string.IsNullOrEmpty(value))
                {
                    value = " ";
                }
                _label = value;
                Invalidate();
            }
        }
    }

    public void AddListener(IListener listener)
    {
        if (listener == null)
        {
            throw new ArgumentException("null listener to a button is not allowed");
        }
        _listeners.Add(listener);
    }

    public bool RemoveListener(IListener listener)
    {
        // Note: ConcurrentBag doesn't support removal efficiently
        // In production, consider using a different collection
        var newBag = new ConcurrentBag<IListener>();
        bool found = false;
        foreach (var existing in _listeners)
        {
            if (!existing.Equals(listener) || found)
            {
                newBag.Add(existing);
            }
            else
            {
                found = true;
            }
        }
        return found;
    }

    public override string ToString()
    {
        return $"Button{{{_label}}}";
    }

    private class ActionListener : IListener
    {
        private readonly Action _action;

        public ActionListener(Action action)
        {
            _action = action;
        }

        public void OnTriggered(Button button)
        {
            _action();
        }
    }

    public interface IButtonRenderer : IInteractableRenderer<Button>
    {
    }

    public class DefaultButtonRenderer : IButtonRenderer
    {
        public TerminalPosition? GetCursorLocation(Button button)
        {
            if (button.ThemeDefinition.CursorVisible)
            {
                return new TerminalPosition(1 + GetLabelShift(button, button.Size), 0);
            }
            else
            {
                return null;
            }
        }

        public TerminalSize GetPreferredSize(Button button)
        {
            return new TerminalSize(Math.Max(8, TerminalTextUtils.GetColumnWidth(button.Label) + 2), 1);
        }

        public void DrawComponent(ITextGUIGraphics graphics, Button button)
        {
            var themeDefinition = button.ThemeDefinition;
            if (button.IsFocused)
            {
                graphics.ApplyThemeStyle(themeDefinition.Active);
            }
            else
            {
                graphics.ApplyThemeStyle(themeDefinition.Insensitive);
            }
            graphics.Fill(' ');
            graphics.SetCharacter(0, 0, themeDefinition.GetCharacter("LEFT_BORDER", '<'));
            graphics.SetCharacter(graphics.Size.Columns - 1, 0, themeDefinition.GetCharacter("RIGHT_BORDER", '>'));

            if (button.IsFocused)
            {
                graphics.ApplyThemeStyle(themeDefinition.Active);
            }
            else
            {
                graphics.ApplyThemeStyle(themeDefinition.PreLight);
            }
            
            int labelShift = GetLabelShift(button, graphics.Size);
            if (button.Label.Length > 0)
            {
                graphics.SetCharacter(1 + labelShift, 0, button.Label[0]);
            }

            if (TerminalTextUtils.GetColumnWidth(button.Label) == 1)
            {
                return;
            }
            
            if (button.IsFocused)
            {
                graphics.ApplyThemeStyle(themeDefinition.Selected);
            }
            else
            {
                graphics.ApplyThemeStyle(themeDefinition.Normal);
            }
            
            if (button.Label.Length > 1)
            {
                graphics.PutString(1 + labelShift + 1, 0, button.Label.Substring(1));
            }
        }

        private int GetLabelShift(Button button, TerminalSize size)
        {
            int availableSpace = size.Columns - 2;
            if (availableSpace <= 0)
            {
                return 0;
            }
            int labelShift = 0;
            int widthInColumns = TerminalTextUtils.GetColumnWidth(button.Label);
            if (availableSpace > widthInColumns)
            {
                labelShift = (size.Columns - 2 - widthInColumns) / 2;
            }
            return labelShift;
        }
    }

    public class FlatButtonRenderer : IButtonRenderer
    {
        public TerminalPosition? GetCursorLocation(Button component)
        {
            return null;
        }

        public TerminalSize GetPreferredSize(Button component)
        {
            return new TerminalSize(TerminalTextUtils.GetColumnWidth(component.Label), 1);
        }

        public void DrawComponent(ITextGUIGraphics graphics, Button button)
        {
            var themeDefinition = button.ThemeDefinition;
            if (button.IsFocused)
            {
                graphics.ApplyThemeStyle(themeDefinition.Active);
            }
            else
            {
                graphics.ApplyThemeStyle(themeDefinition.Insensitive);
            }
            graphics.Fill(' ');
            if (button.IsFocused)
            {
                graphics.ApplyThemeStyle(themeDefinition.Selected);
            }
            else
            {
                graphics.ApplyThemeStyle(themeDefinition.Normal);
            }
            graphics.PutString(0, 0, button.Label);
        }
    }

    public class BorderedButtonRenderer : IButtonRenderer
    {
        public TerminalPosition? GetCursorLocation(Button component)
        {
            return null;
        }

        public TerminalSize GetPreferredSize(Button component)
        {
            return new TerminalSize(TerminalTextUtils.GetColumnWidth(component.Label) + 5, 4);
        }

        public void DrawComponent(ITextGUIGraphics graphics, Button button)
        {
            var themeDefinition = button.ThemeDefinition;
            graphics.ApplyThemeStyle(themeDefinition.Normal);
            var size = graphics.Size;
            
            // Draw border
            graphics.DrawLine(1, 0, size.Columns - 3, 0, Symbols.SingleLineHorizontal);
            graphics.DrawLine(1, size.Rows - 2, size.Columns - 3, size.Rows - 2, Symbols.SingleLineHorizontal);
            graphics.DrawLine(0, 1, 0, size.Rows - 3, Symbols.SingleLineVertical);
            graphics.DrawLine(size.Columns - 2, 1, size.Columns - 2, size.Rows - 3, Symbols.SingleLineVertical);
            graphics.SetCharacter(0, 0, Symbols.SingleLineTopLeftCorner);
            graphics.SetCharacter(size.Columns - 2, 0, Symbols.SingleLineTopRightCorner);
            graphics.SetCharacter(size.Columns - 2, size.Rows - 2, Symbols.SingleLineBottomRightCorner);
            graphics.SetCharacter(0, size.Rows - 2, Symbols.SingleLineBottomLeftCorner);

            // Fill the inner part of the box
            graphics.DrawLine(1, 1, size.Columns - 3, 1, ' ');

            // Draw the text inside the button
            if (button.IsFocused)
            {
                graphics.ApplyThemeStyle(themeDefinition.Active);
            }
            graphics.PutString(2, 1, TerminalTextUtils.FitString(button.Label, size.Columns - 5));

            // Draw the shadow
            graphics.ApplyThemeStyle(themeDefinition.Insensitive);
            graphics.DrawLine(1, size.Rows - 1, size.Columns - 1, size.Rows - 1, ' ');
            graphics.DrawLine(size.Columns - 1, 1, size.Columns - 1, size.Rows - 2, ' ');
        }
    }
}