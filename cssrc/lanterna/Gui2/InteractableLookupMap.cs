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
using Lanterna.Core;

namespace Lanterna.Gui2;

/// <summary>
/// This class is used to maintain a spatial lookup table of Interactable objects for efficient navigation
/// using keyboard arrow keys. It builds a 2D grid map and uses sophisticated search algorithms to find
/// the most appropriate next component in each direction.
/// </summary>
public class InteractableLookupMap : IInteractableLookupMap
{
    private readonly TerminalSize _size;
    private readonly int[,] _lookupMap;
    private readonly List<IInteractable> _interactables;

    /// <summary>
    /// Creates a new InteractableLookupMap for the specified size
    /// </summary>
    /// <param name="size">Size of the area to map</param>
    public InteractableLookupMap(TerminalSize size)
    {
        _size = size;
        _lookupMap = new int[size.Rows, size.Columns];
        _interactables = new List<IInteractable>();
        
        // Initialize lookup map with -1 (empty)
        for (int row = 0; row < size.Rows; row++)
        {
            for (int col = 0; col < size.Columns; col++)
            {
                _lookupMap[row, col] = -1;
            }
        }
    }

    public void Add(IInteractable interactable, TerminalPosition globalPosition)
    {
        Add(interactable, globalPosition, interactable.Size);
    }

    /// <summary>
    /// Adds an interactable component to the lookup map at the specified position and size
    /// </summary>
    /// <param name="interactable">Component to add</param>
    /// <param name="globalPosition">Global position of the component</param>
    /// <param name="size">Size of the component</param>
    public void Add(IInteractable interactable, TerminalPosition globalPosition, TerminalSize size)
    {
        int interactableIndex = _interactables.Count;
        _interactables.Add(interactable);

        // Fill the lookup map with this interactable's index
        for (int row = Math.Max(0, globalPosition.Row);
             row < Math.Min(_size.Rows, globalPosition.Row + size.Rows);
             row++)
        {
            for (int col = Math.Max(0, globalPosition.Column);
                 col < Math.Min(_size.Columns, globalPosition.Column + size.Columns);
                 col++)
            {
                _lookupMap[row, col] = interactableIndex;
            }
        }
    }

    public IInteractable? FindClosest(TerminalPosition position, IInteractable.FocusChangeDirection direction)
    {
        return direction switch
        {
            IInteractable.FocusChangeDirection.Up => FindNextUp(position),
            IInteractable.FocusChangeDirection.Down => FindNextDown(position),
            IInteractable.FocusChangeDirection.Left => FindNextLeft(position),
            IInteractable.FocusChangeDirection.Right => FindNextRight(position),
            IInteractable.FocusChangeDirection.Next => FindNext(position, true),
            IInteractable.FocusChangeDirection.Previous => FindNext(position, false),
            _ => null
        };
    }

    /// <summary>
    /// Finds the next interactable component above the specified position
    /// </summary>
    private IInteractable? FindNextUp(TerminalPosition position)
    {
        var disqualifiedInteractables = GetDisqualifiedInteractables(position.Row, position.Row, position.Column, position.Column);
        return FindNextInDirection(position, -1, 0, disqualifiedInteractables);
    }

    /// <summary>
    /// Finds the next interactable component below the specified position
    /// </summary>
    private IInteractable? FindNextDown(TerminalPosition position)
    {
        var disqualifiedInteractables = GetDisqualifiedInteractables(position.Row, position.Row, position.Column, position.Column);
        return FindNextInDirection(position, 1, 0, disqualifiedInteractables);
    }

    /// <summary>
    /// Finds the next interactable component to the left of the specified position
    /// </summary>
    private IInteractable? FindNextLeft(TerminalPosition position)
    {
        var disqualifiedInteractables = GetDisqualifiedInteractables(position.Row, position.Row, position.Column, position.Column);
        return FindNextInDirection(position, 0, -1, disqualifiedInteractables);
    }

    /// <summary>
    /// Finds the next interactable component to the right of the specified position
    /// </summary>
    private IInteractable? FindNextRight(TerminalPosition position)
    {
        var disqualifiedInteractables = GetDisqualifiedInteractables(position.Row, position.Row, position.Column, position.Column);
        return FindNextInDirection(position, 0, 1, disqualifiedInteractables);
    }

