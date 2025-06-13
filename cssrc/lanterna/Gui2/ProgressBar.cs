using Lanterna.Core;
using Lanterna.Graphics;

namespace Lanterna.Gui2;

/// <summary>
/// This GUI element gives a visual indication of how far a process of some sort has progressed at any given time. It's
/// a classic user interface component that most people are familiar with. It works based on a scale expressed as having
/// a minimum, a maximum and a current value somewhere along that range. When the current value is the same as the 
/// minimum, the progress indication is empty, at 0%. If the value is the same as the maximum, the progress indication 
/// is filled, at 100%. Any value in between the minimum and the maximum will be indicated proportionally to where on 
/// this range between minimum and maximum it is.
/// </summary>
public class ProgressBar : AbstractComponent<ProgressBar>
{
    private int _min;
    private int _max;
    private int _value;
    private int _preferredWidth;
    private string? _labelFormat;
    private readonly object _lock = new object();

    /// <summary>
    /// Creates a new progress bar initially defined with a range from 0 to 100.
    /// </summary>
    public ProgressBar() : this(0, 100)
    {
    }

    /// <summary>
    /// Creates a new progress bar with a defined range of minimum to maximum
    /// </summary>
    /// <param name="min">The minimum value of this progress bar</param>
    /// <param name="max">The maximum value of this progress bar</param>
    public ProgressBar(int min, int max) : this(min, max, 0)
    {
    }

    /// <summary>
    /// Creates a new progress bar with a defined range of minimum to maximum and also with a hint as to how wide the
    /// progress bar should be drawn
    /// </summary>
    /// <param name="min">The minimum value of this progress bar</param>
    /// <param name="max">The maximum value of this progress bar</param>
    /// <param name="preferredWidth">Width size hint, in number of columns, for this progress bar. The renderer may choose to
    ///                        not use this hint. 0 or less means that there is no hint.</param>
    public ProgressBar(int min, int max, int preferredWidth)
    {
        if (min > max)
        {
            min = max;
        }
        
        _min = min;
        _max = max;
        _value = min;
        _labelFormat = "%2.0f%%";

        if (preferredWidth < 1)
        {
            preferredWidth = 1;
        }
        _preferredWidth = preferredWidth;
    }

    /// <summary>
    /// Returns the current minimum value for this progress bar
    /// </summary>
    public int Min => _min;

    /// <summary>
    /// Updates the minimum value of this progress bar. If the current maximum and/or value are
    /// smaller than this new minimum, they are automatically adjusted so that the range is still valid.
    /// </summary>
    /// <param name="min">New minimum value to assign to this progress bar</param>
    /// <returns>Itself</returns>
    public ProgressBar SetMin(int min)
    {
        lock (_lock)
        {
            if (min > _max)
            {
                SetMax(min);
            }
            if (min > _value)
            {
                SetValue(min);
            }
            if (_min != min)
            {
                _min = min;
                Invalidate();
            }
            return this;
        }
    }

    /// <summary>
    /// Returns the current maximum value for this progress bar
    /// </summary>
    public int Max => _max;

    /// <summary>
    /// Updates the maximum value of this progress bar. If the current minimum and/or value are
    /// greater than this new maximum, they are automatically adjusted so that the range is still valid.
    /// </summary>
    /// <param name="max">New maximum value to assign to this progress bar</param>
    /// <returns>Itself</returns>
    public ProgressBar SetMax(int max)
    {
        lock (_lock)
        {
            if (max < _min)
            {
                SetMin(max);
            }
            if (max < _value)
            {
                SetValue(max);
            }
            if (_max != max)
            {
                _max = max;
                Invalidate();
            }
            return this;
        }
    }

    /// <summary>
    /// Returns the current value of this progress bar, which represents how complete the progress indication is.
    /// </summary>
    public int Value => _value;

    /// <summary>
    /// Updates the value of this progress bar, which will update the visual state. If the value passed in is
    /// outside the minimum-maximum range, it is automatically adjusted.
    /// </summary>
    /// <param name="value">New value of the progress bar</param>
    /// <returns>Itself</returns>
    public ProgressBar SetValue(int value)
    {
        lock (_lock)
        {
            if (value < _min)
            {
                value = _min;
            }
            if (value > _max)
            {
                value = _max;
            }
            if (_value != value)
            {
                _value = value;
                Invalidate();
            }
            return this;
        }
    }

    /// <summary>
    /// Returns the preferred width of the progress bar component, in number of columns. If 0 or less, it should be
    /// interpreted as no preference on width and it's up to the renderer to decide.
    /// </summary>
    public int PreferredWidth => _preferredWidth;

