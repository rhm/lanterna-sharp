using Lanterna.Core;

namespace Lanterna.Gui2;

public class AbsoluteLayout : ILayoutManager
{
    private bool _changed = true;

    public TerminalSize GetPreferredSize(IReadOnlyList<IComponent> components)
    {
        int maxColumn = 0;
        int maxRow = 0;
        
        foreach (var component in components.Where(c => c.Visible))
        {
            var position = component.Position;
            var size = component.PreferredSize;
            
            maxColumn = Math.Max(maxColumn, position.Column + size.Columns);
            maxRow = Math.Max(maxRow, position.Row + size.Rows);
        }
        
        return new TerminalSize(maxColumn, maxRow);
    }

    public void DoLayout(TerminalSize area, IReadOnlyList<IComponent> components)
    {
        foreach (var component in components.Where(c => c.Visible))
        {
            component.Size = component.PreferredSize;
        }
        _changed = false;
    }

    public bool HasChanged => _changed;
}