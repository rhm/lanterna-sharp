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
/// This IWindowPostRenderer implementation draws a shadow under the window
/// </summary>
public class WindowShadowRenderer : IWindowPostRenderer
{
    public void PostRender(ITextGUIGraphics textGraphics, IWindowBasedTextGUI textGUI, IWindow window)
    {
        var windowPosition = window.Position;
        var decoratedWindowSize = window.DecoratedSize;
        var themeDefinition = window.Theme.GetDefinition(typeof(WindowShadowRenderer));
        
        textGraphics.ApplyThemeStyle(themeDefinition.Normal);
        char filler = themeDefinition.GetCharacter("FILLER", ' ');
        bool useDoubleWidth = themeDefinition.GetBooleanProperty("DOUBLE_WIDTH", true);
        bool useTransparency = themeDefinition.GetBooleanProperty("TRANSPARENT", false);

        // Draw bottom shadow
        var lowerLeft = windowPosition.WithRelativeColumn(useDoubleWidth ? 2 : 1).WithRelativeRow(decoratedWindowSize.Rows);
        var lowerRight = lowerLeft.WithRelativeColumn(decoratedWindowSize.Columns - (useDoubleWidth ? 3 : 2));
        
        for (int column = lowerLeft.Column; column <= lowerRight.Column + 1; column++)
        {
            char characterToDraw = filler;
            if (useTransparency)
            {
                // For simplified implementation, we'll use the filler character
                // In a full implementation, we'd read the existing character at this position
                characterToDraw = filler;
            }
            textGraphics.SetCharacter(column, lowerLeft.Row, characterToDraw);
        }

        // Draw right shadow
        lowerRight = lowerRight.WithRelativeColumn(1);
        var upperRight = lowerRight.WithRelativeRow(-decoratedWindowSize.Rows + 1);
        
        for (int row = upperRight.Row; row < lowerRight.Row; row++)
        {
            char characterToDraw = filler;
            if (useTransparency)
            {
                // For simplified implementation, we'll use the filler character
                // In a full implementation, we'd read the existing character at this position
                characterToDraw = filler;
            }
            textGraphics.SetCharacter(upperRight.Column, row, characterToDraw);
        }

        // Fill additional column for double-width shadow if needed
        if (useDoubleWidth)
        {
            upperRight = upperRight.WithRelativeColumn(1);
            for (int row = upperRight.Row; row <= lowerRight.Row; row++)
            {
                textGraphics.SetCharacter(upperRight.Column, row, filler);
            }
        }
    }
}