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
using System.Reflection;

namespace Lanterna.Terminal.Ansi
{
    internal static class TelnetProtocol
    {
        public const byte COMMAND_SUBNEGOTIATION_END = 0xf0;      // SE
        public const byte COMMAND_NO_OPERATION = 0xf1;           // NOP
        public const byte COMMAND_DATA_MARK = 0xf2;              // DM
        public const byte COMMAND_BREAK = 0xf3;                  // BRK
        public const byte COMMAND_INTERRUPT_PROCESS = 0xf4;      // IP
        public const byte COMMAND_ABORT_OUTPUT = 0xf5;           // AO
        public const byte COMMAND_ARE_YOU_THERE = 0xf6;          // AYT
        public const byte COMMAND_ERASE_CHARACTER = 0xf7;        // EC
        public const byte COMMAND_ERASE_LINE = 0xf8;             // WL
        public const byte COMMAND_GO_AHEAD = 0xf9;               // GA
        public const byte COMMAND_SUBNEGOTIATION = 0xfa;         // SB
        public const byte COMMAND_WILL = 0xfb;
        public const byte COMMAND_WONT = 0xfc;
        public const byte COMMAND_DO = 0xfd;
        public const byte COMMAND_DONT = 0xfe;
        public const byte COMMAND_IAC = 0xff;

        public const byte OPTION_TRANSMIT_BINARY = 0x00;
        public const byte OPTION_ECHO = 0x01;
        public const byte OPTION_SUPPRESS_GO_AHEAD = 0x03;
        public const byte OPTION_STATUS = 0x05;
        public const byte OPTION_TIMING_MARK = 0x06;
        public const byte OPTION_NAOCRD = 0x0a;
        public const byte OPTION_NAOHTS = 0x0b;
        public const byte OPTION_NAOHTD = 0x0c;
        public const byte OPTION_NAOFFD = 0x0d;
        public const byte OPTION_NAOVTS = 0x0e;
        public const byte OPTION_NAOVTD = 0x0f;
        public const byte OPTION_NAOLFD = 0x10;
        public const byte OPTION_EXTEND_ASCII = 0x01;
        public const byte OPTION_TERMINAL_TYPE = 0x18;
        public const byte OPTION_NAWS = 0x1f;
        public const byte OPTION_TERMINAL_SPEED = 0x20;
        public const byte OPTION_TOGGLE_FLOW_CONTROL = 0x21;
        public const byte OPTION_LINEMODE = 0x22;
        public const byte OPTION_AUTHENTICATION = 0x25;

        public static readonly Dictionary<string, byte> NAME_TO_CODE = CreateName2CodeMap();
        public static readonly Dictionary<byte, string> CODE_TO_NAME = ReverseMap(NAME_TO_CODE);

        private static Dictionary<string, byte> CreateName2CodeMap()
        {
            var result = new Dictionary<string, byte>();
            var fields = typeof(TelnetProtocol).GetFields(BindingFlags.Public | BindingFlags.Static);
            
            foreach (var field in fields)
            {
                if (field.FieldType != typeof(byte) || (!field.Name.StartsWith("COMMAND_") && !field.Name.StartsWith("OPTION_")))
                {
                    continue;
                }
                
                try
                {
                    string namePart = field.Name.Substring(field.Name.IndexOf("_") + 1);
                    result[namePart] = (byte)(field.GetValue(null) ?? 0);
                }
                catch (Exception)
                {
                    // Ignore exceptions
                }
            }
            
            return result;
        }

        private static Dictionary<V, K> ReverseMap<K, V>(Dictionary<K, V> original) where K : notnull where V : notnull
        {
            var result = new Dictionary<V, K>();
            foreach (var entry in original)
            {
                result[entry.Value] = entry.Key;
            }
            return result;
        }
    }
}