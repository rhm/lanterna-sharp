/*
 * This file is part of lanterna (https://github.com/mabe02/lanterna).
 *
 * lanterna is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * Copyright (C) 2010-2020 Martin Berglund
 */

using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;

namespace Lanterna.Core;

/// <summary>
/// This is an abstract base interface for terminal color definitions. Since there are different ways of specifying terminal
/// colors, all with a different range of adoptions, this makes it possible to program an API against an implementation-
/// agnostic color definition. Please remember when using colors that not all terminals and terminal emulators supports
/// them. The 24-bit color mode is very unsupported, for example, and even the default Linux terminal doesn't support
/// the 256-color indexed mode.
/// </summary>
public interface ITextColor
{
    /// <summary>
    /// Returns the byte sequence in between CSI and character 'm' that is used to enable this color as the foreground
    /// color on an ANSI-compatible terminal.
    /// </summary>
    /// <returns>Byte array out data to output in between of CSI and 'm'</returns>
    byte[] GetForegroundSGRSequence();

    /// <summary>
    /// Returns the byte sequence in between CSI and character 'm' that is used to enable this color as the background
    /// color on an ANSI-compatible terminal.
    /// </summary>
    /// <returns>Byte array out data to output in between of CSI and 'm'</returns>
    byte[] GetBackgroundSGRSequence();

    /// <summary>
    /// Red intensity of this color, from 0 to 255
    /// </summary>
    int Red { get; }

    /// <summary>
    /// Green intensity of this color, from 0 to 255  
    /// </summary>
    int Green { get; }

    /// <summary>
    /// Blue intensity of this color, from 0 to 255
    /// </summary>
    int Blue { get; }

    /// <summary>
    /// Converts this color to a System.Drawing.Color object, assuming a standard VGA palette.
    /// </summary>
    /// <returns>TextColor as a System.Drawing.Color</returns>
    Color ToColor();
}

/// <summary>
/// This class represent classic ANSI colors that are likely to be very compatible with most terminal
/// implementations. It is limited to 8 colors (plus the 'default' color) but as a norm, using bold mode (SGR code)
/// will slightly alter the color, giving it a bit brighter tone, so in total this will give you 16 (+1) colors.
/// </summary>
public enum AnsiColor
{
    Black,
    Red,
    Green,
    Yellow,
    Blue,
    Magenta,
    Cyan,
    White,
    Default,
    BlackBright,
    RedBright,
    GreenBright,
    YellowBright,
    BlueBright,
    MagentaBright,
    CyanBright,
    WhiteBright
}

/// <summary>
/// Wrapper class for AnsiColor that implements ITextColor
/// </summary>
public class AnsiTextColor : ITextColor, IEquatable<AnsiTextColor>
{
    private static readonly Dictionary<AnsiColor, (int index, bool bright, int red, int green, int blue)> ColorData = 
        new()
        {
            { AnsiColor.Black, (0, false, 0, 0, 0) },
            { AnsiColor.Red, (1, false, 170, 0, 0) },
            { AnsiColor.Green, (2, false, 0, 170, 0) },
            { AnsiColor.Yellow, (3, false, 170, 85, 0) },
            { AnsiColor.Blue, (4, false, 0, 0, 170) },
            { AnsiColor.Magenta, (5, false, 170, 0, 170) },
            { AnsiColor.Cyan, (6, false, 0, 170, 170) },
            { AnsiColor.White, (7, false, 170, 170, 170) },
            { AnsiColor.Default, (9, false, 0, 0, 0) },
            { AnsiColor.BlackBright, (0, true, 85, 85, 85) },
            { AnsiColor.RedBright, (1, true, 255, 85, 85) },
            { AnsiColor.GreenBright, (2, true, 85, 255, 85) },
            { AnsiColor.YellowBright, (3, true, 255, 255, 85) },
            { AnsiColor.BlueBright, (4, true, 85, 85, 255) },
            { AnsiColor.MagentaBright, (5, true, 255, 85, 255) },
            { AnsiColor.CyanBright, (6, true, 85, 255, 255) },
            { AnsiColor.WhiteBright, (7, true, 255, 255, 255) }
        };

