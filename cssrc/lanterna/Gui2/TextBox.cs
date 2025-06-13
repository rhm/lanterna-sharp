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
using Lanterna.Input;

namespace Lanterna.Gui2;

/// <summary>
/// Stub implementation of TextBox for dialog system support.
/// TODO: Complete implementation as part of input components porting.
/// </summary>
public class TextBox : AbstractInteractableComponent<TextBox>
{
    private string _text;
    private char? _mask;
    private TerminalSize? _explicitPreferredSize;

    public TextBox() : this(new TerminalSize(10, 1))
    {
    }

    public TextBox(TerminalSize? preferredSize) : this(preferredSize, "")
    {
    }

    public TextBox(TerminalSize? preferredSize, string initialContent)
    {
        _text = initialContent ?? "";
        _mask = null;
        _explicitPreferredSize = preferredSize;
    }

    public string GetText()
    {
        return _text;
    }

    public void SetText(string text)
    {
        _text = text ?? "";
        Invalidate();
    }

    public void SetMask(char mask)
    {
        _mask = mask;
        Invalidate();
    }

    public void ClearMask()
    {
        _mask = null;
        Invalidate();
    }

    protected override IComponentRenderer<TextBox> CreateDefaultRenderer()
    {
        return new TextBoxRenderer();
    }

    private class TextBoxRenderer : IInteractableRenderer<TextBox>
    {
        public TerminalSize GetPreferredSize(TextBox component)
        {
            if (component._explicitPreferredSize.HasValue)
            {
                return component._explicitPreferredSize.Value;
            }
            return new TerminalSize(Math.Max(10, component._text.Length + 1), 1);
        }

        public void DrawComponent(ITextGUIGraphics graphics, TextBox component)
        {
            graphics.ApplyThemeStyle(component.Theme.GetDefinition(typeof(TextBox)).Normal);
            graphics.Fill(' ');

            if (!string.IsNullOrEmpty(component._text))
            {
                string displayText;
                if (component._mask.HasValue)
                {
                    displayText = new string(component._mask.Value, component._text.Length);
                }
                else
                {
                    displayText = component._text;
                }

                if (displayText.Length > graphics.Size.Columns)
                {
                    displayText = displayText.Substring(0, graphics.Size.Columns);
                }
                
                graphics.PutString(0, 0, displayText);
            }
        }

        public TerminalPosition? GetCursorLocation(TextBox component)
        {
            return new TerminalPosition(Math.Min(component._text.Length, component.Size.Columns - 1), 0);
        }
    }
}