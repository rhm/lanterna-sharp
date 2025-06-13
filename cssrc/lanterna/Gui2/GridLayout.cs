using Lanterna.Core;

namespace Lanterna.Gui2;

/// <summary>
/// This emulates the behaviour of the GridLayout in SWT (as opposed to the one in AWT/Swing). I originally ported the
/// SWT class itself but due to licensing concerns (the eclipse license is not compatible with LGPL) I was advised not to
/// do that. This is a partial implementation and some of the semantics have changed, but in general it works the same
/// way so the SWT documentation will generally match.
/// <para>
/// You use the GridLayout by specifying a number of columns you want your grid to have and then when you add
/// components, you assign LayoutData to these components using the different static methods in this class
/// (CreateLayoutData(..)). You can set components to span both rows and columns, as well as defining how to
/// distribute the available space.
/// </para>
/// </summary>
public class GridLayout : ILayoutManager
{
    /// <summary>
    /// The enum is used to specify where in a grid cell a component should be placed, in the case that the preferred
    /// size of the component is smaller than the space in the cell. This class will generally use two alignments, one
    /// for horizontal and one for vertical.
    /// </summary>
    public enum Alignment
    {
        /// <summary>
        /// Place the component at the start of the cell (horizontally or vertically) and leave whatever space is left
        /// after the preferred size empty.
        /// </summary>
        Beginning,
        
        /// <summary>
        /// Place the component at the middle of the cell (horizontally or vertically) and leave the space before and
        /// after empty.
        /// </summary>
        Center,
        
        /// <summary>
        /// Place the component at the end of the cell (horizontally or vertically) and leave whatever space is left
        /// before the preferred size empty.
        /// </summary>
        End,
        
        /// <summary>
        /// Force the component to be the same size as the table cell
        /// </summary>
        Fill
    }

    private class GridLayoutData : ILayoutData
    {
        public readonly Alignment HorizontalAlignment;
        public readonly Alignment VerticalAlignment;
        public readonly bool GrabExtraHorizontalSpace;
        public readonly bool GrabExtraVerticalSpace;
        public readonly int HorizontalSpan;
        public readonly int VerticalSpan;

        public GridLayoutData(
            Alignment horizontalAlignment,
            Alignment verticalAlignment,
            bool grabExtraHorizontalSpace,
            bool grabExtraVerticalSpace,
            int horizontalSpan,
            int verticalSpan)
        {
            if (horizontalSpan < 1 || verticalSpan < 1)
            {
                throw new ArgumentException("Horizontal/Vertical span must be 1 or greater");
            }

            HorizontalAlignment = horizontalAlignment;
            VerticalAlignment = verticalAlignment;
            GrabExtraHorizontalSpace = grabExtraHorizontalSpace;
            GrabExtraVerticalSpace = grabExtraVerticalSpace;
            HorizontalSpan = horizontalSpan;
            VerticalSpan = verticalSpan;
        }
    }

    private static readonly GridLayoutData DEFAULT = new GridLayoutData(
        Alignment.Beginning,
        Alignment.Beginning,
        false,
        false,
        1,
        1);

    /// <summary>
    /// Creates a layout data object for GridLayouts that specify the horizontal and vertical alignment for the
    /// component in case the cell space is larger than the preferred size of the component
    /// </summary>
    /// <param name="horizontalAlignment">Horizontal alignment strategy</param>
    /// <param name="verticalAlignment">Vertical alignment strategy</param>
    /// <returns>The layout data object containing the specified alignments</returns>
    public static ILayoutData CreateLayoutData(Alignment horizontalAlignment, Alignment verticalAlignment)
    {
        return CreateLayoutData(horizontalAlignment, verticalAlignment, false, false);
    }

