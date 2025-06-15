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

namespace Lanterna.Input
{
    /// <summary>
    /// Represents a mouse action that is received from the terminal input
    /// </summary>
    public class MouseAction : KeyStroke
    {
        /// <summary>
        /// The type of mouse action
        /// </summary>
        public MouseActionType ActionType { get; }

        /// <summary>
        /// The mouse button involved in this action (0 for no button)
        /// </summary>
        public int Button { get; }

        /// <summary>
        /// The position where the mouse action occurred
        /// </summary>
        public TerminalPosition Position { get; }

        /// <summary>
        /// Creates a new MouseAction with basic information
        /// </summary>
        /// <param name="actionType">Type of mouse action</param>
        /// <param name="button">Mouse button number</param>
        /// <param name="position">Position where action occurred</param>
        public MouseAction(MouseActionType actionType, int button, TerminalPosition position)
            : base(KeyType.MouseEvent, false, false, false)
        {
            ActionType = actionType;
            Button = button;
            Position = position;
        }

        /// <summary>
        /// Creates a new MouseAction with modifier key information
        /// </summary>
        /// <param name="actionType">Type of mouse action</param>
        /// <param name="button">Mouse button number</param>
        /// <param name="position">Position where action occurred</param>
        /// <param name="ctrlDown">Was ctrl pressed</param>
        /// <param name="altDown">Was alt pressed</param>
        /// <param name="shiftDown">Was shift pressed</param>
        public MouseAction(MouseActionType actionType, int button, TerminalPosition position, bool ctrlDown, bool altDown, bool shiftDown)
            : base(KeyType.MouseEvent, ctrlDown, altDown, shiftDown)
        {
            ActionType = actionType;
            Button = button;
            Position = position;
        }

        public override string ToString()
        {
            return $"MouseAction{{actionType={ActionType}, button={Button}, position={Position}}}";
        }
    }
}