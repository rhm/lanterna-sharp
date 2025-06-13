using Lanterna.Core;
using Lanterna.Graphics;

namespace Lanterna.Gui2;

/// <summary>
/// This class containers a couple of border implementation and utility methods for instantiating them. It also contains
/// a utility method for joining border line graphics together with adjacent lines so they blend in together:
/// JoinLinesWithFrame(..).
/// </summary>
public static class Borders
{
    // Different ways to draw the border
    private enum BorderStyle
    {
        Solid,
        Bevel,
        ReverseBevel
    }

    /// <summary>
    /// Creates a Border that is drawn as a solid color single line surrounding the wrapped component
    /// </summary>
    /// <returns>New solid color single line Border</returns>
    public static IBorder SingleLine()
    {
        return SingleLine("");
    }

    /// <summary>
    /// Creates a Border that is drawn as a solid color single line surrounding the wrapped component with a
    /// title string normally drawn at the top-left side
    /// </summary>
    /// <param name="title">The title to draw on the border</param>
    /// <returns>New solid color single line Border with a title</returns>
    public static IBorder SingleLine(string title)
    {
        return new SingleLineBorder(title, BorderStyle.Solid);
    }

    /// <summary>
    /// Creates a Border that is drawn as a bevel color single line surrounding the wrapped component
    /// </summary>
    /// <returns>New bevel color single line Border</returns>
    public static IBorder SingleLineBevel()
    {
        return SingleLineBevel("");
    }

    /// <summary>
    /// Creates a Border that is drawn as a bevel color single line surrounding the wrapped component with a
    /// title string normally drawn at the top-left side
    /// </summary>
    /// <param name="title">The title to draw on the border</param>
    /// <returns>New bevel color single line Border with a title</returns>
    public static IBorder SingleLineBevel(string title)
    {
        return new SingleLineBorder(title, BorderStyle.Bevel);
    }

    /// <summary>
    /// Creates a Border that is drawn as a reverse bevel color single line surrounding the wrapped component
    /// </summary>
    /// <returns>New reverse bevel color single line Border</returns>
    public static IBorder SingleLineReverseBevel()
    {
        return SingleLineReverseBevel("");
    }

    /// <summary>
    /// Creates a Border that is drawn as a reverse bevel color single line surrounding the wrapped component
    /// with a title string normally drawn at the top-left side
    /// </summary>
    /// <param name="title">The title to draw on the border</param>
    /// <returns>New reverse bevel color single line Border with a title</returns>
    public static IBorder SingleLineReverseBevel(string title)
    {
        return new SingleLineBorder(title, BorderStyle.ReverseBevel);
    }

    /// <summary>
    /// Creates a Border that is drawn as a solid color double line surrounding the wrapped component
    /// </summary>
    /// <returns>New solid color double line Border</returns>
    public static IBorder DoubleLine()
    {
        return DoubleLine("");
    }

    /// <summary>
    /// Creates a Border that is drawn as a solid color double line surrounding the wrapped component with a
    /// title string normally drawn at the top-left side
    /// </summary>
    /// <param name="title">The title to draw on the border</param>
    /// <returns>New solid color double line Border with a title</returns>
    public static IBorder DoubleLine(string title)
    {
        return new DoubleLineBorder(title, BorderStyle.Solid);
    }

    /// <summary>
    /// Creates a Border that is drawn as a bevel color double line surrounding the wrapped component
    /// </summary>
    /// <returns>New bevel color double line Border</returns>
    public static IBorder DoubleLineBevel()
    {
        return DoubleLineBevel("");
    }

    /// <summary>
    /// Creates a Border that is drawn as a bevel color double line surrounding the wrapped component with a
    /// title string normally drawn at the top-left side
    /// </summary>
    /// <param name="title">The title to draw on the border</param>
    /// <returns>New bevel color double line Border with a title</returns>
    public static IBorder DoubleLineBevel(string title)
    {
        return new DoubleLineBorder(title, BorderStyle.Bevel);
    }