    /// <summary>
    /// Creates a layout data object for GridLayouts that specify the horizontal and vertical alignment for the
    /// component in case the cell space is larger than the preferred size of the component. This method also has fields
    /// for indicating that the component would like to take more space if available to the container. For example, if
    /// the container is assigned is assigned an area of 50x15, but all the child components in the grid together only
    /// asks for 40x10, the remaining 10 columns and 5 rows will be empty. If just a single component asks for extra
    /// space horizontally and/or vertically, the grid will expand out to fill the entire area and the text space will be
    /// assigned to the component that asked for it.
    /// </summary>
    /// <param name="horizontalAlignment">Horizontal alignment strategy</param>
    /// <param name="verticalAlignment">Vertical alignment strategy</param>
    /// <param name="grabExtraHorizontalSpace">If set to true, this component will ask to be assigned extra horizontal
    ///                                space if there is any to assign</param>
    /// <param name="grabExtraVerticalSpace">If set to true, this component will ask to be assigned extra vertical
    ///                                space if there is any to assign</param>
    /// <returns>The layout data object containing the specified alignments and size requirements</returns>
    public static ILayoutData CreateLayoutData(
        Alignment horizontalAlignment,
        Alignment verticalAlignment,
        bool grabExtraHorizontalSpace,
        bool grabExtraVerticalSpace)
    {
        return CreateLayoutData(horizontalAlignment, verticalAlignment, grabExtraHorizontalSpace, grabExtraVerticalSpace, 1, 1);
    }

    /// <summary>
    /// Creates a layout data object for GridLayouts that specify the horizontal and vertical alignment for the
    /// component in case the cell space is larger than the preferred size of the component. This method also has fields
    /// for indicating that the component would like to take more space if available to the container. For example, if
    /// the container is assigned is assigned an area of 50x15, but all the child components in the grid together only
    /// asks for 40x10, the remaining 10 columns and 5 rows will be empty. If just a single component asks for extra
    /// space horizontally and/or vertically, the grid will expand out to fill the entire area and the text space will be
    /// assigned to the component that asked for it. It also puts in data on how many rows and/or columns the component
    /// should span.
    /// </summary>
    /// <param name="horizontalAlignment">Horizontal alignment strategy</param>
    /// <param name="verticalAlignment">Vertical alignment strategy</param>
    /// <param name="grabExtraHorizontalSpace">If set to true, this component will ask to be assigned extra horizontal
    ///                                space if there is any to assign</param>
    /// <param name="grabExtraVerticalSpace">If set to true, this component will ask to be assigned extra vertical
    ///                                space if there is any to assign</param>
    /// <param name="horizontalSpan">How many "cells" this component wants to span horizontally</param>
    /// <param name="verticalSpan">How many "cells" this component wants to span vertically</param>
    /// <returns>The layout data object containing the specified alignments, size requirements and cell spanning</returns>
    public static ILayoutData CreateLayoutData(
        Alignment horizontalAlignment,
        Alignment verticalAlignment,
        bool grabExtraHorizontalSpace,
        bool grabExtraVerticalSpace,
        int horizontalSpan,
        int verticalSpan)
    {
        return new GridLayoutData(
            horizontalAlignment,
            verticalAlignment,
            grabExtraHorizontalSpace,
            grabExtraVerticalSpace,
            horizontalSpan,
            verticalSpan);
    }

    /// <summary>
    /// This is a shortcut method that will create a grid layout data object that will expand its cell as much as is can
    /// horizontally and make the component occupy the whole area horizontally and center it vertically, spanning 1 cell.
    /// </summary>
    /// <returns>Layout data object with the specified span and horizontally expanding as much as it can</returns>
    public static ILayoutData CreateHorizontallyFilledLayoutData()
    {
        return CreateLayoutData(
            Alignment.Fill,
            Alignment.Center,
            true,
            false,
            1,
            1);
    }

    /// <summary>
    /// This is a shortcut method that will create a grid layout data object that will expand its cell as much as is can
    /// horizontally and make the component occupy the whole area horizontally and center it vertically
    /// </summary>
    /// <param name="horizontalSpan">How many cells to span horizontally</param>
    /// <returns>Layout data object with the specified span and horizontally expanding as much as it can</returns>
    public static ILayoutData CreateHorizontallyFilledLayoutData(int horizontalSpan)
    {
        return CreateLayoutData(
            Alignment.Fill,
            Alignment.Center,
            true,
            false,
            horizontalSpan,
            1);
    }

