namespace Lanterna.Graphics
{
    /// <summary>
    /// Describes an area that can be scrolled by moving a range of lines.
    /// </summary>
    public interface Scrollable
    {
        void ScrollLines(int firstLine, int lastLine, int distance);
    }
}
