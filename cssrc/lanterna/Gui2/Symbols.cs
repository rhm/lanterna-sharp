namespace Lanterna.Gui2;

public static class Symbols
{
    // Single line drawing characters
    public const char SingleLineHorizontal = '─';
    public const char SingleLineVertical = '│';
    public const char SingleLineTopLeftCorner = '┌';
    public const char SingleLineTopRightCorner = '┐';
    public const char SingleLineBottomLeftCorner = '└';
    public const char SingleLineBottomRightCorner = '┘';
    public const char SingleLineCross = '┼';
    public const char SingleLineTUp = '┴';
    public const char SingleLineTDown = '┬';
    public const char SingleLineTLeft = '┤';
    public const char SingleLineTRight = '├';
    
    // Legacy underscore versions for backward compatibility
    public const char SingleLineT_Up = '┴';
    public const char SingleLineT_Down = '┬';
    public const char SingleLineT_Left = '┤';
    public const char SingleLineT_Right = '├';
    
    // Double line drawing characters
    public const char DoubleLineHorizontal = '═';
    public const char DoubleLineVertical = '║';
    public const char DoubleLineTopLeftCorner = '╔';
    public const char DoubleLineTopRightCorner = '╗';
    public const char DoubleLineBottomLeftCorner = '╚';
    public const char DoubleLineBottomRightCorner = '╝';
    public const char DoubleLineCross = '╬';
    public const char DoubleLineTUp = '╩';
    public const char DoubleLineTDown = '╦';
    public const char DoubleLineTLeft = '╣';
    public const char DoubleLineTRight = '╠';
    
    // Mixed single/double line T-junctions
    public const char SingleLineTDoubleUp = '╧';
    public const char SingleLineTDoubleDown = '╤';
    public const char SingleLineTDoubleLeft = '╢';
    public const char SingleLineTDoubleRight = '╟';
    public const char DoubleLineTSingleUp = '╨';
    public const char DoubleLineTSingleDown = '╥';
    public const char DoubleLineTSingleLeft = '╡';
    public const char DoubleLineTSingleRight = '╞';
    
    // Mixed crosses
    public const char DoubleLineHorizontalSingleLineCross = '╪';
    public const char DoubleLineVerticalSingleLineCross = '╫';
    
    // Bold line variations (using heavier line drawing characters)
    public const char BoldSingleLineHorizontal = '━';
    public const char BoldSingleLineVertical = '┃';
    public const char BoldFromNormalSingleLineHorizontal = '━';
    public const char BoldFromNormalSingleLineVertical = '┃';
    public const char BoldToNormalSingleLineHorizontal = '━';
    public const char BoldToNormalSingleLineVertical = '┃';
    
    // Block characters
    public const char Block_Solid = '█';
    public const char Block_Dense = '▓';
    public const char Block_Medium = '▒';
    public const char Block_Sparse = '░';
    public const char Block_Middle = '▒';
    
    // For compatibility with Java Lanterna naming
    public const char BLOCK_SOLID = '█';
    public const char BLOCK_DENSE = '▓';
    public const char BLOCK_MIDDLE = '▒';
    public const char BLOCK_SPARSE = '░';
    
    // Arrow and triangle characters
    public const char Triangle_Up_Pointing_Black = '▲';
    public const char Triangle_Down_Pointing_Black = '▼';
    public const char Triangle_Left_Pointing_Black = '◄';
    public const char Triangle_Right_Pointing_Black = '►';
    
    public const char Arrow_Up = '↑';
    public const char Arrow_Down = '↓';
    public const char Arrow_Left = '←';
    public const char Arrow_Right = '→';
    
    // Miscellaneous symbols
    public const char Bullet = '•';
    public const char Heart = '♥';
    public const char Diamond = '♦';
    public const char Club = '♣';
    public const char Spade = '♠';
}