    /// <summary>
    /// This is a shortcut method that will create a grid layout data object that will expand its cell as much as is can
    /// vertically and make the component occupy the whole area vertically and center it horizontally
    /// </summary>
    /// <param name="horizontalSpan">How many cells to span vertically</param>
    /// <returns>Layout data object with the specified span and vertically expanding as much as it can</returns>
    public static ILayoutData CreateHorizontallyEndAlignedLayoutData(int horizontalSpan)
    {
        return CreateLayoutData(
            Alignment.End,
            Alignment.Center,
            true,
            false,
            horizontalSpan,
            1);
    }

    private readonly int _numberOfColumns;
    private int _horizontalSpacing;
    private int _verticalSpacing;
    private int _topMarginSize;
    private int _bottomMarginSize;
    private int _leftMarginSize;
    private int _rightMarginSize;

    private bool _changed;

    /// <summary>
    /// Creates a new GridLayout with the specified number of columns. Initially, this layout will have a
    /// horizontal spacing of 1 and vertical spacing of 0, with a left and right margin of 1.
    /// </summary>
    /// <param name="numberOfColumns">Number of columns in this grid</param>
    public GridLayout(int numberOfColumns)
    {
        _numberOfColumns = numberOfColumns;
        _horizontalSpacing = 1;
        _verticalSpacing = 0;
        _topMarginSize = 0;
        _bottomMarginSize = 0;
        _leftMarginSize = 1;
        _rightMarginSize = 1;
        _changed = true;
    }

    /// <summary>
    /// Returns the horizontal spacing, i.e. the number of empty columns between each cell
    /// </summary>
    /// <returns>Horizontal spacing</returns>
    public int HorizontalSpacing => _horizontalSpacing;

    /// <summary>
    /// Sets the horizontal spacing, i.e. the number of empty columns between each cell
    /// </summary>
    /// <param name="horizontalSpacing">New horizontal spacing</param>
    /// <returns>Itself</returns>
    public GridLayout SetHorizontalSpacing(int horizontalSpacing)
    {
        if (horizontalSpacing < 0)
        {
            throw new ArgumentException("Horizontal spacing cannot be less than 0");
        }
        _horizontalSpacing = horizontalSpacing;
        _changed = true;
        return this;
    }

    /// <summary>
    /// Returns the vertical spacing, i.e. the number of empty columns between each row
    /// </summary>
    /// <returns>Vertical spacing</returns>
    public int VerticalSpacing => _verticalSpacing;

    /// <summary>
    /// Sets the vertical spacing, i.e. the number of empty columns between each row
    /// </summary>
    /// <param name="verticalSpacing">New vertical spacing</param>
    /// <returns>Itself</returns>
    public GridLayout SetVerticalSpacing(int verticalSpacing)
    {
        if (verticalSpacing < 0)
        {
            throw new ArgumentException("Vertical spacing cannot be less than 0");
        }
        _verticalSpacing = verticalSpacing;
        _changed = true;
        return this;
    }

    /// <summary>
    /// Returns the top margin, i.e. number of empty rows above the first row in the grid
    /// </summary>
    /// <returns>Top margin, in number of rows</returns>
    public int TopMarginSize => _topMarginSize;

    /// <summary>
    /// Sets the top margin, i.e. number of empty rows above the first row in the grid
    /// </summary>
    /// <param name="topMarginSize">Top margin, in number of rows</param>
    /// <returns>Itself</returns>
    public GridLayout SetTopMarginSize(int topMarginSize)
    {
        if (topMarginSize < 0)
        {
            throw new ArgumentException("Top margin size cannot be less than 0");
        }
        _topMarginSize = topMarginSize;
        _changed = true;
        return this;
    }

    /// <summary>
    /// Returns the bottom margin, i.e. number of empty rows below the last row in the grid
    /// </summary>
    /// <returns>Bottom margin, in number of rows</returns>
    public int BottomMarginSize => _bottomMarginSize;

