using Lanterna.Core;

namespace Lanterna.Gui2;

/// <summary>
/// BorderLayout imitates the BorderLayout class from AWT, allowing you to add a center component with optional 
/// components around it in top, bottom, left and right locations. The edge components will be sized at their preferred
/// size and the center component will take up whatever remains.
/// </summary>
public class BorderLayout : ILayoutManager
{
    /// <summary>
    /// This type is what you use as the layout data for components added to a panel using BorderLayout for its
    /// layout manager. This values specified where inside the panel the component should be added.
    /// </summary>
    public enum Location
    {
        /// <summary>
        /// The component with this value as its layout data will occupy the center space, whatever is remaining after
        /// the other components (if any) have allocated their space.
        /// </summary>
        Center,
        
        /// <summary>
        /// The component with this value as its layout data will occupy the left side of the container, attempting to
        /// allocate the preferred width of the component and at least the preferred height, but could be more depending
        /// on the other components added.
        /// </summary>
        Left,
        
        /// <summary>
        /// The component with this value as its layout data will occupy the right side of the container, attempting to
        /// allocate the preferred width of the component and at least the preferred height, but could be more depending
        /// on the other components added.
        /// </summary>
        Right,
        
        /// <summary>
        /// The component with this value as its layout data will occupy the top side of the container, attempting to
        /// allocate the preferred height of the component and at least the preferred width, but could be more depending
        /// on the other components added.
        /// </summary>
        Top,
        
        /// <summary>
        /// The component with this value as its layout data will occupy the bottom side of the container, attempting to
        /// allocate the preferred height of the component and at least the preferred width, but could be more depending
        /// on the other components added.
        /// </summary>
        Bottom
    }

    /// <summary>
    /// Layout data wrapper for BorderLayout locations
    /// </summary>
    public class BorderLayoutData : ILayoutData
    {
        public Location Location { get; }

        public BorderLayoutData(Location location)
        {
            Location = location;
        }
    }

    /// <summary>
    /// Static helper methods for creating BorderLayout layout data
    /// </summary>
    public static class BorderLayoutHelper
    {
        public static BorderLayoutData Center => new BorderLayoutData(Location.Center);
        public static BorderLayoutData Left => new BorderLayoutData(Location.Left);
        public static BorderLayoutData Right => new BorderLayoutData(Location.Right);
        public static BorderLayoutData Top => new BorderLayoutData(Location.Top);
        public static BorderLayoutData Bottom => new BorderLayoutData(Location.Bottom);
    }

    // When components don't have a location, we'll assign an available location based on this order
    private static readonly List<Location> AutoAssignOrder = new List<Location>
    {
        Location.Center,
        Location.Top,
        Location.Bottom,
        Location.Left,
        Location.Right
    };

    public TerminalSize GetPreferredSize(IReadOnlyList<IComponent> components)
    {
        var layout = MakeLookupMap(components);
        
        int preferredHeight = 
            (layout.ContainsKey(Location.Top) ? layout[Location.Top].PreferredSize.Rows : 0)
            +
            Math.Max(
                layout.ContainsKey(Location.Left) ? layout[Location.Left].PreferredSize.Rows : 0,
                Math.Max(
                    layout.ContainsKey(Location.Center) ? layout[Location.Center].PreferredSize.Rows : 0,
                    layout.ContainsKey(Location.Right) ? layout[Location.Right].PreferredSize.Rows : 0))
            +
            (layout.ContainsKey(Location.Bottom) ? layout[Location.Bottom].PreferredSize.Rows : 0);

        int preferredWidth = 
            Math.Max(
                (layout.ContainsKey(Location.Left) ? layout[Location.Left].PreferredSize.Columns : 0) +
                    (layout.ContainsKey(Location.Center) ? layout[Location.Center].PreferredSize.Columns : 0) +
                    (layout.ContainsKey(Location.Right) ? layout[Location.Right].PreferredSize.Columns : 0),
                Math.Max(
                    layout.ContainsKey(Location.Top) ? layout[Location.Top].PreferredSize.Columns : 0,
                    layout.ContainsKey(Location.Bottom) ? layout[Location.Bottom].PreferredSize.Columns : 0));
        
        return new TerminalSize(preferredWidth, preferredHeight);
    }

