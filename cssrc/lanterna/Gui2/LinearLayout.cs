using Lanterna.Core;

namespace Lanterna.Gui2;

public class LinearLayout : ILayoutManager
{
    public enum Alignment
    {
        Beginning,
        Center,
        End,
        Fill
    }

    public enum GrowPolicy
    {
        None,
        CanGrow
    }

    private class LinearLayoutData : ILayoutData
    {
        public Alignment Alignment { get; }
        public GrowPolicy GrowPolicy { get; }

        public LinearLayoutData(Alignment alignment, GrowPolicy growPolicy)
        {
            Alignment = alignment;
            GrowPolicy = growPolicy;
        }
    }

    public static ILayoutData CreateLayoutData(Alignment alignment)
    {
        return CreateLayoutData(alignment, GrowPolicy.None);
    }

    public static ILayoutData CreateLayoutData(Alignment alignment, GrowPolicy growPolicy)
    {
        return new LinearLayoutData(alignment, growPolicy);
    }

    private readonly Direction _direction;
    private int _spacing;
    private bool _changed;

    public LinearLayout() : this(Direction.Vertical)
    {
    }

    public LinearLayout(Direction direction)
    {
        _direction = direction;
        _spacing = direction == Direction.Horizontal ? 1 : 0;
        _changed = true;
    }

    public LinearLayout SetSpacing(int spacing)
    {
        _spacing = spacing;
        _changed = true;
        return this;
    }

    public int Spacing => _spacing;

    public TerminalSize GetPreferredSize(IReadOnlyList<IComponent> components)
    {
        var visibleComponents = components.Where(c => c.Visible).ToList();

        if (_direction == Direction.Vertical)
        {
            return GetPreferredSizeVertically(visibleComponents);
        }
        else
        {
            return GetPreferredSizeHorizontally(visibleComponents);
        }
    }

    private TerminalSize GetPreferredSizeVertically(IReadOnlyList<IComponent> components)
    {
        int maxWidth = 0;
        int height = 0;
        foreach (var component in components)
        {
            var preferredSize = component.PreferredSize;
            if (maxWidth < preferredSize.Columns)
            {
                maxWidth = preferredSize.Columns;
            }
            height += preferredSize.Rows;
        }
        height += _spacing * (components.Count - 1);
        return new TerminalSize(maxWidth, Math.Max(0, height));
    }

    private TerminalSize GetPreferredSizeHorizontally(IReadOnlyList<IComponent> components)
    {
        int maxHeight = 0;
        int width = 0;
        foreach (var component in components)
        {
            var preferredSize = component.PreferredSize;
            if (maxHeight < preferredSize.Rows)
            {
                maxHeight = preferredSize.Rows;
            }
            width += preferredSize.Columns;
        }
        width += _spacing * (components.Count - 1);
        return new TerminalSize(Math.Max(0, width), maxHeight);
    }

    public bool HasChanged => _changed;

    public void DoLayout(TerminalSize area, IReadOnlyList<IComponent> components)
    {
        var visibleComponents = components.Where(c => c.Visible).ToList();

        if (_direction == Direction.Vertical)
        {
            DoFlexibleVerticalLayout(area, visibleComponents);
        }
        else
        {
            DoFlexibleHorizontalLayout(area, visibleComponents);
        }
        _changed = false;
    }

    private void DoFlexibleVerticalLayout(TerminalSize area, IList<IComponent> components)
    {
        int availableVerticalSpace = area.Rows;
        int availableHorizontalSpace = area.Columns;
        var fittingMap = new Dictionary<IComponent, TerminalSize>();
        int totalRequiredVerticalSpace = 0;

        foreach (var component in components)
        {
            var alignment = Alignment.Beginning;
            var layoutData = component.LayoutData;
            if (layoutData is LinearLayoutData linearLayoutData)
            {
                alignment = linearLayoutData.Alignment;
            }

            var preferredSize = component.PreferredSize;
            var fittingSize = new TerminalSize(
                Math.Min(availableHorizontalSpace, preferredSize.Columns),
                preferredSize.Rows);
            if (alignment == Alignment.Fill)
            {
                fittingSize = fittingSize.WithColumns(availableHorizontalSpace);
            }

            fittingMap[component] = fittingSize;
            totalRequiredVerticalSpace += fittingSize.Rows + _spacing;
        }
        if (components.Count > 0)
        {
            totalRequiredVerticalSpace -= _spacing;
        }

        // If we can't fit everything, trim down the size of the largest components until it fits
        if (availableVerticalSpace < totalRequiredVerticalSpace)
        {
            var copyOfComponents = components.ToList();
            copyOfComponents.Reverse();
            copyOfComponents.Sort((o1, o2) => -fittingMap[o1].Rows.CompareTo(fittingMap[o2].Rows));

            while (availableVerticalSpace < totalRequiredVerticalSpace)
            {
                int largestSize = fittingMap[copyOfComponents[0]].Rows;
                foreach (var largeComponent in copyOfComponents)
                {
                    var currentSize = fittingMap[largeComponent];
                    if (largestSize > currentSize.Rows)
                    {
                        break;
                    }
                    fittingMap[largeComponent] = currentSize.WithRelativeRows(-1);
                    totalRequiredVerticalSpace--;
                    if (availableVerticalSpace >= totalRequiredVerticalSpace)
                    {
                        break;
                    }
                }
            }
        }

        // If we have more space available than we need, grow components to fill
        if (availableVerticalSpace > totalRequiredVerticalSpace)
        {
            bool resizedOneComponent = false;
            while (availableVerticalSpace > totalRequiredVerticalSpace)
            {
                foreach (var component in components)
                {
                    var layoutData = (LinearLayoutData?)component.LayoutData;
                    var currentSize = fittingMap[component];
                    if (layoutData?.GrowPolicy == GrowPolicy.CanGrow)
                    {
                        fittingMap[component] = currentSize.WithRelativeRows(1);
                        availableVerticalSpace--;
                        resizedOneComponent = true;
                    }
                    if (availableVerticalSpace <= totalRequiredVerticalSpace)
                    {
                        break;
                    }
                }
                if (!resizedOneComponent)
                {
                    break;
                }
            }
        }

        // Assign the sizes and positions
        int topPosition = 0;
        foreach (var component in components)
        {
            var alignment = Alignment.Beginning;
            var layoutData = component.LayoutData;
            if (layoutData is LinearLayoutData linearLayoutData)
            {
                alignment = linearLayoutData.Alignment;
            }

            var decidedSize = fittingMap[component];
            var position = component.Position;
            position = position.WithRow(topPosition);
            switch (alignment)
            {
                case Alignment.End:
                    position = position.WithColumn(availableHorizontalSpace - decidedSize.Columns);
                    break;
                case Alignment.Center:
                    position = position.WithColumn((availableHorizontalSpace - decidedSize.Columns) / 2);
                    break;
                case Alignment.Beginning:
                default:
                    position = position.WithColumn(0);
                    break;
            }
            component.Position = component.Position.With(position);
            component.Size = component.Size.With(decidedSize);
            topPosition += decidedSize.Rows + _spacing;
        }
    }

