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
namespace Lanterna.Input
{
    /// <summary>
    /// Enum type for the different kinds of mouse actions supported
    /// </summary>
    public enum MouseActionType
    {
        CLICK_DOWN,
        CLICK_RELEASE,
        SCROLL_UP,
        SCROLL_DOWN,
        /// <summary>
        /// Moving the mouse cursor on the screen while holding a button down
        /// </summary>
        DRAG,
        /// <summary>
        /// Moving the mouse cursor on the screen without holding any buttons down
        /// </summary>
        MOVE
    }
}