    /// <summary>
    /// Creates a Border that is drawn as a reverse bevel color double line surrounding the wrapped component
    /// </summary>
    /// <returns>New reverse bevel color double line Border</returns>
    public static IBorder DoubleLineReverseBevel()
    {
        return DoubleLineReverseBevel("");
    }

    /// <summary>
    /// Creates a Border that is drawn as a reverse bevel color double line surrounding the wrapped component
    /// with a title string normally drawn at the top-left side
    /// </summary>
    /// <param name="title">The title to draw on the border</param>
    /// <returns>New reverse bevel color double line Border with a title</returns>
    public static IBorder DoubleLineReverseBevel(string title)
    {
        return new DoubleLineBorder(title, BorderStyle.ReverseBevel);
    }

    private abstract class StandardBorder : AbstractBorder
    {
        private readonly string _title;
        protected readonly BorderStyle BorderStyle;

        protected StandardBorder(string title, BorderStyle borderStyle)
        {
            _title = title ?? throw new ArgumentException("Cannot create a border with null title");
            BorderStyle = borderStyle;
        }

        public string Title => _title;

        public override string ToString()
        {
            return GetType().Name + "{" + _title + "}";
        }
    }

    private abstract class AbstractBorderRenderer : IBorder.IBorderRenderer
    {
        private readonly BorderStyle _borderStyle;

        protected AbstractBorderRenderer(BorderStyle borderStyle)
        {
            _borderStyle = borderStyle;
        }

        public TerminalSize GetPreferredSize(IBorder component)
        {
            var border = (StandardBorder)component;
            var wrappedComponent = border.GetComponent();
            TerminalSize preferredSize;
            if (wrappedComponent == null)
            {
                preferredSize = TerminalSize.Zero;
            }
            else
            {
                preferredSize = wrappedComponent.PreferredSize;
            }
            preferredSize = preferredSize.WithRelativeColumns(2).WithRelativeRows(2);
            string borderTitle = border.Title;
            return preferredSize.Max(new TerminalSize(borderTitle.Length == 0 ? 2 : TerminalTextUtils.GetColumnWidth(borderTitle) + 4, 2));
        }

        public TerminalPosition GetWrappedComponentTopLeftOffset()
        {
            return new TerminalPosition(1, 1);
        }

        public TerminalSize GetWrappedComponentSize(TerminalSize borderSize)
        {
            return borderSize
                .WithRelativeColumns(-Math.Min(2, borderSize.Columns))
                .WithRelativeRows(-Math.Min(2, borderSize.Rows));
        }

