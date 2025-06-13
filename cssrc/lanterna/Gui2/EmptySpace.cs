using Lanterna.Core;
using Lanterna.Graphics;

namespace Lanterna.Gui2;

/// <summary>
/// Simple component which draws a solid color over its area. The size this component will request is specified through
/// its constructor.
/// </summary>
public class EmptySpace : AbstractComponent<EmptySpace>
{
    private readonly TerminalSize _size;
    private ITextColor? _color;

    /// <summary>
    /// Creates an EmptySpace with size 1x1 and a default color chosen from the theme
    /// </summary>
    public EmptySpace() : this(null, TerminalSize.One)
    {
    }

    /// <summary>
    /// Creates an EmptySpace with a specified color and preferred size of 1x1
    /// </summary>
    /// <param name="color">Color to use (null will make it use the theme)</param>
    public EmptySpace(ITextColor? color) : this(color, TerminalSize.One)
    {
    }

    /// <summary>
    /// Creates an EmptySpace with a specified preferred size (color will be chosen from the theme)
    /// </summary>
    /// <param name="size">Preferred size</param>
    public EmptySpace(TerminalSize size) : this(null, size)
    {
    }

    /// <summary>
    /// Creates an EmptySpace with a specified color (null will make it use a color from the theme) and preferred size
    /// </summary>
    /// <param name="color">Color to use (null will make it use the theme)</param>
    /// <param name="size">Preferred size</param>
    public EmptySpace(ITextColor? color, TerminalSize size)
    {
        _color = color;
        _size = size;
    }

    /// <summary>
    /// Changes the color this component will use when drawn
    /// </summary>
    /// <param name="color">New color to draw the component with, if null then the component will use the theme's
    ///               default color</param>
    public void SetColor(ITextColor? color)
    {
        _color = color;
    }

    /// <summary>
    /// Returns the color this component is drawn with, or null if this component uses whatever the default color
    /// the theme is set to use
    /// </summary>
    public ITextColor? Color => _color;

    protected override IComponentRenderer<EmptySpace> CreateDefaultRenderer()
    {
        return new EmptySpaceRenderer(_size, _color);
    }

    private class EmptySpaceRenderer : IComponentRenderer<EmptySpace>
    {
        private readonly TerminalSize _size;
        private readonly ITextColor? _color;

        public EmptySpaceRenderer(TerminalSize size, ITextColor? color)
        {
            _size = size;
            _color = color;
        }

        public TerminalSize GetPreferredSize(EmptySpace component)
        {
            return _size;
        }

        public void DrawComponent(ITextGUIGraphics graphics, EmptySpace component)
        {
            graphics.ApplyThemeStyle(component.ThemeDefinition.Normal);
            if (_color != null)
            {
                graphics.BackgroundColor = _color;
            }
            graphics.Fill(' ');
        }
    }
}