    /// <summary>
    /// Finds the next or previous interactable using linear ordering
    /// </summary>
    private IInteractable? FindNext(TerminalPosition position, bool forward)
    {
        if (_interactables.Count == 0)
            return null;

        // Find current interactable at position
        int currentIndex = GetInteractableIndexAt(position);
        if (currentIndex == -1)
        {
            // Not on an interactable, find closest one
            return forward ? _interactables.FirstOrDefault() : _interactables.LastOrDefault();
        }

        if (forward)
        {
            return currentIndex + 1 < _interactables.Count ? _interactables[currentIndex + 1] : _interactables[0];
        }
        else
        {
            return currentIndex - 1 >= 0 ? _interactables[currentIndex - 1] : _interactables[_interactables.Count - 1];
        }
    }

    /// <summary>
    /// Core directional search algorithm that uses expanding search patterns
    /// </summary>
    private IInteractable? FindNextInDirection(TerminalPosition position, int rowDelta, int colDelta, HashSet<int> disqualified)
    {
        // Search with expanding distance
        for (int distance = 1; distance < Math.Max(_size.Rows, _size.Columns); distance++)
        {
            // Primary search line
            var result = SearchLine(position, rowDelta, colDelta, distance, disqualified);
            if (result != null)
                return result;

            // If moving vertically, also check horizontal positions at this distance
            if (rowDelta != 0)
            {
                for (int horizontalShift = 1; horizontalShift <= distance; horizontalShift++)
                {
                    // Check both sides
                    result = SearchLine(position.WithRelative(0, horizontalShift), rowDelta, colDelta, distance, disqualified);
                    if (result != null)
                        return result;

                    result = SearchLine(position.WithRelative(0, -horizontalShift), rowDelta, colDelta, distance, disqualified);
                    if (result != null)
                        return result;
                }
            }
            // If moving horizontally, also check vertical positions at this distance
            else if (colDelta != 0)
            {
                for (int verticalShift = 1; verticalShift <= distance; verticalShift++)
                {
                    // Check both sides
                    result = SearchLine(position.WithRelative(verticalShift, 0), rowDelta, colDelta, distance, disqualified);
                    if (result != null)
                        return result;

                    result = SearchLine(position.WithRelative(-verticalShift, 0), rowDelta, colDelta, distance, disqualified);
                    if (result != null)
                        return result;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Searches along a line from the starting position
    /// </summary>
    private IInteractable? SearchLine(TerminalPosition start, int rowDelta, int colDelta, int distance, HashSet<int> disqualified)
    {
        var searchPos = start.WithRelative(rowDelta * distance, colDelta * distance);
        if (!IsValidPosition(searchPos))
            return null;

        int interactableIndex = GetInteractableIndexAt(searchPos);
        if (interactableIndex != -1 && !disqualified.Contains(interactableIndex))
        {
            return _interactables[interactableIndex];
        }

        return null;
    }

    /// <summary>
    /// Gets the set of interactables that should be disqualified from navigation
    /// (typically those on the same row/column as the current position)
    /// </summary>
    private HashSet<int> GetDisqualifiedInteractables(int minRow, int maxRow, int minCol, int maxCol)
    {
        var disqualified = new HashSet<int>();
        
        for (int row = Math.Max(0, minRow); row <= Math.Min(_size.Rows - 1, maxRow); row++)
        {
            for (int col = Math.Max(0, minCol); col <= Math.Min(_size.Columns - 1, maxCol); col++)
            {
                int interactableIndex = _lookupMap[row, col];
                if (interactableIndex != -1)
                {
                    disqualified.Add(interactableIndex);
                }
            }
        }

        return disqualified;
    }

    /// <summary>
    /// Gets the interactable index at the specified position
    /// </summary>
    private int GetInteractableIndexAt(TerminalPosition position)
    {
        if (!IsValidPosition(position))
            return -1;

        return _lookupMap[position.Row, position.Column];
    }

    /// <summary>
    /// Checks if the position is within the valid bounds of the lookup map
    /// </summary>
    private bool IsValidPosition(TerminalPosition position)
    {
        return position.Row >= 0 && position.Row < _size.Rows &&
               position.Column >= 0 && position.Column < _size.Columns;
    }

    /// <summary>
    /// Gets the interactable component at the specified position, or null if none exists
    /// </summary>
    public IInteractable? GetInteractableAt(TerminalPosition position)
    {
        int index = GetInteractableIndexAt(position);
        return index != -1 ? _interactables[index] : null;
    }

    /// <summary>
    /// Gets all interactable components registered in this lookup map
    /// </summary>
    public IReadOnlyList<IInteractable> GetInteractables()
    {
        return _interactables.AsReadOnly();
    }

    /// <summary>
    /// Clears all registered interactables from the lookup map
    /// </summary>
    public void Clear()
    {
        _interactables.Clear();
        for (int row = 0; row < _size.Rows; row++)
        {
            for (int col = 0; col < _size.Columns; col++)
            {
                _lookupMap[row, col] = -1;
            }
        }
    }
}