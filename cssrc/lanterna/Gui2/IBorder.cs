using Lanterna.Core;

namespace Lanterna.Gui2;

/// <summary>
/// Main interface for different border classes, with additional methods to help lanterna figure out the size and offset
/// of components wrapped by borders.
/// </summary>
public interface IBorder : IContainer, IComposite
{
    /// <summary>
    /// Border renderer interface for custom border rendering
    /// </summary>
    public interface IBorderRenderer : IComponentRenderer<IBorder>
    {
        /// <summary>
        /// How large is the offset from the top left corner of the border to the top left corner of the wrapped component?
        /// </summary>
        /// <returns>Position of the wrapped components top left position, relative to the top left corner of the border</returns>
        TerminalPosition GetWrappedComponentTopLeftOffset();

        /// <summary>
        /// Given a total size of the border composite and it's wrapped component, how large would the actual wrapped
        /// component be?
        /// </summary>
        /// <param name="borderSize">Size to calculate for, this should be the total size of the border and the inner component</param>
        /// <returns>Size of the inner component if the total size of inner + border is borderSize</returns>
        TerminalSize GetWrappedComponentSize(TerminalSize borderSize);
    }
}