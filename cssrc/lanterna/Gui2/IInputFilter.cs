using Lanterna.Input;

namespace Lanterna.Gui2;

public interface IInputFilter
{
    bool ShouldAllowInput(KeyStroke keyStroke);
}