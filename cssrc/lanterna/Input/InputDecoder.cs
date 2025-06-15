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
using System.IO;
using System.Linq;
using System.Threading;

namespace Lanterna.Input
{
    /// <summary>
    /// Used to read the input stream character by character and generate <see cref="KeyStroke"/> objects to be put in the input queue.
    /// </summary>
    public class InputDecoder
    {
        private readonly TextReader source;
        private readonly List<ICharacterPattern> bytePatterns;
        private readonly List<char> currentMatching;
        private bool seenEOF;
        private int timeoutUnits;
        private readonly object patternsLock = new object();

        /// <summary>
        /// Creates a new input decoder using a specified TextReader as the source to read characters from
        /// </summary>
        /// <param name="source">TextReader to read characters from</param>
        public InputDecoder(TextReader source)
        {
            this.source = source;
            this.bytePatterns = new List<ICharacterPattern>();
            this.currentMatching = new List<char>();
            this.seenEOF = false;
            this.timeoutUnits = 0; // default is no wait at all
        }

        /// <summary>
        /// Adds another key decoding profile to this InputDecoder, which means all patterns from the profile will be used
        /// when decoding input.
        /// </summary>
        /// <param name="profile">Profile to add</param>
        public void AddProfile(IKeyDecodingProfile profile)
        {
            foreach (var pattern in profile.GetPatterns())
            {
                lock (patternsLock)
                {
                    //If an equivalent pattern already exists, remove it first
                    bytePatterns.Remove(pattern);
                    bytePatterns.Add(pattern);
                }
            }
        }

        /// <summary>
        /// Returns a collection of all patterns registered in this InputDecoder.
        /// </summary>
        /// <returns>Collection of patterns in the InputDecoder</returns>
        public ICollection<ICharacterPattern> GetPatterns()
        {
            lock (patternsLock)
            {
                return new List<ICharacterPattern>(bytePatterns);
            }
        }

        /// <summary>
        /// Removes one pattern from the list of patterns in this InputDecoder
        /// </summary>
        /// <param name="pattern">Pattern to remove</param>
        /// <returns><c>true</c> if the supplied pattern was found and was removed, otherwise <c>false</c></returns>
        public bool RemovePattern(ICharacterPattern pattern)
        {
            lock (patternsLock)
            {
                return bytePatterns.Remove(pattern);
            }
        }

        /// <summary>
        /// Sets the number of 1/4-second units for how long to try to get further input
        /// to complete an escape-sequence for a special Key.
        /// 
        /// Negative numbers are mapped to 0 (no wait at all), and unreasonably high
        /// values are mapped to a maximum of 240 (1 minute).
        /// </summary>
        /// <param name="units">New timeout to use, in 250ms units</param>
        public void SetTimeoutUnits(int units)
        {
            timeoutUnits = units < 0 ? 0 : 
                          units > 240 ? 240 : 
                          units;
        }

        /// <summary>
        /// Queries the current timeoutUnits value. One unit is 1/4 second.
        /// </summary>
        /// <returns>The timeout this InputDecoder will use when waiting for additional input, in units of 1/4 seconds</returns>
        public int GetTimeoutUnits()
        {
            return timeoutUnits;
        }

        /// <summary>
        /// Reads and decodes the next key stroke from the input stream
        /// </summary>
        /// <param name="blockingIO">If set to <c>true</c>, the call will not return until it has read at least one <see cref="KeyStroke"/></param>
        /// <returns>Key stroke read from the input stream, or <c>null</c> if none</returns>
        /// <exception cref="IOException">If there was an I/O error when reading from the input stream</exception>
        public KeyStroke? GetNextCharacter(bool blockingIO)
        {
            KeyStroke? bestMatch = null;
            int bestLen = 0;
            int curLen = 0;

            while (true)
            {
                if (curLen < currentMatching.Count)
                {
                    // (re-)consume characters previously read:
                    curLen++;
                }
                else
                {
                    // If we already have a bestMatch but a chance for a longer match
                    //   then we poll for the configured number of timeout units:
                    if (bestMatch != null)
                    {
                        int timeout = GetTimeoutUnits();
                        while (timeout > 0 && !IsInputReady())
                        {
                            try
                            {
                                timeout--;
                                Thread.Sleep(250);
                            }
                            catch (ThreadInterruptedException)
                            {
                                timeout = 0;
                            }
                        }
                    }

                    // if input is available, we can just read a char without waiting,
                    // otherwise, for readInput() with no bestMatch found yet,
                    //  we have to wait blocking for more input:
                    if (IsInputReady() || (blockingIO && bestMatch == null))
                    {
                        int readChar = source.Read();
                        if (readChar == -1)
                        {
                            seenEOF = true;
                            if (currentMatching.Count == 0)
                            {
                                return new KeyStroke(KeyType.EOF);
                            }
                            break;
                        }
                        currentMatching.Add((char)readChar);
                        curLen++;
                    }
                    else
                    { // no more available input at this time.
                        // already found something:
                        if (bestMatch != null)
                        {
                            break; // it's something...
                        }
                        // otherwise: no KeyStroke yet
                        return null;
                    }
                }

                var curSub = currentMatching.Take(curLen).ToList();
                var matching = GetBestMatch(curSub);

                // fullMatch found...
                if (matching.FullMatch != null)
                {
                    bestMatch = matching.FullMatch;
                    bestLen = curLen;

                    if (!matching.PartialMatch)
                    {
                        // that match and no more
                        break;
                    }
                    else
                    {
                        // that match, but maybe more
                        continue;
                    }
                }
                // No match found yet, but there's still potential...
                else if (matching.PartialMatch)
                {
                    continue;
                }
                // no longer match possible at this point:
                else
                {
                    if (bestMatch != null)
                    {
                        // there was already a previous full-match, use it:
                        break;
                    }
                    else
                    { // invalid input!
                        // remove the whole fail and re-try finding a KeyStroke...
                        curSub.Clear(); // or just 1 char?  currentMatching.remove(0);
                        curLen = 0;
                        continue;
                    }
                }
            }

            //Did we find anything? Otherwise return null
            if (bestMatch == null)
            {
                if (seenEOF)
                {
                    currentMatching.Clear();
                    return new KeyStroke(KeyType.EOF);
                }
                return null;
            }

            // Remove matched input from buffer
            for (int i = 0; i < bestLen; i++)
            {
                currentMatching.RemoveAt(0);
            }
            return bestMatch;
        }

        private Matching GetBestMatch(IList<char> characterSequence)
        {
            bool partialMatch = false;
            KeyStroke? bestMatch = null;
            lock (patternsLock)
            {
                foreach (var pattern in bytePatterns)
                {
                    var res = pattern.Match(characterSequence);
                    if (res != null)
                    {
                        if (res.PartialMatch) { partialMatch = true; }
                        if (res.FullMatch != null) { bestMatch = res.FullMatch; }
                    }
                }
            }
            return new Matching(partialMatch, bestMatch);
        }

        private bool IsInputReady()
        {
            // In .NET, TextReader doesn't have a ready() method like Java's BufferedReader
            // This is a simplified implementation - for stream-based readers, 
            // we could check if the underlying stream has data available
            if (source is StreamReader sr && sr.BaseStream.CanSeek)
            {
                return sr.BaseStream.Position < sr.BaseStream.Length;
            }
            
            // For console input or other readers, assume ready
            return true;
        }
    }
}