    /// <summary>
    /// Sets the bottom margin, i.e. number of empty rows below the last row in the grid
    /// </summary>
    /// <param name="bottomMarginSize">Bottom margin, in number of rows</param>
    /// <returns>Itself</returns>
    public GridLayout SetBottomMarginSize(int bottomMarginSize)
    {
        if (bottomMarginSize < 0)
        {
            throw new ArgumentException("Bottom margin size cannot be less than 0");
        }
        _bottomMarginSize = bottomMarginSize;
        _changed = true;
        return this;
    }

    /// <summary>
    /// Returns the left margin, i.e. number of empty columns left of the first column in the grid
    /// </summary>
    /// <returns>Left margin, in number of columns</returns>
    public int LeftMarginSize => _leftMarginSize;

    /// <summary>
    /// Sets the left margin, i.e. number of empty columns left of the first column in the grid
    /// </summary>
    /// <param name="leftMarginSize">Left margin, in number of columns</param>
    /// <returns>Itself</returns>
    public GridLayout SetLeftMarginSize(int leftMarginSize)
    {
        if (leftMarginSize < 0)
        {
            throw new ArgumentException("Left margin size cannot be less than 0");
        }
        _leftMarginSize = leftMarginSize;
        _changed = true;
        return this;
    }

    /// <summary>
    /// Returns the right margin, i.e. number of empty columns right of the last column in the grid
    /// </summary>
    /// <returns>Right margin, in number of columns</returns>
    public int RightMarginSize => _rightMarginSize;

    /// <summary>
    /// Sets the right margin, i.e. number of empty columns right of the last column in the grid
    /// </summary>
    /// <param name="rightMarginSize">Right margin, in number of columns</param>
    /// <returns>Itself</returns>
    public GridLayout SetRightMarginSize(int rightMarginSize)
    {
        if (rightMarginSize < 0)
        {
            throw new ArgumentException("Right margin size cannot be less than 0");
        }
        _rightMarginSize = rightMarginSize;
        _changed = true;
        return this;
    }

    public bool HasChanged => _changed;

    public TerminalSize GetPreferredSize(IReadOnlyList<IComponent> components)
    {
        var preferredSize = TerminalSize.Zero;
        if (components.Count == 0)
        {
            return preferredSize.WithRelative(
                _leftMarginSize + _rightMarginSize,
                _topMarginSize + _bottomMarginSize);
        }

        var table = BuildTable(components);
        table = EliminateUnusedRowsAndColumns(table);

        // Figure out each column first, this can be done independently of the row heights
        int preferredWidth = 0;
        int preferredHeight = 0;
        foreach (int width in GetPreferredColumnWidths(table))
        {
            preferredWidth += width;
        }
        foreach (int height in GetPreferredRowHeights(table))
        {
            preferredHeight += height;
        }
        preferredSize = preferredSize.WithRelative(preferredWidth, preferredHeight);
        preferredSize = preferredSize.WithRelativeColumns(_leftMarginSize + _rightMarginSize + (table[0].Length - 1) * _horizontalSpacing);
        preferredSize = preferredSize.WithRelativeRows(_topMarginSize + _bottomMarginSize + (table.Length - 1) * _verticalSpacing);
        return preferredSize;
    }

