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

namespace Lanterna.Gui2;

/// <summary>
/// Default window decoration renderer that is used unless overridden with another decoration renderer. The windows are
/// drawn using a bevel colored line and the window title in the top-left corner, very similar to ordinary titled
/// borders.
/// </summary>
public class DefaultWindowDecorationRenderer : IWindowDecorationRenderer
{
    private const int TitlePositionWithPadding = 4;
    private const int TitlePositionWithoutPadding = 3;
    private static readonly TerminalPosition Offset = new(1, 1);

    public ITextGUIGraphics Draw(IWindowBasedTextGUI textGUI, ITextGUIGraphics graphics, IWindow window)
    {
        string title = window.Title ?? "";
        var drawableArea = graphics.Size;
        var themeDefinition = window.Theme.GetDefinition(typeof(DefaultWindowDecorationRenderer));
        
        char horizontalLine = themeDefinition.GetCharacter("HORIZONTAL_LINE", Symbols.SingleLineHorizontal);
        char verticalLine = themeDefinition.GetCharacter("VERTICAL_LINE", Symbols.SingleLineVertical);
        char bottomLeftCorner = themeDefinition.GetCharacter("BOTTOM_LEFT_CORNER", Symbols.SingleLineBottomLeftCorner);
        char topLeftCorner = themeDefinition.GetCharacter("TOP_LEFT_CORNER", Symbols.SingleLineTopLeftCorner);
        char bottomRightCorner = themeDefinition.GetCharacter("BOTTOM_RIGHT_CORNER", Symbols.SingleLineBottomRightCorner);
        char topRightCorner = themeDefinition.GetCharacter("TOP_RIGHT_CORNER", Symbols.SingleLineTopRightCorner);
        char titleSeparatorLeft = themeDefinition.GetCharacter("TITLE_SEPARATOR_LEFT", Symbols.SingleLineHorizontal);
        char titleSeparatorRight = themeDefinition.GetCharacter("TITLE_SEPARATOR_RIGHT", Symbols.SingleLineHorizontal);
        bool useTitlePadding = themeDefinition.GetBooleanProperty("TITLE_PADDING", false);
        bool centerTitle = themeDefinition.GetBooleanProperty("CENTER_TITLE", false);

        int titleHorizontalPosition = useTitlePadding ? TitlePositionWithPadding : TitlePositionWithoutPadding;
        int titleMaxColumns = drawableArea.Columns - titleHorizontalPosition * 2;
        if (centerTitle)
        {
            titleHorizontalPosition = (drawableArea.Columns / 2) - (GetTextWidth(title) / 2);
            titleHorizontalPosition = Math.Max(titleHorizontalPosition, useTitlePadding ? TitlePositionWithPadding : TitlePositionWithoutPadding);
        }
        string actualTitle = FitTextToWidth(title, titleMaxColumns);
        int titleActualColumns = GetTextWidth(actualTitle);

        // Don't draw highlights on menu popup windows
        if (window.Hints.Contains(IWindow.Hint.MenuPopup))
        {
            graphics.ApplyThemeStyle(themeDefinition.Normal);
        }
        else
        {
            graphics.ApplyThemeStyle(themeDefinition.PreLight);
        }
        
        graphics.DrawLine(new TerminalPosition(0, drawableArea.Rows - 2), new TerminalPosition(0, 1), verticalLine);
        graphics.DrawLine(new TerminalPosition(1, 0), new TerminalPosition(drawableArea.Columns - 2, 0), horizontalLine);
        graphics.SetCharacter(0, 0, topLeftCorner);
        graphics.SetCharacter(0, drawableArea.Rows - 1, bottomLeftCorner);

        if (!string.IsNullOrEmpty(actualTitle) && drawableArea.Columns > 8)
        {
            int separatorOffset = 1;
            if (useTitlePadding)
            {
                graphics.SetCharacter(titleHorizontalPosition - 1, 0, ' ');
                graphics.SetCharacter(titleHorizontalPosition + titleActualColumns, 0, ' ');
                separatorOffset = 2;
            }
            graphics.SetCharacter(titleHorizontalPosition - separatorOffset, 0, titleSeparatorLeft);
            graphics.SetCharacter(titleHorizontalPosition + titleActualColumns + separatorOffset - 1, 0, titleSeparatorRight);
        }

        graphics.ApplyThemeStyle(themeDefinition.Normal);
        graphics.DrawLine(
            new TerminalPosition(drawableArea.Columns - 1, 1),
            new TerminalPosition(drawableArea.Columns - 1, drawableArea.Rows - 2),
            verticalLine);
        graphics.DrawLine(
            new TerminalPosition(1, drawableArea.Rows - 1),
            new TerminalPosition(drawableArea.Columns - 2, drawableArea.Rows - 1),
            horizontalLine);

        graphics.SetCharacter(drawableArea.Columns - 1, 0, topRightCorner);
        graphics.SetCharacter(drawableArea.Columns - 1, drawableArea.Rows - 1, bottomRightCorner);

        if (!string.IsNullOrEmpty(actualTitle))
        {
            if (textGUI.ActiveWindow == window)
            {
                graphics.ApplyThemeStyle(themeDefinition.Active);
            }
            else
            {
                graphics.ApplyThemeStyle(themeDefinition.Insensitive);
            }
            graphics.PutString(titleHorizontalPosition, 0, actualTitle);
        }

        return graphics.NewTextGraphics(
            new TerminalPosition(1, 1),
            drawableArea
                // Make sure we don't make the new graphic's area smaller than 0
                .WithRelativeColumns(-(Math.Min(2, drawableArea.Columns)))
                .WithRelativeRows(-(Math.Min(2, drawableArea.Rows))));
    }

    public TerminalSize GetDecoratedSize(IWindow window, TerminalSize contentAreaSize)
    {
        var themeDefinition = window.Theme.GetDefinition(typeof(DefaultWindowDecorationRenderer));
        bool useTitlePadding = themeDefinition.GetBooleanProperty("TITLE_PADDING", false);

        int titleWidth = GetTextWidth(window.Title ?? "");
        int minPadding = TitlePositionWithoutPadding * 2;
        if (useTitlePadding)
        {
            minPadding = TitlePositionWithPadding * 2;
        }

        return contentAreaSize
            .WithRelativeColumns(2)
            .WithRelativeRows(2)
            .Max(new TerminalSize(titleWidth + minPadding, 1));  // Make sure the title fits!
    }

    public TerminalPosition GetOffset(IWindow window)
    {
        return Offset;
    }

    // Helper methods for text width calculation - simplified version
    private static int GetTextWidth(string text)
    {
        return text?.Length ?? 0;
    }

    private static string FitTextToWidth(string text, int width)
    {
        if (string.IsNullOrEmpty(text) || width <= 0)
            return "";
        
        if (text.Length <= width)
            return text;
            
        return text.Substring(0, Math.Max(0, width - 3)) + "...";
    }
}