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
using Lanterna.Input;

namespace Lanterna.Gui2;

/// <summary>
/// A GUI component that displays a TextImage. This component can be used to show text-based "images" 
/// constructed from TextCharacter arrays, useful for displaying ASCII art, simple graphics, or 
/// pre-rendered text layouts.
/// </summary>
public class ImageComponent : AbstractInteractableComponent<ImageComponent>
{
    private ITextImage _textImage = null!;

    /// <summary>
    /// Creates a new ImageComponent with an empty (0x0) image
    /// </summary>
    public ImageComponent()
    {
        SetTextImage(new BasicTextImage(0, 0));
    }

    /// <summary>
    /// Creates a new ImageComponent with the specified image
    /// </summary>
    /// <param name="textImage">Initial image to display</param>
    public ImageComponent(ITextImage textImage)
    {
        SetTextImage(textImage ?? new BasicTextImage(0, 0));
    }

    /// <summary>
    /// Gets the current TextImage being displayed
    /// </summary>
    /// <returns>The current TextImage</returns>
    public ITextImage GetTextImage()
    {
        return _textImage;
    }

    /// <summary>
    /// Sets the TextImage to display in this component
    /// </summary>
    /// <param name="textImage">The TextImage to display</param>
    public void SetTextImage(ITextImage textImage)
    {
        _textImage = textImage ?? throw new ArgumentNullException(nameof(textImage));
        Invalidate();
    }

    protected override IInteractableRenderer<ImageComponent> CreateDefaultRenderer()
    {
        return new DefaultImageComponentRenderer();
    }

    protected override IInteractable.Result HandleKeyStroke(KeyStroke keyStroke)
    {
        var superResult = base.HandleKeyStroke(keyStroke);

        // Just arrows and focus move stuff
        if (superResult != IInteractable.Result.Unhandled)
        {
            return superResult;
        }

        return IInteractable.Result.Unhandled;
    }

    /// <summary>
    /// Default renderer for ImageComponent that draws the TextImage directly
    /// </summary>
    private class DefaultImageComponentRenderer : IInteractableRenderer<ImageComponent>
    {
        public void DrawComponent(ITextGUIGraphics graphics, ImageComponent component)
        {
            graphics.DrawImage(TerminalPosition.TopLeftCorner, component._textImage);
        }

        public TerminalSize GetPreferredSize(ImageComponent component)
        {
            return component._textImage.Size;
        }

        public TerminalPosition? GetCursorLocation(ImageComponent component)
        {
            // When null, lanterna hides cursor for this component
            return null;
        }
    }
}