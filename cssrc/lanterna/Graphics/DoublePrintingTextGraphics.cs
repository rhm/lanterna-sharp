using Lanterna.Core;

namespace Lanterna.Graphics;

/// <summary>
/// This TextGraphics implementation wraps another TextGraphics and forwards all operations to it, but with a few
/// differences. First of all, each individual character being printed is printed twice. Secondly, if you call
/// Size, it will return a size that has half the width of the underlying TextGraphics. This presents the
/// writable view as somewhat squared, since normally terminal characters are twice as tall as wide.
/// </summary>
public class DoublePrintingTextGraphics : AbstractTextGraphics
{
    private static readonly TerminalPosition Multiplier = new(2, 1);

    private readonly ITextGraphics _underlyingTextGraphics;

    /// <summary>
    /// Creates a new DoublePrintingTextGraphics on top of a supplied ITextGraphics
    /// </summary>
    /// <param name="underlyingTextGraphics">backend ITextGraphics to forward all the calls to</param>
    public DoublePrintingTextGraphics(ITextGraphics underlyingTextGraphics)
    {
        _underlyingTextGraphics = underlyingTextGraphics;
    }

    public override ITextGraphics SetCharacter(int columnIndex, int rowIndex, TextCharacter textCharacter)
    {
        columnIndex = columnIndex * 2;
        _underlyingTextGraphics.SetCharacter(columnIndex, rowIndex, textCharacter);
        _underlyingTextGraphics.SetCharacter(columnIndex + 1, rowIndex, textCharacter);
        return this;
    }

    public override TextCharacter? GetCharacter(int columnIndex, int rowIndex)
    {
        columnIndex = columnIndex * 2;
        return _underlyingTextGraphics.GetCharacter(columnIndex, rowIndex);
    }

    public override TerminalSize Size
    {
        get
        {
            var size = _underlyingTextGraphics.Size;
            return size.WithColumns(size.Columns / 2);
        }
    }

    public override TerminalPosition? ToScreenPosition(TerminalPosition? pos)
    {
        if (pos == null) return null;
        return _underlyingTextGraphics.ToScreenPosition(pos.Value * Multiplier);
    }

    public override ITextGraphics NewTextGraphics(TerminalPosition topLeftCorner, TerminalSize size)
    {
        return new DoublePrintingTextGraphics(_underlyingTextGraphics.NewTextGraphics(
            topLeftCorner * Multiplier, 
            size.WithColumns(size.Columns * 2)));
    }
}