    public void DoLayout(TerminalSize area, IReadOnlyList<IComponent> components)
    {
        // Sanity check, if the area is way too small, just return
        var table = BuildTable(components);
        table = EliminateUnusedRowsAndColumns(table);

        if (area.Equals(TerminalSize.Zero) ||
            table.Length == 0 ||
            area.Columns <= _leftMarginSize + _rightMarginSize + ((table[0].Length - 1) * _horizontalSpacing) ||
            area.Rows <= _bottomMarginSize + _topMarginSize + ((table.Length - 1) * _verticalSpacing))
        {
            _changed = false;
            return;
        }

        // Adjust area to the margins
        area = area.WithRelative(-_leftMarginSize - _rightMarginSize, -_topMarginSize - _bottomMarginSize);

        var sizeMap = new Dictionary<IComponent, TerminalSize>();
        var positionMap = new Dictionary<IComponent, TerminalPosition>();

        // Figure out each column first, this can be done independently of the row heights
        int[] columnWidths = GetPreferredColumnWidths(table);

        // Take notes of which columns we can expand if the usable area is larger than what the components want
        var expandableColumns = GetExpandableColumns(table);

        // Next, start shrinking to make sure it fits the size of the area we are trying to lay out on.
        // Notice we subtract the horizontalSpacing to take the space between components into account
        var areaWithoutHorizontalSpacing = area.WithRelativeColumns(-_horizontalSpacing * (table[0].Length - 1));
        int totalWidth = ShrinkWidthToFitArea(areaWithoutHorizontalSpacing, columnWidths);

        // Finally, if there is extra space, make the expandable columns larger
        while (areaWithoutHorizontalSpacing.Columns > totalWidth && expandableColumns.Count > 0)
        {
            totalWidth = GrabExtraHorizontalSpace(areaWithoutHorizontalSpacing, columnWidths, expandableColumns, totalWidth);
        }

        // Now repeat for rows
        int[] rowHeights = GetPreferredRowHeights(table);
        var expandableRows = GetExpandableRows(table);
        var areaWithoutVerticalSpacing = area.WithRelativeRows(-_verticalSpacing * (table.Length - 1));
        int totalHeight = ShrinkHeightToFitArea(areaWithoutVerticalSpacing, rowHeights);
        while (areaWithoutVerticalSpacing.Rows > totalHeight && expandableRows.Count > 0)
        {
            totalHeight = GrabExtraVerticalSpace(areaWithoutVerticalSpacing, rowHeights, expandableRows, totalHeight);
        }

        // Ok, all constraints are in place, we can start placing out components. To simplify, do it horizontally first
        // and vertically after
        var tableCellTopLeft = TerminalPosition.TopLeftCorner;
        for (int y = 0; y < table.Length; y++)
        {
            tableCellTopLeft = tableCellTopLeft.WithColumn(0);
            for (int x = 0; x < table[y].Length; x++)
            {
                var component = table[y][x];
                if (component != null && !positionMap.ContainsKey(component))
                {
                    var layoutData = GetLayoutData(component);
                    var size = component.PreferredSize;
                    var position = tableCellTopLeft;

                    int availableHorizontalSpace = 0;
                    int availableVerticalSpace = 0;
                    for (int i = 0; i < layoutData.HorizontalSpan && x + i < columnWidths.Length; i++)
                    {
                        availableHorizontalSpace += columnWidths[x + i] + (i > 0 ? _horizontalSpacing : 0);
                    }
                    for (int i = 0; i < layoutData.VerticalSpan; i++)
                    {
                        availableVerticalSpace += rowHeights[y + i] + (i > 0 ? _verticalSpacing : 0);
                    }

                    // Make sure to obey the size restrictions
                    size = size.WithColumns(Math.Min(size.Columns, availableHorizontalSpace));
                    size = size.WithRows(Math.Min(size.Rows, availableVerticalSpace));

                    switch (layoutData.HorizontalAlignment)
                    {
                        case Alignment.Center:
                            position = position.WithRelativeColumn((availableHorizontalSpace - size.Columns) / 2);
                            break;
                        case Alignment.End:
                            position = position.WithRelativeColumn(availableHorizontalSpace - size.Columns);
                            break;
                        case Alignment.Fill:
                            size = size.WithColumns(availableHorizontalSpace);
                            break;
                        default:
                            break;
                    }
                    switch (layoutData.VerticalAlignment)
                    {
                        case Alignment.Center:
                            position = position.WithRelativeRow((availableVerticalSpace - size.Rows) / 2);
                            break;
                        case Alignment.End:
                            position = position.WithRelativeRow(availableVerticalSpace - size.Rows);
                            break;
                        case Alignment.Fill:
                            size = size.WithRows(availableVerticalSpace);
                            break;
                        default:
                            break;
                    }

                    sizeMap[component] = size;
                    positionMap[component] = position;
                }
                tableCellTopLeft = tableCellTopLeft.WithRelativeColumn(columnWidths[x] + _horizontalSpacing);
            }
            tableCellTopLeft = tableCellTopLeft.WithRelativeRow(rowHeights[y] + _verticalSpacing);
        }

        // Apply the margins here
        foreach (var component in components)
        {
            component.Position = positionMap[component].WithRelative(_leftMarginSize, _topMarginSize);
            component.Size = sizeMap[component];
        }
        _changed = false;
    }