    /// <summary>
    /// Updated the preferred width hint, which tells the renderer how wide this progress bar would like to be. If called
    /// with 0 (or less), it means no preference on width and the renderer will have to figure out on its own how wide
    /// to make it.
    /// </summary>
    /// <param name="preferredWidth">New preferred width in number of columns, or 0 if no preference</param>
    public void SetPreferredWidth(int preferredWidth)
    {
        _preferredWidth = preferredWidth;
    }

    /// <summary>
    /// Returns the current label format string which is the template for what the progress bar would like to be the
    /// label printed. Exactly how this label is printed depends on the renderer, but the default renderer will print
    /// the label centered in the middle of the progress indication.
    /// </summary>
    public string? LabelFormat => _labelFormat;

    /// <summary>
    /// Sets the label format this progress bar should use when the component is drawn. The string would be compatible
    /// with string formatting, the class will pass the string through that method and pass in the current
    /// progress as a single parameter (passed in as a float in the range of 0.0f to 100.0f). Setting this
    /// format string to null or empty string will turn off the label rendering.
    /// </summary>
    /// <param name="labelFormat">Label format to use when drawing the progress bar, or null to disable</param>
    /// <returns>Itself</returns>
    public ProgressBar SetLabelFormat(string? labelFormat)
    {
        lock (_lock)
        {
            _labelFormat = labelFormat;
            Invalidate();
            return this;
        }
    }

    /// <summary>
    /// Returns the current progress of this progress bar's value from minimum to maximum, expressed
    /// as a float from 0.0f to 1.0f.
    /// </summary>
    public float Progress
    {
        get
        {
            lock (_lock)
            {
                return (float)(_value - _min) / (float)_max;
            }
        }
    }

    /// <summary>
    /// Returns the label of this progress bar formatted with the current progress value.
    /// </summary>
    public string FormattedLabel
    {
        get
        {
            lock (_lock)
            {
                if (_labelFormat == null)
                {
                    return "";
                }
                return string.Format(_labelFormat, Progress * 100.0f);
            }
        }
    }

    protected override IComponentRenderer<ProgressBar> CreateDefaultRenderer()
    {
        return new DefaultProgressBarRenderer();
    }

    /// <summary>
    /// Default implementation of the progress bar GUI component renderer. This renderer will draw the progress bar
    /// on a single line and gradually fill up the space with a different color as the progress is increasing.
    /// </summary>
    public class DefaultProgressBarRenderer : IComponentRenderer<ProgressBar>
    {
        public TerminalSize GetPreferredSize(ProgressBar component)
        {
            int preferredWidth = component.PreferredWidth;
            if (preferredWidth > 0)
            {
                return new TerminalSize(preferredWidth, 1);
            }
            if (component.LabelFormat != null && !string.IsNullOrWhiteSpace(component.LabelFormat))
            {
                return new TerminalSize(TerminalTextUtils.GetColumnWidth(string.Format(component.LabelFormat, 100.0f)) + 2, 1);
            }
            return new TerminalSize(10, 1);
        }

        public void DrawComponent(ITextGUIGraphics graphics, ProgressBar component)
        {
            var size = graphics.Size;
            if (size.Rows == 0 || size.Columns == 0)
            {
                return;
            }
            
            var themeDefinition = component.ThemeDefinition;
            int columnOfProgress = (int)(component.Progress * size.Columns);
            string label = component.FormattedLabel;
            int labelRow = size.Rows / 2;

            // Adjust label so it fits inside the component
            int labelWidth = TerminalTextUtils.GetColumnWidth(label);

            // Can't be too smart about this, because of CJK characters
            if (labelWidth > size.Columns)
            {
                bool tail = true;
                while (labelWidth > size.Columns)
                {
                    if (tail)
                    {
                        label = label.Substring(0, label.Length - 1);
                    }
                    else
                    {
                        label = label.Substring(1);
                    }
                    tail = !tail;
                    labelWidth = TerminalTextUtils.GetColumnWidth(label);
                }
            }
            int labelStartPosition = (size.Columns - labelWidth) / 2;

            for (int row = 0; row < size.Rows; row++)
            {
                graphics.ApplyThemeStyle(themeDefinition.Active);
                for (int column = 0; column < size.Columns; column++)
                {
                    if (column == columnOfProgress)
                    {
                        graphics.ApplyThemeStyle(themeDefinition.Normal);
                    }
                    if (row == labelRow && column >= labelStartPosition && column < labelStartPosition + labelWidth)
                    {
                        char character = label[TerminalTextUtils.GetStringCharacterIndex(label, column - labelStartPosition)];
                        graphics.SetCharacter(column, row, character);
                        if (TerminalTextUtils.IsCharDoubleWidth(character))
                        {
                            column++;
                            if (column == columnOfProgress)
                            {
                                graphics.ApplyThemeStyle(themeDefinition.Normal);
                            }
                        }
                    }
                    else
                    {
                        graphics.SetCharacter(column, row, themeDefinition.GetCharacter("FILLER", ' '));
                    }
                }
            }
        }
    }
}