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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Lanterna.Core;

namespace Lanterna.Input
{
    /// <summary>
    /// Pattern used to detect Xterm-protocol mouse events coming in on the standard input channel
    /// </summary>
    public class MouseCharacterPattern : ICharacterPattern
    {
        private static readonly char[] Header = { IKeyDecodingProfile.EscCode, '[', '<' };
        private static readonly Regex Pattern = new Regex(@".*\<([0-9]+);([0-9]+);([0-9]+)([mM])");

        // Some terminals, for example XTerm, issue mouse down when it
        // should be mouse move, after first click then they correctly issues
        // mouse move, do some coercion here to force the correct action
        private bool isMouseDown = false;

        public Matching? Match(IList<char> seq)
        {
            int size = seq.Count;
            if (size > 15)
            {
                return null; // nope
            }

            // Check first 3 chars:
            for (int i = 0; i < 3; i++)
            {
                if (i >= (size - 1))
                {
                    return Matching.NotYet; // maybe later
                }
                if (seq[i] != Header[i])
                {
                    return null; // nope
                }
            }

            // Check if we have a number on the next position
            if (seq[3] < '0' || seq[3] > '9')
            {
                return null; // nope
            }

            // If the size is lower than 7 then we don't have the pattern yet for sure
            if (size < 7)
            {
                return Matching.NotYet; // maybe later
            }

            // Converts the list of characters to a string
            string seqAsString = new string(seq.ToArray());

            // Check if we match the regex
            Match match = Pattern.Match(seqAsString);
            if (match.Success)
            {
                bool shiftDown = false;
                bool altDown = false;
                bool ctrlDown = false;

                // Get the button
                int item = int.Parse(match.Groups[1].Value);
                int button = 0;

                // If the 6th bit is set, then it's a wheel event then we check the 1st bit to know if it's up or down
                if ((item & 0x40) != 0)
                {
                    if ((item & 0x1) == 0)
                    {
                        button = 4;
                    }
                    else
                    {
                        button = 5;
                    }
                }
                else if ((item & 0x2) != 0)
                {
                    button = 3;
                }
                else if ((item & 0x1) != 0)
                {
                    button = 1;
                }
                else if ((item & 0x1) == 0)
                {
                    button = 2;
                }

                // Get the modifier keys (it seems that they do not are always reported correctly depending on the terminal)
                if ((item & 0x4) != 0)
                {
                    shiftDown = true;
                }
                if ((item & 0x8) != 0)
                {
                    altDown = true;
                }
                if ((item & 0x10) != 0)
                {
                    ctrlDown = true;
                }

                // Get the action
                MouseActionType actionType;
                if (match.Groups[4].Value.Equals("M"))
                {
                    actionType = MouseActionType.CLICK_DOWN;
                }
                else
                {
                    actionType = MouseActionType.CLICK_RELEASE;
                }

                // Get the move and drag actions
                if ((item & 0x20) != 0)
                {
                    if ((item & 0x3) != 0)
                    {
                        // In move mode, the bits 0, 1 are set in addition to the 6th bit
                        actionType = MouseActionType.MOVE;
                        button = 0;
                    }
                    else
                    {
                        actionType = MouseActionType.DRAG;
                    }
                }
                else
                {
                    isMouseDown = (actionType == MouseActionType.CLICK_DOWN);
                }

                // Coerce action types:
                // when in between CLICK_DOWN and CLICK_RELEASE coerce MOVE to DRAG
                // when not between CLICK_DOWN and CLICK_RELEASE coerce DRAG to MOVE
                if (isMouseDown)
                {
                    if (actionType == MouseActionType.MOVE)
                    {
                        actionType = MouseActionType.DRAG;
                    }
                }
                else if (actionType == MouseActionType.DRAG)
                {
                    actionType = MouseActionType.MOVE;
                }

                // Get the position
                var pos = new TerminalPosition(int.Parse(match.Groups[2].Value) - 1, int.Parse(match.Groups[3].Value) - 1);

                var ma = new MouseAction(actionType, button, pos, ctrlDown, altDown, shiftDown);
                return new Matching(ma); // yep
            }
            else
            {
                return Matching.NotYet; // maybe later
            }
        }
    }
}