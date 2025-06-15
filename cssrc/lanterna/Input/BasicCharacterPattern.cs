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

namespace Lanterna.Input
{
    /// <summary>
    /// Very simple pattern that matches the input stream against a pre-defined list of characters. For the pattern to match,
    /// the list of characters must match exactly what's coming in on the input stream.
    /// </summary>
    public class BasicCharacterPattern : ICharacterPattern
    {
        private readonly KeyStroke result;
        private readonly char[] pattern;

        /// <summary>
        /// Creates a new BasicCharacterPattern that matches a particular sequence of characters into a <see cref="KeyStroke"/>
        /// </summary>
        /// <param name="result"><see cref="KeyStroke"/> that this pattern will translate to</param>
        /// <param name="pattern">Sequence of characters that translates into the <see cref="KeyStroke"/></param>
        public BasicCharacterPattern(KeyStroke result, params char[] pattern)
        {
            this.result = result;
            this.pattern = pattern;
        }

        /// <summary>
        /// Returns the characters that makes up this pattern, as an array that is a copy of the array used internally
        /// </summary>
        /// <returns>Array of characters that defines this pattern</returns>
        public char[] GetPattern()
        {
            return (char[])pattern.Clone();
        }

        /// <summary>
        /// Returns the keystroke that this pattern results in
        /// </summary>
        /// <returns>The keystroke this pattern will return if it matches</returns>
        public KeyStroke GetResult()
        {
            return result;
        }

        public Matching? Match(IList<char> seq)
        {
            int size = seq.Count;
            
            if (size > pattern.Length)
            {
                return null; // nope
            }
            for (int i = 0; i < size; i++)
            {
                if (pattern[i] != seq[i])
                {
                    return null; // nope
                }
            }
            if (size == pattern.Length)
            {
                return new Matching(GetResult()); // yep
            }
            else
            {
                return Matching.NotYet; // maybe later
            }
        }

        public override bool Equals(object? obj)
        {
            if (!(obj is BasicCharacterPattern other))
            {
                return false;
            }

            return pattern.SequenceEqual(other.pattern);
        }

        public override int GetHashCode()
        {
            int hash = 3;
            foreach (char c in pattern)
            {
                hash = 53 * hash + c.GetHashCode();
            }
            return hash;
        }
    }
}