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

namespace Lanterna.Core;

/// <summary>
/// SGR - Select Graphic Rendition, changes the state of the terminal as to what kind of text to print after this
/// command. When working with the Terminal interface, its keeping a state of which SGR codes are active, so activating
/// one of these codes will make it apply to all text until you explicitly deactivate it. When you work with Screen and
/// GUI systems, usually the SGR is a property of an independent character and won't affect others.
/// </summary>
public enum SGR
{
    /// <summary>
    /// Bold text mode. Please note that on some terminal implementations, instead of (or in addition to) making the text
    /// bold, it will draw the text in a slightly different color
    /// </summary>
    Bold,

    /// <summary>
    /// Reverse text mode, will flip the foreground and background colors while active
    /// </summary>
    Reverse,

    /// <summary>
    /// Draws a horizontal line under the text. Not widely supported.
    /// </summary>
    Underline,

    /// <summary>
    /// Text will blink on the screen by alternating the foreground color between the real foreground color and the
    /// background color. Not widely supported.
    /// </summary>
    Blink,

    /// <summary>
    /// Draws a border around the text. Rarely supported.
    /// </summary>
    Bordered,

    /// <summary>
    /// I have no idea, exotic extension, please send me a reference screen shots!
    /// </summary>
    Fraktur,

    /// <summary>
    /// Draws a horizontal line through the text. Rarely supported.
    /// </summary>
    CrossedOut,

    /// <summary>
    /// Draws a circle around the text. Rarely supported.
    /// </summary>
    Circled,

    /// <summary>
    /// Italic (cursive) text mode. Some Terminal seem to support it.
    /// </summary>
    Italic
}