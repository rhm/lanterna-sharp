using Lanterna.Core;
using Lanterna.Screen;

namespace Lanterna.Graphics;

/// <summary>
/// This implementation of ITextGraphics will take a 'proper' object and composite a view on top of it, by using a
/// top-left position and a size. Any attempts to put text outside of this area will be dropped.
/// </summary>
internal class SubTextGraphics : AbstractTextGraphics
{
    private readonly ITextGraphics _underlyingTextGraphics;
    private readonly TerminalPosition _topLeft;
    private readonly TerminalPosition _screenTopLeft;
    private readonly TerminalSize _writableAreaSize;

    public SubTextGraphics(ITextGraphics underlyingTextGraphics, TerminalPosition topLeft, TerminalPosition screenRelative, TerminalSize writableAreaSize)
    {
        _underlyingTextGraphics = underlyingTextGraphics;
        _topLeft = topLeft;
        _writableAreaSize = writableAreaSize;
        _screenTopLeft = screenRelative.WithRelative(topLeft);
    }

    private TerminalPosition Project(int column, int row)
    {
        return _topLeft.WithRelative(column, row);
    }

    protected override TerminalPosition GetScreenLocation()
    {
        return _screenTopLeft;
    }

    public override ITextGraphics SetCharacter(int columnIndex, int rowIndex, TextCharacter textCharacter)
    {
        var writableArea = Size;
        if (columnIndex < 0 || columnIndex >= writableArea.Columns ||
            rowIndex < 0 || rowIndex >= writableArea.Rows)
        {
            return this;
        }
        
        var projectedPosition = Project(columnIndex, rowIndex);
        _underlyingTextGraphics.SetCharacter(projectedPosition, textCharacter);
        return this;
    }

    public override TerminalSize Size => _writableAreaSize;

    public override TextCharacter? GetCharacter(int column, int row)
    {
        var projectedPosition = Project(column, row);
        return _underlyingTextGraphics.GetCharacter(projectedPosition.Column, projectedPosition.Row);
    }

    public override ITextGraphics NewTextGraphics(TerminalPosition topLeftCorner, TerminalSize size)
    {
        var writableArea = Size;
        if (topLeftCorner.Column < 0 || topLeftCorner.Row < 0 || 
            topLeftCorner.Column >= writableArea.Columns || topLeftCorner.Row >= writableArea.Rows)
        {
            topLeftCorner = TerminalPosition.TopLeftCorner;
        }
        
        if (topLeftCorner.Column + size.Columns > writableArea.Columns)
        {
            size = size.WithColumns(writableArea.Columns - topLeftCorner.Column);
        }
        
        if (topLeftCorner.Row + size.Rows > writableArea.Rows)
        {
            size = size.WithRows(writableArea.Rows - topLeftCorner.Row);
        }
        
        return new SubTextGraphics(this, topLeftCorner, TerminalPosition.TopLeftCorner, size);
    }
}