        public void DrawComponent(ITextGUIGraphics graphics, IBorder component)
        {
            var border = (StandardBorder)component;
            var wrappedComponent = border.GetComponent();
            if (wrappedComponent == null)
            {
                return;
            }
            var drawableArea = graphics.Size;

            char horizontalLine = GetHorizontalLine(component.Theme);
            char verticalLine = GetVerticalLine(component.Theme);
            char bottomLeftCorner = GetBottomLeftCorner(component.Theme);
            char topLeftCorner = GetTopLeftCorner(component.Theme);
            char bottomRightCorner = GetBottomRightCorner(component.Theme);
            char topRightCorner = GetTopRightCorner(component.Theme);
            char titleLeft = GetTitleLeft(component.Theme);
            char titleRight = GetTitleRight(component.Theme);

            var themeDefinition = component.Theme.GetDefinition(typeof(AbstractBorder));
            if (_borderStyle == BorderStyle.Bevel)
            {
                graphics.ApplyThemeStyle(themeDefinition.PreLight);
            }
            else
            {
                graphics.ApplyThemeStyle(themeDefinition.Normal);
            }
            graphics.SetCharacter(0, drawableArea.Rows - 1, bottomLeftCorner);
            if (drawableArea.Rows > 2)
            {
                graphics.DrawLine(new TerminalPosition(0, drawableArea.Rows - 2), new TerminalPosition(0, 1), verticalLine);
            }
            graphics.SetCharacter(0, 0, topLeftCorner);
            if (drawableArea.Columns > 2)
            {
                graphics.DrawLine(new TerminalPosition(1, 0), new TerminalPosition(drawableArea.Columns - 2, 0), horizontalLine);
            }

            if (_borderStyle == BorderStyle.ReverseBevel)
            {
                graphics.ApplyThemeStyle(themeDefinition.PreLight);
            }
            else
            {
                graphics.ApplyThemeStyle(themeDefinition.Normal);
            }
            graphics.SetCharacter(drawableArea.Columns - 1, 0, topRightCorner);
            if (drawableArea.Rows > 2)
            {
                graphics.DrawLine(new TerminalPosition(drawableArea.Columns - 1, 1),
                    new TerminalPosition(drawableArea.Columns - 1, drawableArea.Rows - 2),
                    verticalLine);
            }
            graphics.SetCharacter(drawableArea.Columns - 1, drawableArea.Rows - 1, bottomRightCorner);
            if (drawableArea.Columns > 2)
            {
                graphics.DrawLine(new TerminalPosition(1, drawableArea.Rows - 1),
                    new TerminalPosition(drawableArea.Columns - 2, drawableArea.Rows - 1),
                    horizontalLine);
            }

            if (!string.IsNullOrEmpty(border.Title) &&
                drawableArea.Columns >= TerminalTextUtils.GetColumnWidth(border.Title) + 4)
            {
                graphics.ApplyThemeStyle(themeDefinition.Active);
                graphics.PutString(2, 0, border.Title);

                if (_borderStyle == BorderStyle.Bevel)
                {
                    graphics.ApplyThemeStyle(themeDefinition.PreLight);
                }
                else
                {
                    graphics.ApplyThemeStyle(themeDefinition.Normal);
                }
                graphics.SetCharacter(1, 0, titleLeft);
                graphics.SetCharacter(2 + TerminalTextUtils.GetColumnWidth(border.Title), 0, titleRight);
            }

            wrappedComponent.Draw(graphics.NewTextGraphics(GetWrappedComponentTopLeftOffset(), GetWrappedComponentSize(drawableArea)));
            JoinLinesWithFrame(graphics);
        }

        protected abstract char GetHorizontalLine(Theme theme);
        protected abstract char GetVerticalLine(Theme theme);
        protected abstract char GetBottomLeftCorner(Theme theme);
        protected abstract char GetTopLeftCorner(Theme theme);
        protected abstract char GetBottomRightCorner(Theme theme);
        protected abstract char GetTopRightCorner(Theme theme);
        protected abstract char GetTitleLeft(Theme theme);
        protected abstract char GetTitleRight(Theme theme);
    }

