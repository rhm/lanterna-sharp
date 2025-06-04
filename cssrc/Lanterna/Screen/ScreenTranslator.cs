using Lanterna.Terminal;

namespace Lanterna.Screen
{
    public interface ScreenTranslator
    {
        TerminalPosition? ToScreenPosition(TerminalPosition? pos);
    }
}
