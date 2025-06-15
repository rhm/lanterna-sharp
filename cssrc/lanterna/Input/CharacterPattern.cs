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
using System.Collections.Generic;

namespace Lanterna.Input
{
    /// <summary>
    /// Used to compare a list of character if they match a particular pattern, and in that case, return the kind of 
    /// keystroke this pattern represents
    /// </summary>
    public interface ICharacterPattern
    {
        /// <summary>
        /// Given a list of characters, determine whether it exactly matches
        /// any known KeyStroke, and whether a longer sequence can possibly match.
        /// </summary>
        /// <param name="seq">of characters to check</param>
        /// <returns>see <see cref="Matching"/></returns>
        Matching? Match(IList<char> seq);
    }

    /// <summary>
    /// This immutable class describes a matching result. It wraps two items,
    /// partialMatch and fullMatch.
    /// <list type="bullet">
    /// <item><term>fullMatch</term><description>
    ///   The resulting KeyStroke if the pattern matched, otherwise null.<br/>
    ///     Example: if the tested sequence is <c>Esc [ A</c>, and if the
    ///      pattern recognized this as <c>ArrowUp</c>, then this field has
    ///      a value like <c>new KeyStroke(KeyType.ArrowUp)</c></description></item>
    /// <item><term>partialMatch</term><description>
    ///   <c>true</c>, if appending appropriate characters at the end of the 
    ///      sequence <i>can</i> produce a match.<br/>
    ///     Example: if the tested sequence is "Esc [", and the Pattern would match
    ///      "Esc [ A", then this field would be set to <c>true</c>.</description></item>
    /// </list>
    /// In principle, a sequence can match one KeyStroke, but also say that if 
    /// another character is available, then a different KeyStroke might result.
    /// This can happen, if (e.g.) a single CharacterPattern-instance matches
    /// both the Escape key and a longer Escape-sequence.
    /// </summary>
    public class Matching
    {
        public KeyStroke? FullMatch { get; }
        public bool PartialMatch { get; }
        
        /// <summary>
        /// Re-usable result for "not yet" half-matches
        /// </summary>
        public static readonly Matching NotYet = new Matching(true, null);

        /// <summary>
        /// Convenience constructor for exact matches
        /// </summary>
        /// <param name="fullMatch">the KeyStroke that matched the sequence</param>
        public Matching(KeyStroke fullMatch) : this(false, fullMatch)
        {
        }

        /// <summary>
        /// General constructor
        /// For mismatches rather use <c>null</c> and for "not yet" matches use NotYet.
        /// Use this constructor, where a sequence may yield both fullMatch and
        /// partialMatch or for merging result Matchings of multiple patterns.
        /// </summary>
        /// <param name="partialMatch">true if further characters could lead to a match</param>
        /// <param name="fullMatch">The perfectly matching KeyStroke</param>
        public Matching(bool partialMatch, KeyStroke? fullMatch)
        {
            this.PartialMatch = partialMatch;
            this.FullMatch = fullMatch;
        }

        public override string ToString()
        {
            return $"Matching{{partialMatch={PartialMatch}, fullMatch={FullMatch}}}";
        }
    }
}