    public AnsiColor Color { get; }

    public AnsiTextColor(AnsiColor color)
    {
        Color = color;
    }

    public byte[] GetForegroundSGRSequence()
    {
        var (index, bright, _, _, _) = ColorData[Color];
        return Encoding.UTF8.GetBytes($"{(bright ? 9 : 3)}{index}");
    }

    public byte[] GetBackgroundSGRSequence()
    {
        var (index, bright, _, _, _) = ColorData[Color];
        return Encoding.UTF8.GetBytes($"{(bright ? 10 : 4)}{index}");
    }

    public bool IsBright => ColorData[Color].bright;

    public int Red => ColorData[Color].red;
    public int Green => ColorData[Color].green;
    public int Blue => ColorData[Color].blue;

    public Color ToColor()
    {
        return System.Drawing.Color.FromArgb(Red, Green, Blue);
    }

    public static implicit operator AnsiTextColor(AnsiColor color)
    {
        return new AnsiTextColor(color);
    }

    public bool Equals(AnsiTextColor? other)
    {
        return other != null && Color == other.Color;
    }

    public override bool Equals(object? obj)
    {
        return obj is AnsiTextColor other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Color.GetHashCode();
    }

    public override string ToString()
    {
        return $"{{ANSI:{Color}}}";
    }
}

