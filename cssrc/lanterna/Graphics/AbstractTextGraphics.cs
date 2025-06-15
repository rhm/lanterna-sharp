using Lanterna.Core;
using Lanterna.Screen;

namespace Lanterna.Graphics;

/// <summary>
/// This class holds the default logic for drawing the basic text graphics as exposed by ITextGraphics. 
/// All implementations rely on a SetCharacter method being implemented in subclasses.
/// </summary>
public abstract class AbstractTextGraphics : ITextGraphics
{
    protected ITextColor _foregroundColor;
    protected ITextColor _backgroundColor;
    protected TabBehaviour _tabBehaviour;
    protected readonly ISet<SGR> _activeModifiers;

    protected AbstractTextGraphics()
    {
        _activeModifiers = new HashSet<SGR>();
        _tabBehaviour = TabBehaviour.AlignToColumn4;
        _foregroundColor = new AnsiTextColor(AnsiColor.Default);
        _backgroundColor = new AnsiTextColor(AnsiColor.Default);
    }

    public abstract TerminalSize Size { get; }

    public virtual ITextColor BackgroundColor
    {
        get => _backgroundColor;
        set => _backgroundColor = value;
    }

    public virtual ITextColor ForegroundColor
    {
        get => _foregroundColor;
        set => _foregroundColor = value;
    }

    public virtual ISet<SGR> ActiveModifiers => _activeModifiers;

    public virtual TabBehaviour TabBehaviour
    {
        get => _tabBehaviour;
        set => _tabBehaviour = value;
    }

    public virtual ITextGraphics SetBackgroundColor(ITextColor backgroundColor)
    {
        _backgroundColor = backgroundColor;
        return this;
    }

    public virtual ITextGraphics SetForegroundColor(ITextColor foregroundColor)
    {
        _foregroundColor = foregroundColor;
        return this;
    }

    public virtual ITextGraphics EnableModifiers(params SGR[] modifiers)
    {
        foreach (var modifier in modifiers)
        {
            _activeModifiers.Add(modifier);
        }
        return this;
    }

    public virtual ITextGraphics DisableModifiers(params SGR[] modifiers)
    {
        foreach (var modifier in modifiers)
        {
            _activeModifiers.Remove(modifier);
        }
        return this;
    }

    public virtual ITextGraphics SetModifiers(ISet<SGR> modifiers)
    {
        _activeModifiers.Clear();
        foreach (var modifier in modifiers)
        {
            _activeModifiers.Add(modifier);
        }
        return this;
    }

    public virtual ITextGraphics ClearModifiers()
    {
        _activeModifiers.Clear();
        return this;
    }

    public virtual ITextGraphics SetTabBehaviour(TabBehaviour tabBehaviour)
    {
        _tabBehaviour = tabBehaviour;
        return this;
    }

    public virtual ITextGraphics Fill(char c)
    {
        return FillRectangle(TerminalPosition.TopLeftCorner, Size, c);
    }

    // Abstract method that subclasses must implement
    public abstract ITextGraphics SetCharacter(int column, int row, TextCharacter character);

    public virtual ITextGraphics SetCharacter(TerminalPosition position, TextCharacter character)
    {
        return SetCharacter(position.Column, position.Row, character);
    }

    public virtual ITextGraphics SetCharacter(int column, int row, char character)
    {
        return SetCharacter(column, row, TextCharacter.FromCharacter(character, _foregroundColor, _backgroundColor, _activeModifiers.ToArray()));
    }

    public virtual ITextGraphics SetCharacter(TerminalPosition position, char character)
    {
        return SetCharacter(position.Column, position.Row, character);
    }

    // Basic rectangle implementation - subclasses can override for efficiency
    public virtual ITextGraphics FillRectangle(TerminalPosition topLeft, TerminalSize size, char character)
    {
        return FillRectangle(topLeft, size, TextCharacter.FromCharacter(character, _foregroundColor, _backgroundColor, _activeModifiers.ToArray()));
    }