    private int[] GetPreferredColumnWidths(IComponent?[][] table)
    {
        // actualNumberOfColumns may be different from this._numberOfColumns since some columns may have been eliminated
        int actualNumberOfColumns = table[0].Length;
        int[] columnWidths = new int[actualNumberOfColumns];

        // Start by letting all span = 1 columns take what they need
        foreach (var row in table)
        {
            for (int i = 0; i < actualNumberOfColumns; i++)
            {
                var component = row[i];
                if (component == null)
                {
                    continue;
                }
                var layoutData = GetLayoutData(component);
                if (layoutData.HorizontalSpan == 1)
                {
                    columnWidths[i] = Math.Max(columnWidths[i], component.PreferredSize.Columns);
                }
            }
        }

        // Next, do span > 1 and enlarge if necessary
        foreach (var row in table)
        {
            for (int i = 0; i < actualNumberOfColumns;)
            {
                var component = row[i];
                if (component == null)
                {
                    i++;
                    continue;
                }
                var layoutData = GetLayoutData(component);
                int horizontalSpan = Math.Min(layoutData.HorizontalSpan, actualNumberOfColumns - i);
                if (horizontalSpan > 1)
                {
                    int accumWidth = 0;
                    for (int j = i; j < i + horizontalSpan; j++)
                    {
                        accumWidth += columnWidths[j];
                    }

                    int preferredWidth = component.PreferredSize.Columns;
                    if (preferredWidth > accumWidth)
                    {
                        int columnOffset = 0;
                        do
                        {
                            columnWidths[i + columnOffset++]++;
                            accumWidth++;
                            if (columnOffset == horizontalSpan)
                            {
                                columnOffset = 0;
                            }
                        }
                        while (preferredWidth > accumWidth);
                    }
                }
                i += horizontalSpan;
            }
        }
        return columnWidths;
    }

    private int[] GetPreferredRowHeights(IComponent?[][] table)
    {
        int numberOfRows = table.Length;
        int[] rowHeights = new int[numberOfRows];

        // Start by letting all span = 1 rows take what they need
        int rowIndex = 0;
        foreach (var row in table)
        {
            foreach (var component in row)
            {
                if (component == null)
                {
                    continue;
                }
                var layoutData = GetLayoutData(component);
                if (layoutData.VerticalSpan == 1)
                {
                    rowHeights[rowIndex] = Math.Max(rowHeights[rowIndex], component.PreferredSize.Rows);
                }
            }
            rowIndex++;
        }

        // Next, do span > 1 and enlarge if necessary
        for (int x = 0; x < _numberOfColumns; x++)
        {
            for (int y = 0; y < numberOfRows;)
            {
                if (x >= table[y].Length)
                {
                    y++;
                    continue;
                }
                var component = table[y][x];
                if (component == null)
                {
                    y++;
                    continue;
                }
                var layoutData = GetLayoutData(component);
                if (layoutData.VerticalSpan > 1)
                {
                    int accumulatedHeight = 0;
                    for (int i = y; i < y + layoutData.VerticalSpan; i++)
                    {
                        accumulatedHeight += rowHeights[i];
                    }

                    int preferredHeight = component.PreferredSize.Rows;
                    if (preferredHeight > accumulatedHeight)
                    {
                        int rowOffset = 0;
                        do
                        {
                            rowHeights[y + rowOffset++]++;
                            accumulatedHeight++;
                            if (rowOffset == layoutData.VerticalSpan)
                            {
                                rowOffset = 0;
                            }
                        }
                        while (preferredHeight > accumulatedHeight);
                    }
                }
                y += layoutData.VerticalSpan;
            }
        }
        return rowHeights;
    }

