using System.Linq;
using System;

namespace Lanterna.Terminal
{
    /// <summary>
    /// Represents a color used by the terminal. This is a simplified
    /// version of the Java TextColor hierarchy.
    /// </summary>
    public interface ITextColor
    {
        byte[] GetForegroundSGRSequence();
        byte[] GetBackgroundSGRSequence();
        int Red { get; }
        int Green { get; }
        int Blue { get; }
    }

    /// <summary>
    /// Simple implementation of a 24-bit RGB text color.
    /// </summary>
    public class RGB : ITextColor
    {
        public int Red { get; }
        public int Green { get; }
        public int Blue { get; }

        private readonly byte[] _fg;
        private readonly byte[] _bg;

        public RGB(int red, int green, int blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
            _fg = $"38;2;{red};{green};{blue}".ToCharArray().Select(c => (byte)c).ToArray();
            _bg = $"48;2;{red};{green};{blue}".ToCharArray().Select(c => (byte)c).ToArray();
        }

        public byte[] GetForegroundSGRSequence() => (byte[])_fg.Clone();
        public byte[] GetBackgroundSGRSequence() => (byte[])_bg.Clone();
    }
}
