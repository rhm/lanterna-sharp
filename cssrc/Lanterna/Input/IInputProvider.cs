using System;

namespace Lanterna.Input
{
    /// <summary>
    /// Interface describing an object that can provide keyboard input.
    /// </summary>
    public interface IInputProvider
    {
        /// <summary>
        /// Returns the next available keystroke if one is waiting, otherwise null.
        /// </summary>
        /// <returns>The next keystroke or null.</returns>
        KeyStroke? PollInput();

        /// <summary>
        /// Blocks until a keystroke is available and returns it.
        /// </summary>
        /// <returns>The next keystroke.</returns>
        KeyStroke ReadInput();
    }
}