    /// <summary>
    /// This method will attempt to join line drawing characters with the outermost bottom and top rows and left and
    /// right columns. For example, if a vertical left border character is ║ and the character immediately to the right
    /// of it is ─, then the border character will be updated to ╟ to join the two together. Please note that this method
    /// will only join the outer border columns and rows.
    /// </summary>
    /// <param name="graphics">Graphics to use when inspecting and joining characters</param>
    public static void JoinLinesWithFrame(ITextGraphics graphics)
    {
        var drawableArea = graphics.Size;
        if (drawableArea.Rows <= 2 || drawableArea.Columns <= 2)
        {
            // Too small
            return;
        }

        int upperRow = 0;
        int lowerRow = drawableArea.Rows - 1;
        int leftRow = 0;
        int rightRow = drawableArea.Columns - 1;

        var junctionFromBelowSingle = new HashSet<char>
        {
            Symbols.SingleLineVertical,
            Symbols.BoldFromNormalSingleLineVertical,
            Symbols.BoldSingleLineVertical,
            Symbols.SingleLineCross,
            Symbols.DoubleLineHorizontalSingleLineCross,
            Symbols.SingleLineBottomLeftCorner,
            Symbols.SingleLineBottomRightCorner,
            Symbols.SingleLineTLeft,
            Symbols.SingleLineTRight,
            Symbols.SingleLineTUp,
            Symbols.SingleLineTDoubleLeft,
            Symbols.SingleLineTDoubleRight,
            Symbols.DoubleLineTSingleUp
        };
        var junctionFromBelowDouble = new HashSet<char>
        {
            Symbols.DoubleLineVertical,
            Symbols.DoubleLineCross,
            Symbols.DoubleLineVerticalSingleLineCross,
            Symbols.DoubleLineBottomLeftCorner,
            Symbols.DoubleLineBottomRightCorner,
            Symbols.DoubleLineTLeft,
            Symbols.DoubleLineTRight,
            Symbols.DoubleLineTUp,
            Symbols.DoubleLineTSingleLeft,
            Symbols.DoubleLineTSingleRight,
            Symbols.SingleLineTDoubleUp
        };
        var junctionFromAboveSingle = new HashSet<char>
        {
            Symbols.SingleLineVertical,
            Symbols.BoldToNormalSingleLineVertical,
            Symbols.BoldSingleLineVertical,
            Symbols.SingleLineCross,
            Symbols.DoubleLineHorizontalSingleLineCross,
            Symbols.SingleLineTopLeftCorner,
            Symbols.SingleLineTopRightCorner,
            Symbols.SingleLineTLeft,
            Symbols.SingleLineTRight,
            Symbols.SingleLineTDown,
            Symbols.SingleLineTDoubleLeft,
            Symbols.SingleLineTDoubleRight,
            Symbols.DoubleLineTSingleDown
        };
        var junctionFromAboveDouble = new HashSet<char>
        {
            Symbols.DoubleLineVertical,
            Symbols.DoubleLineCross,
            Symbols.DoubleLineVerticalSingleLineCross,
            Symbols.DoubleLineTopLeftCorner,
            Symbols.DoubleLineTopRightCorner,
            Symbols.DoubleLineTLeft,
            Symbols.DoubleLineTRight,
            Symbols.DoubleLineTDown,
            Symbols.DoubleLineTSingleLeft,
            Symbols.DoubleLineTSingleRight,
            Symbols.SingleLineTDoubleDown
        };
        var junctionFromLeftSingle = new HashSet<char>
        {
            Symbols.SingleLineHorizontal,
            Symbols.BoldToNormalSingleLineHorizontal,
            Symbols.BoldSingleLineHorizontal,
            Symbols.SingleLineCross,
            Symbols.DoubleLineVerticalSingleLineCross,
            Symbols.SingleLineBottomLeftCorner,
            Symbols.SingleLineTopLeftCorner,
            Symbols.SingleLineTUp,
            Symbols.SingleLineTDown,
            Symbols.SingleLineTRight,
            Symbols.SingleLineTDoubleUp,
            Symbols.SingleLineTDoubleDown,
            Symbols.DoubleLineTSingleRight
        };
        var junctionFromLeftDouble = new HashSet<char>
        {
            Symbols.DoubleLineHorizontal,
            Symbols.DoubleLineCross,
            Symbols.DoubleLineHorizontalSingleLineCross,
            Symbols.DoubleLineBottomLeftCorner,
            Symbols.DoubleLineTopLeftCorner,
            Symbols.DoubleLineTUp,
            Symbols.DoubleLineTDown,
            Symbols.DoubleLineTRight,
            Symbols.DoubleLineTSingleUp,
            Symbols.DoubleLineTSingleDown,
            Symbols.SingleLineTDoubleRight
        };
        var junctionFromRightSingle = new HashSet<char>
        {
            Symbols.SingleLineHorizontal,
            Symbols.BoldFromNormalSingleLineHorizontal,
            Symbols.BoldSingleLineHorizontal,
            Symbols.SingleLineCross,
            Symbols.DoubleLineVerticalSingleLineCross,
            Symbols.SingleLineBottomRightCorner,
            Symbols.SingleLineTopRightCorner,
            Symbols.SingleLineTUp,
            Symbols.SingleLineTDown,
            Symbols.SingleLineTLeft,
            Symbols.SingleLineTDoubleUp,
            Symbols.SingleLineTDoubleDown,
            Symbols.DoubleLineTSingleLeft
        };
        var junctionFromRightDouble = new HashSet<char>
        {
            Symbols.DoubleLineHorizontal,
            Symbols.DoubleLineCross,
            Symbols.DoubleLineHorizontalSingleLineCross,
            Symbols.DoubleLineBottomRightCorner,
            Symbols.DoubleLineTopRightCorner,
            Symbols.DoubleLineTUp,
            Symbols.DoubleLineTDown,
            Symbols.DoubleLineTLeft,
            Symbols.DoubleLineTSingleUp,
            Symbols.DoubleLineTSingleDown,
            Symbols.SingleLineTDoubleLeft
        };

        // Go horizontally and check vertical neighbours if it's possible to extend lines into the border
        for (int column = 1; column < drawableArea.Columns - 1; column++)
        {
            // Check first row
            var borderCharacter = graphics.GetCharacter(column, upperRow);
            if (borderCharacter == null)
            {
                continue;
            }
            var neighbourCharacter = graphics.GetCharacter(column, upperRow + 1);
            if (neighbourCharacter != null)
            {
                char neighbour = neighbourCharacter.CharacterString[0];
                if (borderCharacter.Is(Symbols.SingleLineHorizontal))
                {
                    if (junctionFromBelowSingle.Contains(neighbour))
                    {
                        graphics.SetCharacter(column, upperRow, borderCharacter.WithCharacter(Symbols.SingleLineTDown));
                    }
                    else if (junctionFromBelowDouble.Contains(neighbour))
                    {
                        graphics.SetCharacter(column, upperRow, borderCharacter.WithCharacter(Symbols.SingleLineTDoubleDown));
                    }
                }
                else if (borderCharacter.Is(Symbols.DoubleLineHorizontal))
                {
                    if (junctionFromBelowSingle.Contains(neighbour))
                    {
                        graphics.SetCharacter(column, upperRow, borderCharacter.WithCharacter(Symbols.DoubleLineTSingleDown));
                    }
                    else if (junctionFromBelowDouble.Contains(neighbour))
                    {
                        graphics.SetCharacter(column, upperRow, borderCharacter.WithCharacter(Symbols.DoubleLineTDown));
                    }
                }
            }

            // Check last row
            borderCharacter = graphics.GetCharacter(column, lowerRow);
            if (borderCharacter == null)
            {
                continue;
            }
            neighbourCharacter = graphics.GetCharacter(column, lowerRow - 1);
            if (neighbourCharacter != null)
            {
                char neighbour = neighbourCharacter.CharacterString[0];
                if (borderCharacter.Is(Symbols.SingleLineHorizontal))
                {
                    if (junctionFromAboveSingle.Contains(neighbour))
                    {
                        graphics.SetCharacter(column, lowerRow, borderCharacter.WithCharacter(Symbols.SingleLineTUp));
                    }
                    else if (junctionFromAboveDouble.Contains(neighbour))
                    {
                        graphics.SetCharacter(column, lowerRow, borderCharacter.WithCharacter(Symbols.SingleLineTDoubleUp));
                    }
                }
                else if (borderCharacter.Is(Symbols.DoubleLineHorizontal))
                {
                    if (junctionFromAboveSingle.Contains(neighbour))
                    {
                        graphics.SetCharacter(column, lowerRow, borderCharacter.WithCharacter(Symbols.DoubleLineTSingleUp));
                    }
                    else if (junctionFromAboveDouble.Contains(neighbour))
                    {
                        graphics.SetCharacter(column, lowerRow, borderCharacter.WithCharacter(Symbols.DoubleLineTUp));
                    }
                }
            }
        }

        // Go vertically and check horizontal neighbours if it's possible to extend lines into the border
        for (int row = 1; row < drawableArea.Rows - 1; row++)
        {
            // Check first column
            var borderCharacter = graphics.GetCharacter(leftRow, row);
            if (borderCharacter == null)
            {
                continue;
            }
            var neighbourCharacter = graphics.GetCharacter(leftRow + 1, row);
            if (neighbourCharacter != null)
            {
                char neighbour = neighbourCharacter.CharacterString[0];
                if (borderCharacter.Is(Symbols.SingleLineVertical))
                {
                    if (junctionFromRightSingle.Contains(neighbour))
                    {
                        graphics.SetCharacter(leftRow, row, borderCharacter.WithCharacter(Symbols.SingleLineTRight));
                    }
                    else if (junctionFromRightDouble.Contains(neighbour))
                    {
                        graphics.SetCharacter(leftRow, row, borderCharacter.WithCharacter(Symbols.SingleLineTDoubleRight));
                    }
                }
                else if (borderCharacter.Is(Symbols.DoubleLineVertical))
                {
                    if (junctionFromRightSingle.Contains(neighbour))
                    {
                        graphics.SetCharacter(leftRow, row, borderCharacter.WithCharacter(Symbols.DoubleLineTSingleRight));
                    }
                    else if (junctionFromRightDouble.Contains(neighbour))
                    {
                        graphics.SetCharacter(leftRow, row, borderCharacter.WithCharacter(Symbols.DoubleLineTRight));
                    }
                }
            }

            // Check last column
            borderCharacter = graphics.GetCharacter(rightRow, row);
            if (borderCharacter == null)
            {
                continue;
            }
            neighbourCharacter = graphics.GetCharacter(rightRow - 1, row);
            if (neighbourCharacter != null)
            {
                char neighbour = neighbourCharacter.CharacterString[0];
                if (borderCharacter.Is(Symbols.SingleLineVertical))
                {
                    if (junctionFromLeftSingle.Contains(neighbour))
                    {
                        graphics.SetCharacter(rightRow, row, borderCharacter.WithCharacter(Symbols.SingleLineTLeft));
                    }
                    else if (junctionFromLeftDouble.Contains(neighbour))
                    {
                        graphics.SetCharacter(rightRow, row, borderCharacter.WithCharacter(Symbols.SingleLineTDoubleLeft));
                    }
                }
                else if (borderCharacter.Is(Symbols.DoubleLineVertical))
                {
                    if (junctionFromLeftSingle.Contains(neighbour))
                    {
                        graphics.SetCharacter(rightRow, row, borderCharacter.WithCharacter(Symbols.DoubleLineTSingleLeft));
                    }
                    else if (junctionFromLeftDouble.Contains(neighbour))
                    {
                        graphics.SetCharacter(rightRow, row, borderCharacter.WithCharacter(Symbols.DoubleLineTLeft));
                    }
                }
            }
        }
    }

