using Lanterna.Core;

namespace Lanterna.Graphics;

/// <summary>
/// Default implementation of IShapeRenderer. This class (and the interface) is mostly here to make the code cleaner in
/// AbstractTextGraphics.
/// </summary>
internal class DefaultShapeRenderer : IShapeRenderer
{
    public delegate void Callback(int column, int row, TextCharacter character);

    private readonly Callback _callback;

    public DefaultShapeRenderer(Callback callback)
    {
        _callback = callback;
    }

    public void DrawLine(TerminalPosition p1, TerminalPosition p2, TextCharacter character)
    {
        // http://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm
        // Implementation from Graphics Programming Black Book by Michael Abrash
        // Available at http://www.gamedev.net/page/resources/_/technical/graphics-programming-and-theory/graphics-programming-black-book-r1698
        if (p1.Row > p2.Row)
        {
            (p1, p2) = (p2, p1);
        }

        int deltaX = p2.Column - p1.Column;
        int deltaY = p2.Row - p1.Row;

        if (deltaX > 0)
        {
            if (deltaX > deltaY)
            {
                DrawLine0(p1, deltaX, deltaY, true, character);
            }
            else
            {
                DrawLine1(p1, deltaX, deltaY, true, character);
            }
        }
        else
        {
            deltaX = Math.Abs(deltaX);
            if (deltaX > deltaY)
            {
                DrawLine0(p1, deltaX, deltaY, false, character);
            }
            else
            {
                DrawLine1(p1, deltaX, deltaY, false, character);
            }
        }
    }

    private void DrawLine0(TerminalPosition start, int deltaX, int deltaY, bool leftToRight, TextCharacter character)
    {
        int x = start.Column;
        int y = start.Row;
        int deltaYx2 = deltaY * 2;
        int deltaYx2MinusDeltaXx2 = deltaYx2 - (deltaX * 2);
        int errorTerm = deltaYx2 - deltaX;
        _callback(x, y, character);
        
        while (deltaX-- > 0)
        {
            if (errorTerm >= 0)
            {
                y++;
                errorTerm += deltaYx2MinusDeltaXx2;
            }
            else
            {
                errorTerm += deltaYx2;
            }
            x += leftToRight ? 1 : -1;
            _callback(x, y, character);
        }
    }

    private void DrawLine1(TerminalPosition start, int deltaX, int deltaY, bool leftToRight, TextCharacter character)
    {
        int x = start.Column;
        int y = start.Row;
        int deltaXx2 = deltaX * 2;
        int deltaXx2MinusDeltaYx2 = deltaXx2 - (deltaY * 2);
        int errorTerm = deltaXx2 - deltaY;
        _callback(x, y, character);
        
        while (deltaY-- > 0)
        {
            if (errorTerm >= 0)
            {
                x += leftToRight ? 1 : -1;
                errorTerm += deltaXx2MinusDeltaYx2;
            }
            else
            {
                errorTerm += deltaXx2;
            }
            y++;
            _callback(x, y, character);
        }
    }

    public void DrawTriangle(TerminalPosition p1, TerminalPosition p2, TerminalPosition p3, TextCharacter character)
    {
        DrawLine(p1, p2, character);
        DrawLine(p2, p3, character);
        DrawLine(p3, p1, character);
    }

    public void DrawRectangle(TerminalPosition topLeft, TerminalSize size, TextCharacter character)
    {
        var topRight = topLeft.WithRelativeColumn(size.Columns - 1);
        var bottomRight = topRight.WithRelativeRow(size.Rows - 1);
        var bottomLeft = topLeft.WithRelativeRow(size.Rows - 1);
        
        DrawLine(topLeft, topRight, character);
        DrawLine(topRight, bottomRight, character);
        DrawLine(bottomRight, bottomLeft, character);
        DrawLine(bottomLeft, topLeft, character);
    }

    public void FillTriangle(TerminalPosition p1, TerminalPosition p2, TerminalPosition p3, TextCharacter character)
    {
        // I've used the algorithm described here:
        // http://www-users.mat.uni.torun.pl/~wrona/3d_tutor/tri_fillers.html
        var points = new[] { p1, p2, p3 };
        Array.Sort(points, (a, b) => a.Row.CompareTo(b.Row));

        float dx1, dx2, dx3;
        
        if (points[1].Row - points[0].Row > 0)
        {
            dx1 = (float)(points[1].Column - points[0].Column) / (float)(points[1].Row - points[0].Row);
        }
        else
        {
            dx1 = 0;
        }
        
        if (points[2].Row - points[0].Row > 0)
        {
            dx2 = (float)(points[2].Column - points[0].Column) / (float)(points[2].Row - points[0].Row);
        }
        else
        {
            dx2 = 0;
        }
        
        if (points[2].Row - points[1].Row > 0)
        {
            dx3 = (float)(points[2].Column - points[1].Column) / (float)(points[2].Row - points[1].Row);
        }
        else
        {
            dx3 = 0;
        }

        float startX, startY, endX;
        startX = endX = points[0].Column;
        startY = points[0].Row;
        
        if (dx1 > dx2)
        {
            for (; startY <= points[1].Row; startY++, startX += dx2, endX += dx1)
            {
                DrawLine(new TerminalPosition((int)startX, (int)startY), new TerminalPosition((int)endX, (int)startY), character);
            }
            endX = points[1].Column;
            for (; startY <= points[2].Row; startY++, startX += dx2, endX += dx3)
            {
                DrawLine(new TerminalPosition((int)startX, (int)startY), new TerminalPosition((int)endX, (int)startY), character);
            }
        }
        else
        {
            for (; startY <= points[1].Row; startY++, startX += dx1, endX += dx2)
            {
                DrawLine(new TerminalPosition((int)startX, (int)startY), new TerminalPosition((int)endX, (int)startY), character);
            }
            startX = points[1].Column;
            startY = points[1].Row;
            for (; startY <= points[2].Row; startY++, startX += dx3, endX += dx2)
            {
                DrawLine(new TerminalPosition((int)startX, (int)startY), new TerminalPosition((int)endX, (int)startY), character);
            }
        }
    }

    public void FillRectangle(TerminalPosition topLeft, TerminalSize size, TextCharacter character)
    {
        bool characterDoubleWidth = character.IsDoubleWidth;
        
        for (int y = 0; y < size.Rows; y++)
        {
            for (int x = 0; x < size.Columns; x++)
            {
                // Don't put a double-width character at the right edge of the area
                if (characterDoubleWidth && x + 1 == size.Columns)
                {
                    _callback(topLeft.Column + x, topLeft.Row + y, character.WithCharacter(' '));
                }
                else
                {
                    // Default case
                    _callback(topLeft.Column + x, topLeft.Row + y, character);
                }
                
                if (characterDoubleWidth)
                {
                    x++;
                }
            }
        }
    }
}