    public virtual ITextGraphics FillRectangle(TerminalPosition topLeft, TerminalSize size, TextCharacter character)
    {
        for (int row = topLeft.Row; row < topLeft.Row + size.Rows; row++)
        {
            for (int col = topLeft.Column; col < topLeft.Column + size.Columns; col++)
            {
                SetCharacter(col, row, character);
            }
        }
        return this;
    }

    public virtual ITextGraphics DrawRectangle(TerminalPosition topLeft, TerminalSize size, char character)
    {
        return DrawRectangle(topLeft, size, TextCharacter.FromCharacter(character, _foregroundColor, _backgroundColor, _activeModifiers.ToArray()));
    }

    public virtual ITextGraphics DrawRectangle(TerminalPosition topLeft, TerminalSize size, TextCharacter character)
    {
        // Draw top and bottom edges
        for (int col = topLeft.Column; col < topLeft.Column + size.Columns; col++)
        {
            SetCharacter(col, topLeft.Row, character);
            SetCharacter(col, topLeft.Row + size.Rows - 1, character);
        }
        // Draw left and right edges
        for (int row = topLeft.Row; row < topLeft.Row + size.Rows; row++)
        {
            SetCharacter(topLeft.Column, row, character);
            SetCharacter(topLeft.Column + size.Columns - 1, row, character);
        }
        return this;
    }

    // Basic string drawing
    public virtual ITextGraphics PutString(int column, int row, string text)
    {
        return PutString(new TerminalPosition(column, row), text);
    }

    public virtual ITextGraphics PutString(TerminalPosition position, string text)
    {
        int col = position.Column;
        foreach (char c in text)
        {
            SetCharacter(col, position.Row, c);
            col++;
        }
        return this;
    }

    public virtual ITextGraphics PutString(int column, int row, string text, SGR extraModifier, params SGR[] optionalExtraModifiers)
    {
        var previousModifiers = new HashSet<SGR>(_activeModifiers);
        EnableModifiers(extraModifier);
        EnableModifiers(optionalExtraModifiers);
        PutString(column, row, text);
        SetModifiers(previousModifiers);
        return this;
    }

    public virtual ITextGraphics PutString(TerminalPosition position, string text, SGR extraModifier, params SGR[] optionalExtraModifiers)
    {
        return PutString(position.Column, position.Row, text, extraModifier, optionalExtraModifiers);
    }

    public virtual ITextGraphics PutString(int column, int row, string text, ICollection<SGR> extraModifiers)
    {
        var previousModifiers = new HashSet<SGR>(_activeModifiers);
        foreach (var modifier in extraModifiers)
        {
            _activeModifiers.Add(modifier);
        }
        PutString(column, row, text);
        SetModifiers(previousModifiers);
        return this;
    }

    public virtual ITextGraphics DrawLine(TerminalPosition fromPoint, TerminalPosition toPoint, char character)
    {
        // Bresenham's line algorithm implementation
        int dx = Math.Abs(toPoint.Column - fromPoint.Column);
        int dy = Math.Abs(toPoint.Row - fromPoint.Row);
        int sx = fromPoint.Column < toPoint.Column ? 1 : -1;
        int sy = fromPoint.Row < toPoint.Row ? 1 : -1;
        int err = dx - dy;

        int currentColumn = fromPoint.Column;
        int currentRow = fromPoint.Row;

        while (true)
        {
            SetCharacter(currentColumn, currentRow, character);

            if (currentColumn == toPoint.Column && currentRow == toPoint.Row)
                break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                currentColumn += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                currentRow += sy;
            }
        }
        
