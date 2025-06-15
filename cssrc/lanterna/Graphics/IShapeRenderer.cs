using Lanterna.Core;

namespace Lanterna.Graphics;

/// <summary>
/// This internal interface exposes methods for translating abstract lines, triangles and rectangles to discrete
/// points on a grid.
/// </summary>
internal interface IShapeRenderer
{
    void DrawLine(TerminalPosition p1, TerminalPosition p2, TextCharacter character);
    void DrawTriangle(TerminalPosition p1, TerminalPosition p2, TerminalPosition p3, TextCharacter character);
    void DrawRectangle(TerminalPosition topLeft, TerminalSize size, TextCharacter character);
    void FillTriangle(TerminalPosition p1, TerminalPosition p2, TerminalPosition p3, TextCharacter character);
    void FillRectangle(TerminalPosition topLeft, TerminalSize size, TextCharacter character);
}