    private class SingleLineBorder : StandardBorder
    {
        public SingleLineBorder(string title, BorderStyle borderStyle) : base(title, borderStyle)
        {
        }

        protected override IComponentRenderer<IBorder> CreateDefaultRenderer()
        {
            return new SingleLineRenderer(BorderStyle);
        }
    }

    private class SingleLineRenderer : AbstractBorderRenderer
    {
        public SingleLineRenderer(BorderStyle borderStyle) : base(borderStyle)
        {
        }

        protected override char GetTopRightCorner(Theme theme)
        {
            return theme.GetDefinition(typeof(SingleLineBorder)).GetCharacter("TOP_RIGHT_CORNER", Symbols.SingleLineTopRightCorner);
        }

        protected override char GetBottomRightCorner(Theme theme)
        {
            return theme.GetDefinition(typeof(SingleLineBorder)).GetCharacter("BOTTOM_RIGHT_CORNER", Symbols.SingleLineBottomRightCorner);
        }

        protected override char GetTopLeftCorner(Theme theme)
        {
            return theme.GetDefinition(typeof(SingleLineBorder)).GetCharacter("TOP_LEFT_CORNER", Symbols.SingleLineTopLeftCorner);
        }

        protected override char GetBottomLeftCorner(Theme theme)
        {
            return theme.GetDefinition(typeof(SingleLineBorder)).GetCharacter("BOTTOM_LEFT_CORNER", Symbols.SingleLineBottomLeftCorner);
        }

