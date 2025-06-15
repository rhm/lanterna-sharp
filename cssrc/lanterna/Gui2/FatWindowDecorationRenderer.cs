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
using Lanterna.Graphics;

namespace Lanterna.Gui2;

/// <summary>
/// This renderer implements "fat" window decorations that draws thick borders with title bar
/// </summary>
public class FatWindowDecorationRenderer : IWindowDecorationRenderer
{
    private static readonly TerminalPosition OFFSET_WITH_TITLE = new TerminalPosition(1, 3);
    private static readonly TerminalPosition OFFSET_WITHOUT_TITLE = new TerminalPosition(1, 1);

    public ITextGUIGraphics Draw(IWindowBasedTextGUI textGUI, ITextGUIGraphics graphics, IWindow window)
    {
        var themeDefinition = textGUI.Theme.GetDefinition(typeof(IWindow));
        var size = graphics.Size;
        var titleBarHeight = HasTitle(window) ? 3 : 1;
        
        // Draw the main border
        if (HasTitle(window))
        {
            // Draw title bar area
            graphics.ApplyThemeStyle(themeDefinition.PreLight);
            graphics.Fill(' ');
            
            // Draw the border around the entire window
            graphics.ApplyThemeStyle(themeDefinition.Normal);
            DrawBorderWithTitle(graphics, size, window.Title, themeDefinition);
        }
        else
        {
            graphics.ApplyThemeStyle(themeDefinition.Normal);
            DrawBorderWithoutTitle(graphics, size, themeDefinition);
        }

        // Create a sub-graphics region for the content area
        var contentAreaTopLeft = GetOffset(window);
        var contentAreaSize = new TerminalSize(
            size.Columns - 2, // Remove left and right border
            size.Rows - titleBarHeight - 1); // Remove top and bottom borders

        return graphics.NewTextGraphics(contentAreaTopLeft, contentAreaSize);
    }

    public TerminalSize GetDecoratedSize(IWindow window, TerminalSize contentAreaSize)
    {
        if (HasTitle(window))
        {
            // Title bar takes 3 rows (top border, title, separator) + bottom border
            return new TerminalSize(contentAreaSize.Columns + 2, contentAreaSize.Rows + 4);
        }
        else
        {
            // Just top and bottom borders
            return new TerminalSize(contentAreaSize.Columns + 2, contentAreaSize.Rows + 2);
        }
    }

    public TerminalPosition GetOffset(IWindow window)
    {
        return HasTitle(window) ? OFFSET_WITH_TITLE : OFFSET_WITHOUT_TITLE;
    }

    private bool HasTitle(IWindow window)
    {
        return !string.IsNullOrEmpty(window.Title);
    }

