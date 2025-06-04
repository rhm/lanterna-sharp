using System.Collections.Generic;

namespace Lanterna.Terminal.Ansi
{
    /// <summary>
    /// Constants for the Telnet protocol. Only a subset is provided.
    /// </summary>
    internal static class TelnetProtocol
    {
        public const byte COMMAND_SUBNEGOTIATION_END = 0xf0;
        public const byte COMMAND_NO_OPERATION = 0xf1;
        public const byte COMMAND_DATA_MARK = 0xf2;
        public const byte COMMAND_BREAK = 0xf3;
        public const byte COMMAND_INTERRUPT_PROCESS = 0xf4;
        public const byte COMMAND_ABORT_OUTPUT = 0xf5;
        public const byte COMMAND_ARE_YOU_THERE = 0xf6;
        public const byte COMMAND_ERASE_CHARACTER = 0xf7;
        public const byte COMMAND_ERASE_LINE = 0xf8;
        public const byte COMMAND_GO_AHEAD = 0xf9;
        public const byte COMMAND_SUBNEGOTIATION = 0xfa;
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

        public static readonly IReadOnlyDictionary<string, byte> NAME_TO_CODE;
        public static readonly IReadOnlyDictionary<byte, string> CODE_TO_NAME;

        static TelnetProtocol()
        {
            var name2code = new Dictionary<string, byte>();
            foreach (var field in typeof(TelnetProtocol).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
            {
                if (field.FieldType != typeof(byte))
                    continue;
                var name = field.Name;
                if (name.StartsWith("COMMAND_") || name.StartsWith("OPTION_"))
                {
                    name2code[name.Substring(name.IndexOf('_') + 1)] = (byte)field.GetValue(null);
                }
            }
            NAME_TO_CODE = name2code;
            var rev = new Dictionary<byte, string>();
            foreach (var kv in name2code)
                rev[kv.Value] = kv.Key;
            CODE_TO_NAME = rev;
        }
    }
}
