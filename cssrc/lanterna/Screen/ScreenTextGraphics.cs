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
 * Copyright (C) 2010-2024 Martin Berglund
 */

using Lanterna.Core;
using Lanterna.Graphics;

namespace Lanterna.Screen;

/// <summary>
/// This is an implementation of ITextGraphics that targets the output to a Screen. The ScreenTextGraphics object is valid
/// after screen resizing and provides efficient drawing operations on the screen's back buffer.
/// </summary>
public class ScreenTextGraphics : AbstractTextGraphics
{
    private readonly IScreen _screen;

    /// <summary>
    /// Creates a new ScreenTextGraphics targeting the specified screen
    /// </summary>
    /// <param name="screen">Screen we are targeting</param>
    public ScreenTextGraphics(IScreen screen)
    {
        _screen = screen ?? throw new ArgumentNullException(nameof(screen));
    }

    public override TerminalSize Size => _screen.GetTerminalSize();

    public override ITextGraphics SetCharacter(int column, int row, TextCharacter character)
    {
        _screen.SetCharacter(column, row, character);
        return this;
    }

    public override ITextGraphics NewTextGraphics(TerminalPosition topLeftCorner, TerminalSize size)
    {
        // Create a sub-region graphics - for now, return a new instance with the same screen
        // This should be properly implemented to handle the sub-region
        return new ScreenTextGraphics(_screen);
    }
}