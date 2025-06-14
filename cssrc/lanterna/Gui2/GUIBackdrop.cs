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
/// Special component that is by default displayed as the background of a text gui unless you override it with something
/// else. Themes can control how this backdrop is drawn, the normal is one solid color.
/// </summary>
public class GUIBackdrop : EmptySpace
{
    protected override IComponentRenderer<EmptySpace> CreateDefaultRenderer()
    {
        return new GUIBackdropRenderer();
    }

    private class GUIBackdropRenderer : IComponentRenderer<EmptySpace>
    {
        public TerminalSize GetPreferredSize(EmptySpace component)
        {
            return TerminalSize.One;
        }

        public void DrawComponent(ITextGUIGraphics graphics, EmptySpace component)
        {
            var themeDefinition = component.Theme.GetDefinition(typeof(GUIBackdrop));
            graphics.ApplyThemeStyle(themeDefinition.Normal);
            graphics.Fill(themeDefinition.GetCharacter("BACKGROUND", ' '));
        }
    }
}