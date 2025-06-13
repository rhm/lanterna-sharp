namespace Lanterna.Gui2;

public interface ITextGUIElement
{
    void Draw(ITextGUIGraphics graphics);
    
    bool IsInvalid { get; }
}