    public void DoLayout(TerminalSize area, IReadOnlyList<IComponent> components)
    {
        var layout = MakeLookupMap(components);
        int availableHorizontalSpace = area.Columns;
        int availableVerticalSpace = area.Rows;
        
        // We'll need this later on
        int topComponentHeight = 0;
        int leftComponentWidth = 0;

        // First allocate the top
        if (layout.ContainsKey(Location.Top))
        {
            var topComponent = layout[Location.Top];
            topComponentHeight = Math.Min(topComponent.PreferredSize.Rows, availableVerticalSpace);
            topComponent.Position = TerminalPosition.TopLeftCorner;
            topComponent.Size = new TerminalSize(availableHorizontalSpace, topComponentHeight);
            availableVerticalSpace -= topComponentHeight;
        }

        // Next allocate the bottom
        if (layout.ContainsKey(Location.Bottom))
        {
            var bottomComponent = layout[Location.Bottom];
            int bottomComponentHeight = Math.Min(bottomComponent.PreferredSize.Rows, availableVerticalSpace);
            bottomComponent.Position = new TerminalPosition(0, area.Rows - bottomComponentHeight);
            bottomComponent.Size = new TerminalSize(availableHorizontalSpace, bottomComponentHeight);
            availableVerticalSpace -= bottomComponentHeight;
        }

        // Now divide the remaining space between LEFT, CENTER and RIGHT
        if (layout.ContainsKey(Location.Left))
        {
            var leftComponent = layout[Location.Left];
            leftComponentWidth = Math.Min(leftComponent.PreferredSize.Columns, availableHorizontalSpace);
            leftComponent.Position = new TerminalPosition(0, topComponentHeight);
            leftComponent.Size = new TerminalSize(leftComponentWidth, availableVerticalSpace);
            availableHorizontalSpace -= leftComponentWidth;
        }
        
        if (layout.ContainsKey(Location.Right))
        {
            var rightComponent = layout[Location.Right];
            int rightComponentWidth = Math.Min(rightComponent.PreferredSize.Columns, availableHorizontalSpace);
            rightComponent.Position = new TerminalPosition(area.Columns - rightComponentWidth, topComponentHeight);
            rightComponent.Size = new TerminalSize(rightComponentWidth, availableVerticalSpace);
            availableHorizontalSpace -= rightComponentWidth;
        }
        
        if (layout.ContainsKey(Location.Center))
        {
            var centerComponent = layout[Location.Center];
            centerComponent.Position = new TerminalPosition(leftComponentWidth, topComponentHeight);
            centerComponent.Size = new TerminalSize(availableHorizontalSpace, availableVerticalSpace);
        }
        
        // Set the remaining components to 0x0
        foreach (var component in components)
        {
            if (component.Visible && !layout.ContainsValue(component))
            {
                component.Position = TerminalPosition.TopLeftCorner;
                component.Size = TerminalSize.Zero;
            }
        }
    }
    
    private Dictionary<Location, IComponent> MakeLookupMap(IReadOnlyList<IComponent> components)
    {
        var map = new Dictionary<Location, IComponent>();
        var unassignedComponents = new List<IComponent>();
        
        foreach (var component in components)
        {
            if (!component.Visible)
            {
                continue;
            }
            
            if (component.LayoutData is BorderLayoutData borderLayoutData)
            {
                map[borderLayoutData.Location] = component;
            }
            else
            {
                unassignedComponents.Add(component);
            }
        }
        
        // Try to assign components to available locations
        foreach (var component in unassignedComponents)
        {
            foreach (var location in AutoAssignOrder)
            {
                if (!map.ContainsKey(location))
                {
                    map[location] = component;
                    break;
                }
            }
        }
        
        return map;
    }

    public bool HasChanged => false; // No internal state
}