/// <summary>
/// This class represents a color expressed in the indexed XTerm 256 color extension, where each color is defined in a
/// lookup-table. All in all, there are 256 codes, but in order to know which one to know you either need to have the
/// table at hand, or you can use the FromRGB static method which can help you convert from three 8-bit
/// RGB values to the closest approximate indexed color number.
/// </summary>
public class IndexedColor : ITextColor, IEquatable<IndexedColor>
{
    private static readonly byte[,] ColorTable = {
        // Standard 16-color VGA palette entries
        {0,0,0}, {170,0,0}, {0,170,0}, {170,85,0}, {0,0,170}, {170,0,170}, {0,170,170}, {170,170,170},
        {85,85,85}, {255,85,85}, {85,255,85}, {255,255,85}, {85,85,255}, {255,85,255}, {85,255,255}, {255,255,255},
        
        // 6x6x6 RGB color cube from 16-231
        {0x00,0x00,0x00}, {0x00,0x00,0x5f}, {0x00,0x00,0x87}, {0x00,0x00,0xaf}, {0x00,0x00,0xd7}, {0x00,0x00,0xff},
        {0x00,0x5f,0x00}, {0x00,0x5f,0x5f}, {0x00,0x5f,0x87}, {0x00,0x5f,0xaf}, {0x00,0x5f,0xd7}, {0x00,0x5f,0xff},
        {0x00,0x87,0x00}, {0x00,0x87,0x5f}, {0x00,0x87,0x87}, {0x00,0x87,0xaf}, {0x00,0x87,0xd7}, {0x00,0x87,0xff},
        {0x00,0xaf,0x00}, {0x00,0xaf,0x5f}, {0x00,0xaf,0x87}, {0x00,0xaf,0xaf}, {0x00,0xaf,0xd7}, {0x00,0xaf,0xff},
        {0x00,0xd7,0x00}, {0x00,0xd7,0x5f}, {0x00,0xd7,0x87}, {0x00,0xd7,0xaf}, {0x00,0xd7,0xd7}, {0x00,0xd7,0xff},
        {0x00,0xff,0x00}, {0x00,0xff,0x5f}, {0x00,0xff,0x87}, {0x00,0xff,0xaf}, {0x00,0xff,0xd7}, {0x00,0xff,0xff},
        {0x5f,0x00,0x00}, {0x5f,0x00,0x5f}, {0x5f,0x00,0x87}, {0x5f,0x00,0xaf}, {0x5f,0x00,0xd7}, {0x5f,0x00,0xff},
        {0x5f,0x5f,0x00}, {0x5f,0x5f,0x5f}, {0x5f,0x5f,0x87}, {0x5f,0x5f,0xaf}, {0x5f,0x5f,0xd7}, {0x5f,0x5f,0xff},
        {0x5f,0x87,0x00}, {0x5f,0x87,0x5f}, {0x5f,0x87,0x87}, {0x5f,0x87,0xaf}, {0x5f,0x87,0xd7}, {0x5f,0x87,0xff},
        {0x5f,0xaf,0x00}, {0x5f,0xaf,0x5f}, {0x5f,0xaf,0x87}, {0x5f,0xaf,0xaf}, {0x5f,0xaf,0xd7}, {0x5f,0xaf,0xff},
        {0x5f,0xd7,0x00}, {0x5f,0xd7,0x5f}, {0x5f,0xd7,0x87}, {0x5f,0xd7,0xaf}, {0x5f,0xd7,0xd7}, {0x5f,0xd7,0xff},
        {0x5f,0xff,0x00}, {0x5f,0xff,0x5f}, {0x5f,0xff,0x87}, {0x5f,0xff,0xaf}, {0x5f,0xff,0xd7}, {0x5f,0xff,0xff},
        {0x87,0x00,0x00}, {0x87,0x00,0x5f}, {0x87,0x00,0x87}, {0x87,0x00,0xaf}, {0x87,0x00,0xd7}, {0x87,0x00,0xff},
        {0x87,0x5f,0x00}, {0x87,0x5f,0x5f}, {0x87,0x5f,0x87}, {0x87,0x5f,0xaf}, {0x87,0x5f,0xd7}, {0x87,0x5f,0xff},
        {0x87,0x87,0x00}, {0x87,0x87,0x5f}, {0x87,0x87,0x87}, {0x87,0x87,0xaf}, {0x87,0x87,0xd7}, {0x87,0x87,0xff},
        {0x87,0xaf,0x00}, {0x87,0xaf,0x5f}, {0x87,0xaf,0x87}, {0x87,0xaf,0xaf}, {0x87,0xaf,0xd7}, {0x87,0xaf,0xff},
        {0x87,0xd7,0x00}, {0x87,0xd7,0x5f}, {0x87,0xd7,0x87}, {0x87,0xd7,0xaf}, {0x87,0xd7,0xd7}, {0x87,0xd7,0xff},
        {0x87,0xff,0x00}, {0x87,0xff,0x5f}, {0x87,0xff,0x87}, {0x87,0xff,0xaf}, {0x87,0xff,0xd7}, {0x87,0xff,0xff},
        {0xaf,0x00,0x00}, {0xaf,0x00,0x5f}, {0xaf,0x00,0x87}, {0xaf,0x00,0xaf}, {0xaf,0x00,0xd7}, {0xaf,0x00,0xff},
        {0xaf,0x5f,0x00}, {0xaf,0x5f,0x5f}, {0xaf,0x5f,0x87}, {0xaf,0x5f,0xaf}, {0xaf,0x5f,0xd7}, {0xaf,0x5f,0xff},
        {0xaf,0x87,0x00}, {0xaf,0x87,0x5f}, {0xaf,0x87,0x87}, {0xaf,0x87,0xaf}, {0xaf,0x87,0xd7}, {0xaf,0x87,0xff},
        {0xaf,0xaf,0x00}, {0xaf,0xaf,0x5f}, {0xaf,0xaf,0x87}, {0xaf,0xaf,0xaf}, {0xaf,0xaf,0xd7}, {0xaf,0xaf,0xff},
        {0xaf,0xd7,0x00}, {0xaf,0xd7,0x5f}, {0xaf,0xd7,0x87}, {0xaf,0xd7,0xaf}, {0xaf,0xd7,0xd7}, {0xaf,0xd7,0xff},
        {0xaf,0xff,0x00}, {0xaf,0xff,0x5f}, {0xaf,0xff,0x87}, {0xaf,0xff,0xaf}, {0xaf,0xff,0xd7}, {0xaf,0xff,0xff},
        {0xd7,0x00,0x00}, {0xd7,0x00,0x5f}, {0xd7,0x00,0x87}, {0xd7,0x00,0xaf}, {0xd7,0x00,0xd7}, {0xd7,0x00,0xff},
        {0xd7,0x5f,0x00}, {0xd7,0x5f,0x5f}, {0xd7,0x5f,0x87}, {0xd7,0x5f,0xaf}, {0xd7,0x5f,0xd7}, {0xd7,0x5f,0xff},
        {0xd7,0x87,0x00}, {0xd7,0x87,0x5f}, {0xd7,0x87,0x87}, {0xd7,0x87,0xaf}, {0xd7,0x87,0xd7}, {0xd7,0x87,0xff},
        {0xd7,0xaf,0x00}, {0xd7,0xaf,0x5f}, {0xd7,0xaf,0x87}, {0xd7,0xaf,0xaf}, {0xd7,0xaf,0xd7}, {0xd7,0xaf,0xff},
        {0xd7,0xd7,0x00}, {0xd7,0xd7,0x5f}, {0xd7,0xd7,0x87}, {0xd7,0xd7,0xaf}, {0xd7,0xd7,0xd7}, {0xd7,0xd7,0xff},
        {0xd7,0xff,0x00}, {0xd7,0xff,0x5f}, {0xd7,0xff,0x87}, {0xd7,0xff,0xaf}, {0xd7,0xff,0xd7}, {0xd7,0xff,0xff},
        {0xff,0x00,0x00}, {0xff,0x00,0x5f}, {0xff,0x00,0x87}, {0xff,0x00,0xaf}, {0xff,0x00,0xd7}, {0xff,0x00,0xff},
        {0xff,0x5f,0x00}, {0xff,0x5f,0x5f}, {0xff,0x5f,0x87}, {0xff,0x5f,0xaf}, {0xff,0x5f,0xd7}, {0xff,0x5f,0xff},
        {0xff,0x87,0x00}, {0xff,0x87,0x5f}, {0xff,0x87,0x87}, {0xff,0x87,0xaf}, {0xff,0x87,0xd7}, {0xff,0x87,0xff},
        {0xff,0xaf,0x00}, {0xff,0xaf,0x5f}, {0xff,0xaf,0x87}, {0xff,0xaf,0xaf}, {0xff,0xaf,0xd7}, {0xff,0xaf,0xff},
        {0xff,0xd7,0x00}, {0xff,0xd7,0x5f}, {0xff,0xd7,0x87}, {0xff,0xd7,0xaf}, {0xff,0xd7,0xd7}, {0xff,0xd7,0xff},
        {0xff,0xff,0x00}, {0xff,0xff,0x5f}, {0xff,0xff,0x87}, {0xff,0xff,0xaf}, {0xff,0xff,0xd7}, {0xff,0xff,0xff},

        // Grey-scale ramp from 232-255
        {0x08,0x08,0x08}, {0x12,0x12,0x12}, {0x1c,0x1c,0x1c}, {0x26,0x26,0x26}, {0x30,0x30,0x30}, {0x3a,0x3a,0x3a},
        {0x44,0x44,0x44}, {0x4e,0x4e,0x4e}, {0x58,0x58,0x58}, {0x62,0x62,0x62}, {0x6c,0x6c,0x6c}, {0x76,0x76,0x76},
        {0x80,0x80,0x80}, {0x8a,0x8a,0x8a}, {0x94,0x94,0x94}, {0x9e,0x9e,0x9e}, {0xa8,0xa8,0xa8}, {0xb2,0xb2,0xb2},
        {0xbc,0xbc,0xbc}, {0xc6,0xc6,0xc6}, {0xd0,0xd0,0xd0}, {0xda,0xda,0xda}, {0xe4,0xe4,0xe4}, {0xee,0xee,0xee}
    };

