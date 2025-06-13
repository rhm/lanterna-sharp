using Lanterna.Core;
using Lanterna.Graphics;

namespace Lanterna.Gui2;

/// <summary>
/// Static non-interactive component that is typically rendered as a single line. Normally this component is used to
/// separate component from each other in situations where a bordered panel isn't ideal. By default the separator will
/// ask for a size of 1x1 so you'll need to make it bigger, either through the layout manager or by overriding the
/// preferred size.
/// </summary>
public class Separator : AbstractComponent<Separator>
{
    private readonly Direction _direction;

    /// <summary>
    /// Creates a new Separator for a specific direction, which will decide whether to draw a horizontal line or
    /// a vertical line
    /// </summary>
    /// <param name="direction">Direction of the line to draw within the separator</param>
    public Separator(Direction direction)
    {
        if (!Enum.IsDefined(typeof(Direction), direction))
        {
            throw new ArgumentException("Invalid direction value");
        }
        _direction = direction;
    }

    /// <summary>
    /// Returns the direction of the line drawn for this separator
    /// </summary>
    public Direction Direction => _direction;

    protected override IComponentRenderer<Separator> CreateDefaultRenderer()
    {
        return new DefaultSeparatorRenderer();
    }

    /// <summary>
    /// Helper interface that doesn't add any new methods but makes coding new separator renderers a little bit more clear
    /// </summary>
    public abstract class SeparatorRenderer : IComponentRenderer<Separator>
    {
        public abstract TerminalSize GetPreferredSize(Separator component);
        public abstract void DrawComponent(ITextGUIGraphics graphics, Separator component);
    }

    /// <summary>
    /// This is the default separator renderer that is used if you don't override anything. With this renderer, the
    /// separator has a preferred size of one but will take up the whole area it is given and fill that space with either
    /// horizontal or vertical lines, depending on the direction of the Separator
    /// </summary>
    public class DefaultSeparatorRenderer : SeparatorRenderer
    {
        public override TerminalSize GetPreferredSize(Separator component)
        {
            return TerminalSize.One;
        }

        public override void DrawComponent(ITextGUIGraphics graphics, Separator component)
        {
            var themeDefinition = component.ThemeDefinition;
            graphics.ApplyThemeStyle(themeDefinition.Normal);
            
            char character = themeDefinition.GetCharacter(
                component.Direction.ToString().ToUpper(),
                component.Direction == Direction.Horizontal ? Symbols.SingleLineHorizontal : Symbols.SingleLineVertical);
            
            graphics.Fill(character);
        }
    }
}