        protected override char GetVerticalLine(Theme theme)
        {
            return theme.GetDefinition(typeof(SingleLineBorder)).GetCharacter("VERTICAL_LINE", Symbols.SingleLineVertical);
        }

        protected override char GetHorizontalLine(Theme theme)
        {
            return theme.GetDefinition(typeof(SingleLineBorder)).GetCharacter("HORIZONTAL_LINE", Symbols.SingleLineHorizontal);
        }

        protected override char GetTitleLeft(Theme theme)
        {
            return theme.GetDefinition(typeof(SingleLineBorder)).GetCharacter("TITLE_LEFT", Symbols.SingleLineHorizontal);
        }

        protected override char GetTitleRight(Theme theme)
        {
            return theme.GetDefinition(typeof(SingleLineBorder)).GetCharacter("TITLE_RIGHT", Symbols.SingleLineHorizontal);
        }
    }

    private class DoubleLineBorder : StandardBorder
    {
        public DoubleLineBorder(string title, BorderStyle borderStyle) : base(title, borderStyle)
        {
        }

        protected override IComponentRenderer<IBorder> CreateDefaultRenderer()
        {
            return new DoubleLineRenderer(BorderStyle);
        }
    }

    private class DoubleLineRenderer : AbstractBorderRenderer
    {
        public DoubleLineRenderer(BorderStyle borderStyle) : base(borderStyle)
        {
        }