        return this;
    }

    public virtual ITextGraphics DrawLine(TerminalPosition fromPoint, TerminalPosition toPoint, TextCharacter character)
    {
        return DrawLine(fromPoint, toPoint, character.CharacterString.Length > 0 ? character.CharacterString[0] : ' ');
    }

    public virtual ITextGraphics DrawLine(int fromX, int fromY, int toX, int toY, char character)
    {
        return DrawLine(new TerminalPosition(fromX, fromY), new TerminalPosition(toX, toY), character);
    }

    public virtual ITextGraphics DrawLine(int fromX, int fromY, int toX, int toY, TextCharacter character)
    {
        return DrawLine(fromX, fromY, toX, toY, character.CharacterString.Length > 0 ? character.CharacterString[0] : ' ');
    }

    public virtual ITextGraphics DrawTriangle(TerminalPosition p1, TerminalPosition p2, TerminalPosition p3, char character)
    {
        // Stub implementation
        return this;
    }

    public virtual ITextGraphics DrawTriangle(TerminalPosition p1, TerminalPosition p2, TerminalPosition p3, TextCharacter character)
    {
        return DrawTriangle(p1, p2, p3, character.CharacterString.Length > 0 ? character.CharacterString[0] : ' ');
    }

    public virtual ITextGraphics FillTriangle(TerminalPosition p1, TerminalPosition p2, TerminalPosition p3, char character)
    {
        // Stub implementation
        return this;
    }

    public virtual ITextGraphics FillTriangle(TerminalPosition p1, TerminalPosition p2, TerminalPosition p3, TextCharacter character)
    {
        return FillTriangle(p1, p2, p3, character.CharacterString.Length > 0 ? character.CharacterString[0] : ' ');
    }

    public virtual ITextGraphics DrawImage(TerminalPosition topLeft, ITextImage image)
    {
        if (image == null)
            return this;

        return DrawImage(topLeft, image, TerminalPosition.TopLeftCorner, image.Size);
    }

    public virtual ITextGraphics DrawImage(TerminalPosition topLeft, ITextImage image, TerminalPosition sourceImageTopLeft, TerminalSize sourceImageSize)
    {
        if (image == null)
            return this;

        // Calculate actual copy area bounds
        int sourceStartX = Math.Max(0, sourceImageTopLeft.Column);
        int sourceStartY = Math.Max(0, sourceImageTopLeft.Row);
        int sourceEndX = Math.Min(image.Size.Columns, sourceImageTopLeft.Column + sourceImageSize.Columns);
        int sourceEndY = Math.Min(image.Size.Rows, sourceImageTopLeft.Row + sourceImageSize.Rows);

        // Copy each character from the image to this graphics
        for (int sourceY = sourceStartY; sourceY < sourceEndY; sourceY++)
        {
            for (int sourceX = sourceStartX; sourceX < sourceEndX; sourceX++)
            {
                TextCharacter? character = image.GetCharacterAt(sourceX, sourceY);
                if (character != null)
                {
                    int targetX = topLeft.Column + (sourceX - sourceImageTopLeft.Column);
                    int targetY = topLeft.Row + (sourceY - sourceImageTopLeft.Row);
                    
                    // Check bounds of target graphics
                    if (targetX >= 0 && targetX < Size.Columns && targetY >= 0 && targetY < Size.Rows)
                    {
                        SetCharacter(targetX, targetY, character);
                    }
                }
            }
        }

        return this;
    }

    public virtual ITextGraphics PutCSIStyledString(int column, int row, string text)
    {
        // For now, just put the string without CSI styling
        return PutString(column, row, text);
    }

    public virtual ITextGraphics PutCSIStyledString(TerminalPosition position, string text)
    {
        return PutCSIStyledString(position.Column, position.Row, text);
    }

    public virtual ITextGraphics SetStyleFrom(IStyleSet source)
    {
        // Stub implementation
        return this;
    }

    public virtual TextCharacter? GetCharacter(int column, int row)
    {
        return GetCharacter(new TerminalPosition(column, row));
    }

    public virtual TextCharacter? GetCharacter(TerminalPosition position)
    {
        // Stub implementation - subclasses should override this for full functionality
        return null;
    }

    public abstract ITextGraphics NewTextGraphics(TerminalPosition topLeftCorner, TerminalSize size);

    protected virtual TerminalPosition GetScreenLocation()
    {
        return TerminalPosition.TopLeftCorner;
    }

    public virtual TerminalPosition? ToScreenPosition(TerminalPosition? pos)
    {
        if (pos == null)
            return GetScreenLocation();

        TerminalPosition max = GetScreenLocation() + 
            new TerminalPosition(Size.Columns - 1, Size.Rows - 1);
        TerminalPosition loc = GetScreenLocation() + pos.Value;
        
        if (loc.Column > max.Column || loc.Row > max.Row)
        {
            return null;
        }
        else
        {
            return loc;
        }
    }
}