    private HashSet<int> GetExpandableColumns(IComponent?[][] table)
    {
        var expandableColumns = new HashSet<int>();
        IComponent? previousComponent = null;
        foreach (var row in table)
        {
            for (int i = 0; i < row.Length; i++)
            {
                if (row[i] == null || row[i] == previousComponent)
                {
                    continue;
                }
                var layoutData = GetLayoutData(row[i]!);
                if (layoutData.GrabExtraHorizontalSpace)
                {
                    expandableColumns.Add(i);
                }
                previousComponent = row[i];
            }
        }
        return expandableColumns;
    }

    private HashSet<int> GetExpandableRows(IComponent?[][] table)
    {
        var expandableRows = new HashSet<int>();
        IComponent? previousComponent = null;
        if (table.Length > 0)
        {
            for (int columnIndex = 0; columnIndex < table[0].Length; columnIndex++)
            {
                for (int rowIndex = 0; rowIndex < table.Length; rowIndex++)
                {
                    var cell = table[rowIndex][columnIndex];
                    if (cell == null || cell == previousComponent)
                    {
                        continue;
                    }
                    var layoutData = GetLayoutData(cell);
                    if (layoutData.GrabExtraVerticalSpace)
                    {
                        expandableRows.Add(rowIndex);
                    }
                    previousComponent = cell;
                }
            }
        }
        return expandableRows;
    }

    private int ShrinkWidthToFitArea(TerminalSize area, int[] columnWidths)
    {
        int totalWidth = 0;
        foreach (int width in columnWidths)
        {
            totalWidth += width;
        }
        if (totalWidth > area.Columns)
        {
            int columnOffset = 0;
            do
            {
                if (columnWidths[columnOffset] > 0)
                {
                    columnWidths[columnOffset]--;
                    totalWidth--;
                }
                if (++columnOffset == columnWidths.Length)
                {
                    columnOffset = 0;
                }
            }
            while (totalWidth > area.Columns);
        }
        return totalWidth;
    }

    private int ShrinkHeightToFitArea(TerminalSize area, int[] rowHeights)
    {
        int totalHeight = 0;
        foreach (int height in rowHeights)
        {
            totalHeight += height;
        }
        if (totalHeight > area.Rows)
        {
            int rowOffset = 0;
            do
            {
                if (rowHeights[rowOffset] > 0)
                {
                    rowHeights[rowOffset]--;
                    totalHeight--;
                }
                if (++rowOffset == rowHeights.Length)
                {
                    rowOffset = 0;
                }
            }
            while (totalHeight > area.Rows);
        }
        return totalHeight;
    }

    private int GrabExtraHorizontalSpace(TerminalSize area, int[] columnWidths, HashSet<int> expandableColumns, int totalWidth)
    {
        foreach (int columnIndex in expandableColumns)
        {
            columnWidths[columnIndex]++;
            totalWidth++;
            if (area.Columns == totalWidth)
            {
                break;
            }
        }
        return totalWidth;
    }

    private int GrabExtraVerticalSpace(TerminalSize area, int[] rowHeights, HashSet<int> expandableRows, int totalHeight)
    {
        foreach (int rowIndex in expandableRows)
        {
            rowHeights[rowIndex]++;
            totalHeight++;
            // Note: This appears to be a bug in the original Java code - should be area.Rows, not area.Columns
            // But preserving the bug for one-to-one compatibility with Java
            if (area.Columns == totalHeight)
            {
                break;
            }
        }
        return totalHeight;
    }