    public int ColorIndex { get; }

    /// <summary>
    /// Creates a new TextColor using the XTerm 256 color indexed mode, with the specified index value. You must
    /// choose a value between 0 and 255.
    /// </summary>
    /// <param name="colorIndex">Index value to use for this color.</param>
    /// <exception cref="ArgumentException">Thrown when colorIndex is not in range 0-255</exception>
    public IndexedColor(int colorIndex)
    {
        if (colorIndex is > 255 or < 0)
            throw new ArgumentException($"Cannot create a IndexedColor with a color index of {colorIndex}, must be in the range of 0-255");
        
        ColorIndex = colorIndex;
    }

    public byte[] GetForegroundSGRSequence()
    {
        return Encoding.UTF8.GetBytes($"38;5;{ColorIndex}");
    }

    public byte[] GetBackgroundSGRSequence()
    {
        return Encoding.UTF8.GetBytes($"48;5;{ColorIndex}");
    }

    public int Red => ColorTable[ColorIndex, 0];
    public int Green => ColorTable[ColorIndex, 1];
    public int Blue => ColorTable[ColorIndex, 2];

    public Color ToColor()
    {
        return System.Drawing.Color.FromArgb(Red, Green, Blue);
    }

    /// <summary>
    /// Picks out a color approximated from the supplied RGB components
    /// </summary>
    /// <param name="red">Red intensity, from 0 to 255</param>
    /// <param name="green">Green intensity, from 0 to 255</param>
    /// <param name="blue">Blue intensity, from 0 to 255</param>
    /// <returns>Nearest color from the 6x6x6 RGB color cube or from the 24 entries grey-scale ramp (whichever is closest)</returns>
    public static IndexedColor FromRGB(int red, int green, int blue)
    {
        if (red is < 0 or > 255)
            throw new ArgumentException("fromRGB: red is outside of valid range (0-255)");
        if (green is < 0 or > 255)
            throw new ArgumentException("fromRGB: green is outside of valid range (0-255)");
        if (blue is < 0 or > 255)
            throw new ArgumentException("fromRGB: blue is outside of valid range (0-255)");

        int rescaledRed = (int)((red / 255.0) * 5.0);
        int rescaledGreen = (int)((green / 255.0) * 5.0);
        int rescaledBlue = (int)((blue / 255.0) * 5.0);

        int index = rescaledBlue + (6 * rescaledGreen) + (36 * rescaledRed) + 16;
        var fromColorCube = new IndexedColor(index);
        var fromGreyRamp = FromGreyRamp((red + green + blue) / 3);

        // Now figure out which one is closest
        int coloredDistance = ((red - fromColorCube.Red) * (red - fromColorCube.Red)) +
                            ((green - fromColorCube.Green) * (green - fromColorCube.Green)) +
                            ((blue - fromColorCube.Blue) * (blue - fromColorCube.Blue));
        int greyDistance = ((red - fromGreyRamp.Red) * (red - fromGreyRamp.Red)) +
                         ((green - fromGreyRamp.Green) * (green - fromGreyRamp.Green)) +
                         ((blue - fromGreyRamp.Blue) * (blue - fromGreyRamp.Blue));
        
        return coloredDistance < greyDistance ? fromColorCube : fromGreyRamp;
    }