        protected override char GetTopRightCorner(Theme theme)
        {
            return theme.GetDefinition(typeof(DoubleLineBorder)).GetCharacter("TOP_RIGHT_CORNER", Symbols.DoubleLineTopRightCorner);
        }

        protected override char GetBottomRightCorner(Theme theme)
        {
            return theme.GetDefinition(typeof(DoubleLineBorder)).GetCharacter("BOTTOM_RIGHT_CORNER", Symbols.DoubleLineBottomRightCorner);
        }

        protected override char GetTopLeftCorner(Theme theme)
        {
            return theme.GetDefinition(typeof(DoubleLineBorder)).GetCharacter("TOP_LEFT_CORNER", Symbols.DoubleLineTopLeftCorner);
        }

        protected override char GetBottomLeftCorner(Theme theme)
        {
            return theme.GetDefinition(typeof(DoubleLineBorder)).GetCharacter("BOTTOM_LEFT_CORNER", Symbols.DoubleLineBottomLeftCorner);
        }

        protected override char GetVerticalLine(Theme theme)
        {
            return theme.GetDefinition(typeof(DoubleLineBorder)).GetCharacter("VERTICAL_LINE", Symbols.DoubleLineVertical);
        }

        protected override char GetHorizontalLine(Theme theme)
        {
            return theme.GetDefinition(typeof(DoubleLineBorder)).GetCharacter("HORIZONTAL_LINE", Symbols.DoubleLineHorizontal);
        }

        protected override char GetTitleLeft(Theme theme)
        {
            return theme.GetDefinition(typeof(DoubleLineBorder)).GetCharacter("TITLE_LEFT", Symbols.DoubleLineHorizontal);
        }

        protected override char GetTitleRight(Theme theme)
        {
            return theme.GetDefinition(typeof(DoubleLineBorder)).GetCharacter("TITLE_RIGHT", Symbols.DoubleLineHorizontal);
        }
    }
}