    private void DrawBorderWithTitle(ITextGUIGraphics graphics, TerminalSize size, string title, IThemeDefinition themeDefinition)
    {
        // Get box-drawing characters from theme, with fallbacks to Symbols
        var horizontalLine = themeDefinition.GetCharacter("HORIZONTAL", Symbols.SINGLE_LINE_HORIZONTAL);
        var verticalLine = themeDefinition.GetCharacter("VERTICAL", Symbols.SINGLE_LINE_VERTICAL);
        var topLeftCorner = themeDefinition.GetCharacter("TOP_LEFT_CORNER", Symbols.SINGLE_LINE_TOP_LEFT_CORNER);
        var topRightCorner = themeDefinition.GetCharacter("TOP_RIGHT_CORNER", Symbols.SINGLE_LINE_TOP_RIGHT_CORNER);
        var bottomLeftCorner = themeDefinition.GetCharacter("BOTTOM_LEFT_CORNER", Symbols.SINGLE_LINE_BOTTOM_LEFT_CORNER);
        var bottomRightCorner = themeDefinition.GetCharacter("BOTTOM_RIGHT_CORNER", Symbols.SINGLE_LINE_BOTTOM_RIGHT_CORNER);
        var tRight = themeDefinition.GetCharacter("T_RIGHT", Symbols.SINGLE_LINE_T_RIGHT);
        var tLeft = themeDefinition.GetCharacter("T_LEFT", Symbols.SINGLE_LINE_T_LEFT);

        graphics.ApplyThemeStyle(themeDefinition.Normal);

        // Draw top border with corners
        graphics.SetCharacter(0, 0, topLeftCorner);
        graphics.DrawLine(new TerminalPosition(1, 0), new TerminalPosition(size.Columns - 2, 0), horizontalLine);
        graphics.SetCharacter(size.Columns - 1, 0, topRightCorner);

        // Draw title on the second row
        if (!string.IsNullOrEmpty(title))
        {
            graphics.ApplyThemeStyle(themeDefinition.Active);
            var titleToUse = TerminalTextUtils.FitString(title, size.Columns - 4);
            var titleStartX = Math.Max(2, (size.Columns - TerminalTextUtils.GetColumnWidth(titleToUse)) / 2);
            graphics.PutString(titleStartX, 1, titleToUse);
        }

        // Draw left and right borders for the title area
        graphics.ApplyThemeStyle(themeDefinition.Normal);
        graphics.SetCharacter(0, 1, verticalLine);
        graphics.SetCharacter(size.Columns - 1, 1, verticalLine);

        // Draw separator line between title and content
        graphics.SetCharacter(0, 2, tRight);
        graphics.DrawLine(new TerminalPosition(1, 2), new TerminalPosition(size.Columns - 2, 2), horizontalLine);
        graphics.SetCharacter(size.Columns - 1, 2, tLeft);

        // Draw left and right borders for the content area
        for (int row = 3; row < size.Rows - 1; row++)
        {
            graphics.SetCharacter(0, row, verticalLine);
            graphics.SetCharacter(size.Columns - 1, row, verticalLine);
        }

        // Draw bottom border
        graphics.SetCharacter(0, size.Rows - 1, bottomLeftCorner);
        graphics.DrawLine(new TerminalPosition(1, size.Rows - 1), new TerminalPosition(size.Columns - 2, size.Rows - 1), horizontalLine);
        graphics.SetCharacter(size.Columns - 1, size.Rows - 1, bottomRightCorner);
    }

    private void DrawBorderWithoutTitle(ITextGUIGraphics graphics, TerminalSize size, IThemeDefinition themeDefinition)
    {
        // Get box-drawing characters from theme, with fallbacks to Symbols
        var horizontalLine = themeDefinition.GetCharacter("HORIZONTAL", Symbols.SINGLE_LINE_HORIZONTAL);
        var verticalLine = themeDefinition.GetCharacter("VERTICAL", Symbols.SINGLE_LINE_VERTICAL);
        var topLeftCorner = themeDefinition.GetCharacter("TOP_LEFT_CORNER", Symbols.SINGLE_LINE_TOP_LEFT_CORNER);
        var topRightCorner = themeDefinition.GetCharacter("TOP_RIGHT_CORNER", Symbols.SINGLE_LINE_TOP_RIGHT_CORNER);
        var bottomLeftCorner = themeDefinition.GetCharacter("BOTTOM_LEFT_CORNER", Symbols.SINGLE_LINE_BOTTOM_LEFT_CORNER);
        var bottomRightCorner = themeDefinition.GetCharacter("BOTTOM_RIGHT_CORNER", Symbols.SINGLE_LINE_BOTTOM_RIGHT_CORNER);

        graphics.ApplyThemeStyle(themeDefinition.Normal);

        // Draw top border
        graphics.SetCharacter(0, 0, topLeftCorner);
        graphics.DrawLine(new TerminalPosition(1, 0), new TerminalPosition(size.Columns - 2, 0), horizontalLine);
        graphics.SetCharacter(size.Columns - 1, 0, topRightCorner);

        // Draw left and right borders
        for (int row = 1; row < size.Rows - 1; row++)
        {
            graphics.SetCharacter(0, row, verticalLine);
            graphics.SetCharacter(size.Columns - 1, row, verticalLine);
        }

        // Draw bottom border
        graphics.SetCharacter(0, size.Rows - 1, bottomLeftCorner);
        graphics.DrawLine(new TerminalPosition(1, size.Rows - 1), new TerminalPosition(size.Columns - 2, size.Rows - 1), horizontalLine);
        graphics.SetCharacter(size.Columns - 1, size.Rows - 1, bottomRightCorner);
    }
}