    /// <summary>
    /// Picks out a color from the grey-scale ramp area of the color index.
    /// </summary>
    /// <param name="intensity">Intensity, 0 - 255</param>
    /// <returns>Indexed color from the grey-scale ramp which is the best match for the supplied intensity</returns>
    private static IndexedColor FromGreyRamp(int intensity)
    {
        int rescaled = (int)((intensity / 255.0) * 23.0) + 232;
        return new IndexedColor(rescaled);
    }

    public override string ToString()
    {
        return $"{{IndexedColor:{ColorIndex}}}";
    }

    public bool Equals(IndexedColor? other)
    {
        return other != null && ColorIndex == other.ColorIndex;
    }

    public override bool Equals(object? obj)
    {
        return obj is IndexedColor other && Equals(other);
    }

    public override int GetHashCode()
    {
        return ColorIndex.GetHashCode();
    }
}

/// <summary>
/// This class can be used to specify a color in 24-bit color space (RGB with 8-bit resolution per color). Please be
/// aware that only a few terminal support 24-bit color control codes, please avoid using this class unless you know
/// all users will have compatible terminals.
/// </summary>
public class RgbColor : ITextColor, IEquatable<RgbColor>
{
    public int Red { get; }
    public int Green { get; }
    public int Blue { get; }