    private IComponent?[][] BuildTable(IReadOnlyList<IComponent> components)
    {
        var rows = new List<IComponent?[]>();
        var hspans = new List<int[]>();
        var vspans = new List<int[]>();

        int rowCount = 0;
        int rowsExtent = 1;
        var toBePlaced = new Queue<IComponent>(components);
        while (toBePlaced.Count > 0 || rowCount < rowsExtent)
        {
            // Start new row
            var row = new IComponent?[_numberOfColumns];
            var hspan = new int[_numberOfColumns];
            var vspan = new int[_numberOfColumns];

            for (int i = 0; i < _numberOfColumns; i++)
            {
                if (i > 0 && hspan[i - 1] > 1)
                {
                    row[i] = row[i - 1];
                    hspan[i] = hspan[i - 1] - 1;
                    vspan[i] = vspan[i - 1];
                }
                else if (rowCount > 0 && vspans[rowCount - 1][i] > 1)
                {
                    row[i] = rows[rowCount - 1][i];
                    hspan[i] = hspans[rowCount - 1][i];
                    vspan[i] = vspans[rowCount - 1][i] - 1;
                }
                else if (toBePlaced.Count > 0)
                {
                    var component = toBePlaced.Dequeue();
                    var gridLayoutData = GetLayoutData(component);

                    row[i] = component;
                    hspan[i] = gridLayoutData.HorizontalSpan;
                    vspan[i] = gridLayoutData.VerticalSpan;
                    rowsExtent = Math.Max(rowsExtent, rowCount + gridLayoutData.VerticalSpan);
                }
                else
                {
                    row[i] = null;
                    hspan[i] = 1;
                    vspan[i] = 1;
                }
            }

            rows.Add(row);
            hspans.Add(hspan);
            vspans.Add(vspan);
            rowCount++;
        }
        return rows.ToArray();
    }

    private IComponent?[][] EliminateUnusedRowsAndColumns(IComponent?[][] table)
    {
        if (table.Length == 0)
        {
            return table;
        }
        // Could make this into a Set, but I doubt there will be any real gain in performance as these are probably going
        // to be very small.
        var rowsToRemove = new List<int>();
        var columnsToRemove = new List<int>();

        int tableRows = table.Length;
        int tableColumns = table[0].Length;

        // Scan for unnecessary columns
        for (int column = tableColumns - 1; column > 0; column--)
        {
            bool foundComponent = false;
            foreach (var row in table)
            {
                if (row[column] != null)
                {
                    foundComponent = true;
                    break;
                }
            }
            if (!foundComponent)
            {
                columnsToRemove.Add(column);
            }
        }

        // Scan for unnecessary rows
        for (int row = tableRows - 1; row > 0; row--)
        {
            bool foundComponent = false;
            for (int column = 0; column < tableColumns; column++)
            {
                if (table[row][column] != null)
                {
                    foundComponent = true;
                    break;
                }
            }
            if (!foundComponent)
            {
                rowsToRemove.Add(row);
            }
        }

        // If there's nothing to remove, just return the same
        if (rowsToRemove.Count == 0 && columnsToRemove.Count == 0)
        {
            return table;
        }

        // Build a new table with rows & columns eliminated
        var newTable = new IComponent?[tableRows - rowsToRemove.Count][];
        int insertedRowCounter = 0;
        for (int rowIndex = 0; rowIndex < tableRows; rowIndex++)
        {
            if (rowsToRemove.Contains(rowIndex))
            {
                continue;
            }
            var row = table[rowIndex];
            var newColumn = new IComponent?[tableColumns - columnsToRemove.Count];
            int insertedColumnCounter = 0;
            for (int column = 0; column < tableColumns; column++)
            {
                if (columnsToRemove.Contains(column))
                {
                    continue;
                }
                newColumn[insertedColumnCounter++] = row[column];
            }
            newTable[insertedRowCounter++] = newColumn;
        }
        return newTable;
    }

    private GridLayoutData GetLayoutData(IComponent component)
    {
        var layoutData = component.LayoutData;
        if (layoutData is GridLayoutData gridLayoutData)
        {
            return gridLayoutData;
        }
        else
        {
            return DEFAULT;
        }
    }
}