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

namespace Lanterna.Terminal;

/// <summary>
/// A null object implementation of ITextGraphics that does nothing. Used as a fallback
/// when terminal operations fail in IOSafeTerminalAdapter.
/// </summary>
internal class NullTextGraphics : ITextGraphics
{
    public ITextColor ForegroundColor { get; set; } = new AnsiTextColor(AnsiColor.Default);
    public ITextColor BackgroundColor { get; set; } = new AnsiTextColor(AnsiColor.Default);

    public void SetCharacter(int column, int row, char character)
    {
        // Do nothing
    }

    public void SetCharacter(TerminalPosition position, char character)
    {
        // Do nothing
    }

    public void SetCharacter(int column, int row, TextCharacter character)
    {
        // Do nothing
    }

    public void SetCharacter(TerminalPosition position, TextCharacter character)
    {
        // Do nothing
    }

    public void PutString(int column, int row, string text)
    {
        // Do nothing
    }

    public void PutString(TerminalPosition position, string text)
    {
        // Do nothing
    }

    public void DrawLine(TerminalPosition fromPoint, TerminalPosition toPoint, char character)
    {
        // Do nothing
    }

    public void DrawLine(int fromColumn, int fromRow, int toColumn, int toRow, char character)
    {
        // Do nothing
    }

    public void DrawRectangle(TerminalPosition topLeft, TerminalSize size, char character)
    {
        // Do nothing
    }

    public void DrawRectangle(int column, int row, int width, int height, char character)
    {
        // Do nothing
    }

    public void FillRectangle(TerminalPosition topLeft, TerminalSize size, char character)
    {
        // Do nothing
    }

    public void FillRectangle(int column, int row, int width, int height, char character)
    {
        // Do nothing
    }
}