    public static RgbColor FromColor(Color color)
    {
        return new RgbColor(color.R, color.G, color.B);
    }

    /// <summary>
    /// This class can be used to specify a color in 24-bit color space (RGB with 8-bit resolution per color). Please be
    /// aware that only a few terminal support 24-bit color control codes, please avoid using this class unless you know
    /// all users will have compatible terminals.
    /// </summary>
    /// <param name="r">Red intensity, from 0 to 255</param>
    /// <param name="g">Green intensity, from 0 to 255</param>
    /// <param name="b">Blue intensity, from 0 to 255</param>
    /// <exception cref="ArgumentException">Thrown when any color component is outside 0-255 range</exception>
    public RgbColor(int r, int g, int b)
    {
        if (r is < 0 or > 255)
            throw new ArgumentException("RGB: r is outside of valid range (0-255)");
        if (g is < 0 or > 255)
            throw new ArgumentException("RGB: g is outside of valid range (0-255)");
        if (b is < 0 or > 255)
            throw new ArgumentException("RGB: b is outside of valid range (0-255)");
        
        Red = r;
        Green = g;
        Blue = b;
    }

    public byte[] GetForegroundSGRSequence()
    {
        return Encoding.UTF8.GetBytes($"38;2;{Red};{Green};{Blue}");
    }

    public byte[] GetBackgroundSGRSequence()
    {
        return Encoding.UTF8.GetBytes($"48;2;{Red};{Green};{Blue}");
    }

    public Color ToColor()
    {
        return System.Drawing.Color.FromArgb(Red, Green, Blue);
    }

    public override string ToString()
    {
        return $"{{RGB:{Red},{Green},{Blue}}}";
    }

    public bool Equals(RgbColor? other)
    {
        return other != null && Red == other.Red && Green == other.Green && Blue == other.Blue;
    }

    public override bool Equals(object? obj)
    {
        return obj is RgbColor other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Red, Green, Blue);
    }
}

/// <summary>
/// Utility class to instantiate colors from other types and definitions
/// </summary>
public static class TextColorFactory
{
    private static readonly Regex IndexedColorPattern = new(@"#[0-9]{1,3}", RegexOptions.Compiled);
    private static readonly Regex RgbColorPattern = new(@"#[0-9a-fA-F]{6}", RegexOptions.Compiled);

    /// <summary>
    /// Parses a string into a color. The string can have one of three formats:
    /// <list type="bullet">
    /// <item><i>blue</i> - Constant value from the AnsiColor enum</item>
    /// <item><i>#17</i> - Hash character followed by one to three numbers; picks the color with that index from the 256 color palette</item>
    /// <item><i>#1a1a1a</i> - Hash character followed by three hex-decimal tuples; creates an RGB color entry by parsing the tuples as Red, Green and Blue</item>
    /// </list>
    /// </summary>
    /// <param name="value">The string value to parse</param>
    /// <returns>A ITextColor that is either an AnsiColor, an IndexedColor or an RgbColor depending on the format of the string, or null if value is null.</returns>
    /// <exception cref="ArgumentException">Thrown when the color definition is not recognized</exception>
    public static ITextColor? FromString(string? value)
    {
        if (value == null)
            return null;

        value = value.Trim();
        
        if (RgbColorPattern.IsMatch(value))
        {
            int r = Convert.ToInt32(value.Substring(1, 2), 16);
            int g = Convert.ToInt32(value.Substring(3, 2), 16);
            int b = Convert.ToInt32(value.Substring(5, 2), 16);
            return new RgbColor(r, g, b);
        }
        
        if (IndexedColorPattern.IsMatch(value))
        {
            int index = int.Parse(value.Substring(1));
            return new IndexedColor(index);
        }
        
        if (Enum.TryParse<AnsiColor>(value, true, out var ansiColor))
        {
            return new AnsiTextColor(ansiColor);
        }
        
        throw new ArgumentException($"Unknown color definition \"{value}\"");
    }
}