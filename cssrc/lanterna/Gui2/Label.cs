using Lanterna.Core;
using Lanterna.Graphics;

namespace Lanterna.Gui2;

public class Label : AbstractComponent<Label>
{
    private string[] _lines;
    private int? _labelWidth;
    private TerminalSize _labelSize;
    private ITextColor? _foregroundColor;
    private ITextColor? _backgroundColor;
    private readonly HashSet<SGR> _additionalStyles;

    public Label(string text)
    {
        _lines = Array.Empty<string>();
        _labelSize = TerminalSize.Zero;
        _labelWidth = 0;
        _foregroundColor = null;
        _backgroundColor = null;
        _additionalStyles = new HashSet<SGR>();
        Text = text;
    }

    protected void SetLines(string[] lines)
    {
        _lines = lines;
    }

    public string Text
    {
        get
        {
            lock (this)
            {
                if (_lines.Length == 0)
                {
                    return "";
                }
                
                if (_lines.Length == 1)
                {
                    return _lines[0];
                }
                
                return string.Join("\n", _lines);
            }
        }
        set
        {
            lock (this)
            {
                SetLines(SplitIntoMultipleLines(value));
                _labelSize = GetBounds(_lines, _labelSize);
                Invalidate();
            }
        }
    }

    protected virtual string[] SplitIntoMultipleLines(string text)
    {
        return text.Replace("\r", "").Split('\n');
    }

    protected virtual TerminalSize GetBounds(string[] lines, TerminalSize currentBounds)
    {
        currentBounds = currentBounds.WithRows(lines.Length);
        
        if (_labelWidth == null || _labelWidth == 0)
        {
            int preferredWidth = 0;
            foreach (string line in lines)
            {
                int lineWidth = TerminalTextUtils.GetColumnWidth(line);
                if (preferredWidth < lineWidth)
                {
                    preferredWidth = lineWidth;
                }
            }
            currentBounds = currentBounds.WithColumns(preferredWidth);
        }
        else
        {
            var wordWrapped = TerminalTextUtils.GetWordWrappedText(_labelWidth.Value, string.Join(" ", lines));
            currentBounds = currentBounds.WithColumns(_labelWidth.Value).WithRows(wordWrapped.Length);
        }
        return currentBounds;
    }

    public Label SetForegroundColor(ITextColor? foregroundColor)
    {
        lock (this)
        {
            _foregroundColor = foregroundColor;
            return this;
        }
    }

    public ITextColor? ForegroundColor => _foregroundColor;

    public Label SetBackgroundColor(ITextColor? backgroundColor)
    {
        lock (this)
        {
            _backgroundColor = backgroundColor;
            return this;
        }
    }

    public ITextColor? BackgroundColor => _backgroundColor;

    public Label AddStyle(SGR sgr)
    {
        lock (this)
        {
            _additionalStyles.Add(sgr);
            return this;
        }
    }

    public Label RemoveStyle(SGR sgr)
    {
        lock (this)
        {
            _additionalStyles.Remove(sgr);
            return this;
        }
    }

    public Label SetLabelWidth(int? labelWidth)
    {
        lock (this)
        {
            _labelWidth = labelWidth;
            return this;
        }
    }

    public int? LabelWidth => _labelWidth;

    protected override IComponentRenderer<Label> CreateDefaultRenderer()
    {
        return new LabelRenderer(this);
    }

    private class LabelRenderer : IComponentRenderer<Label>
    {
        private readonly Label _label;

        public LabelRenderer(Label label)
        {
            _label = label;
        }

        public TerminalSize GetPreferredSize(Label component)
        {
            return component._labelSize;
        }

        public void DrawComponent(ITextGUIGraphics graphics, Label component)
        {
            var themeDefinition = component.ThemeDefinition;
            graphics.ApplyThemeStyle(themeDefinition.Normal);
            
            if (component._foregroundColor != null)
            {
                graphics.SetForegroundColor(component._foregroundColor);
            }
            
            if (component._backgroundColor != null)
            {
                graphics.SetBackgroundColor(component._backgroundColor);
            }
            
            foreach (var sgr in component._additionalStyles)
            {
                graphics.EnableModifiers(sgr);
            }

            string[] linesToDraw;
            if (component.LabelWidth == null)
            {
                linesToDraw = component._lines;
            }
            else
            {
                linesToDraw = TerminalTextUtils.GetWordWrappedText(graphics.Size.Columns, string.Join(" ", component._lines));
            }

            for (int row = 0; row < Math.Min(graphics.Size.Rows, linesToDraw.Length); row++)
            {
                string line = linesToDraw[row];
                if (graphics.Size.Columns >= component._labelSize.Columns)
                {
                    graphics.PutString(0, row, line);
                }
                else
                {
                    int availableColumns = graphics.Size.Columns;
                    string fitString = TerminalTextUtils.FitString(line, availableColumns);
                    graphics.PutString(0, row, fitString);
                }
            }
        }
    }
}