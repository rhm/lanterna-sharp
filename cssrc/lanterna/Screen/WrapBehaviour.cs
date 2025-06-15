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
namespace Lanterna.Screen
{
    /// <summary>
    /// What to do when line length is exceeded.
    /// </summary>
    public enum WrapBehaviour
    {
        /// <summary>
        /// Never ever leave current line.
        /// </summary>
        SINGLE_LINE,
        /// <summary>
        /// Don't wrap lines automatically, but honor explicit line-feeds.
        /// </summary>
        CLIP,
        /// <summary>
        /// Wrap at any character boundaries.
        /// </summary>
        CHAR,
        /// <summary>
        /// Only wrap at word boundaries. If a single word exceeds line 
        /// length, it will still be broken to line length.
        /// </summary>
        WORD
    }

    /// <summary>
    /// Extension methods for WrapBehaviour enum to provide Java-like behavior properties.
    /// </summary>
    public static class WrapBehaviourExtensions
    {
        /// <summary>
        /// Gets whether this wrap behavior allows line feeds.
        /// </summary>
        public static bool AllowLineFeed(this WrapBehaviour wrapBehaviour)
        {
            return wrapBehaviour switch
            {
                WrapBehaviour.SINGLE_LINE => false,
                WrapBehaviour.CLIP => true,
                WrapBehaviour.CHAR => true,
                WrapBehaviour.WORD => true,
                _ => false
            };
        }

        /// <summary>
        /// Gets whether this wrap behavior allows automatic wrapping.
        /// </summary>
        public static bool AutoWrap(this WrapBehaviour wrapBehaviour)
        {
            return wrapBehaviour switch
            {
                WrapBehaviour.SINGLE_LINE => false,
                WrapBehaviour.CLIP => false,
                WrapBehaviour.CHAR => true,
                WrapBehaviour.WORD => true,
                _ => false
            };
        }

        /// <summary>
        /// Gets whether this wrap behavior keeps words together.
        /// </summary>
        public static bool KeepWords(this WrapBehaviour wrapBehaviour)
        {
            return wrapBehaviour switch
            {
                WrapBehaviour.SINGLE_LINE => false,
                WrapBehaviour.CLIP => false,
                WrapBehaviour.CHAR => false,
                WrapBehaviour.WORD => true,
                _ => false
            };
        }
    }
}