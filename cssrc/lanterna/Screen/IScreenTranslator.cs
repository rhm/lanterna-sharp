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
 * Copyright (C) 2010-2020 Svatopluk Dedic
 */
using Lanterna.Core;

namespace Lanterna.Screen;

/// <summary>
/// Mixin interface of an area or location object that provides translation to screen (absolute)
/// coordinates.
/// </summary>
public interface IScreenTranslator
{
    /// <summary>
    /// Returns the screen on terminal coordinates of the given position within the implementing area.
    /// The result value can be used with IScreen object, or its NewTextGraphics(),
    /// it accommodates all coordinate translations from the chain of TextGraphics.
    /// If pos is null, the method returns screen coordinates of origin (usually top-left corner) 
    /// of this area object.
    /// 
    /// Will return null, if the position is outside area implementing IScreenTranslator.
    /// </summary>
    /// <param name="pos">The position to translate, or null to convert position of top-left corner of this area</param>
    /// <returns>Screen coordinates of the given position, or origin (usually top-left corner) of this area, if pos
    /// is null.</returns>
    TerminalPosition? ToScreenPosition(TerminalPosition? pos);
}