    private void DoFlexibleHorizontalLayout(TerminalSize area, IList<IComponent> components)
    {
        int availableVerticalSpace = area.Rows;
        int availableHorizontalSpace = area.Columns;
        var fittingMap = new Dictionary<IComponent, TerminalSize>();
        int totalRequiredHorizontalSpace = 0;

        foreach (var component in components)
        {
            var alignment = Alignment.Beginning;
            var layoutData = component.LayoutData;
            if (layoutData is LinearLayoutData linearLayoutData)
            {
                alignment = linearLayoutData.Alignment;
            }

            var preferredSize = component.PreferredSize;
            var fittingSize = new TerminalSize(
                preferredSize.Columns,
                Math.Min(availableVerticalSpace, preferredSize.Rows));
            if (alignment == Alignment.Fill)
            {
                fittingSize = fittingSize.WithRows(availableVerticalSpace);
            }

            fittingMap[component] = fittingSize;
            totalRequiredHorizontalSpace += fittingSize.Columns + _spacing;
        }
        if (components.Count > 0)
        {
            totalRequiredHorizontalSpace -= _spacing;
        }

        // If we can't fit everything, trim down the size of the largest components until it fits
        if (availableHorizontalSpace < totalRequiredHorizontalSpace)
        {
            var copyOfComponents = components.ToList();
            copyOfComponents.Reverse();
            copyOfComponents.Sort((o1, o2) => -fittingMap[o1].Columns.CompareTo(fittingMap[o2].Columns));

            while (availableHorizontalSpace < totalRequiredHorizontalSpace)
            {
                int largestSize = fittingMap[copyOfComponents[0]].Columns;
                foreach (var largeComponent in copyOfComponents)
                {
                    var currentSize = fittingMap[largeComponent];
                    if (largestSize > currentSize.Columns)
                    {
                        break;
                    }
                    fittingMap[largeComponent] = currentSize.WithRelativeColumns(-1);
                    totalRequiredHorizontalSpace--;
                    if (availableHorizontalSpace >= totalRequiredHorizontalSpace)
                    {
                        break;
                    }
                }
            }
        }

        // If we have more space available than we need, grow components to fill
        if (availableHorizontalSpace > totalRequiredHorizontalSpace)
        {
            bool resizedOneComponent = false;
            while (availableHorizontalSpace > totalRequiredHorizontalSpace)
            {
                foreach (var component in components)
                {
                    var layoutData = (LinearLayoutData?)component.LayoutData;
                    var currentSize = fittingMap[component];
                    if (layoutData?.GrowPolicy == GrowPolicy.CanGrow)
                    {
                        fittingMap[component] = currentSize.WithRelativeColumns(1);
                        availableHorizontalSpace--;
                        resizedOneComponent = true;
                    }
                    if (availableHorizontalSpace <= totalRequiredHorizontalSpace)
                    {
                        break;
                    }
                }
                if (!resizedOneComponent)
                {
                    break;
                }
            }
        }

        // Assign the sizes and positions
        int leftPosition = 0;
        foreach (var component in components)
        {
            var alignment = Alignment.Beginning;
            var layoutData = component.LayoutData;
            if (layoutData is LinearLayoutData linearLayoutData)
            {
                alignment = linearLayoutData.Alignment;
            }

            var decidedSize = fittingMap[component];
            var position = component.Position;
            position = position.WithColumn(leftPosition);
            switch (alignment)
            {
                case Alignment.End:
                    position = position.WithRow(availableVerticalSpace - decidedSize.Rows);
                    break;
                case Alignment.Center:
                    position = position.WithRow((availableVerticalSpace - decidedSize.Rows) / 2);
                    break;
                case Alignment.Beginning:
                default:
                    position = position.WithRow(0);
                    break;
            }
            component.Position = component.Position.With(position);
            component.Size = component.Size.With(decidedSize);
            leftPosition += decidedSize.Columns + _spacing;
        }
    }
}