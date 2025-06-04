using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Lanterna.Terminal.Ansi
{
    /// <summary>
    /// Cygwin specific terminal implementation using stty.
    /// </summary>
    public class CygwinTerminal : UnixLikeTTYTerminal
    {
        private static readonly string STTY_LOCATION = FindProgram("stty.exe");
        private static readonly Regex STTY_SIZE_PATTERN = new Regex(@".*rows ([0-9]+);.*columns ([0-9]+);.*");
        private const string CYGWIN_HOME_ENV = "CYGWIN_HOME";

        public CygwinTerminal(Stream input, Stream output, Encoding encoding)
            : base(null, input, output, encoding, CtrlCBehaviour.TRAP)
        {
        }

        protected override TerminalSize FindTerminalSize()
        {
            try
            {
                string stty = RunSTTYCommand("-a");
                var m = STTY_SIZE_PATTERN.Match(stty);
                if (m.Success)
                    return new TerminalSize(int.Parse(m.Groups[2].Value), int.Parse(m.Groups[1].Value));
            }
            catch
            {
            }
            return new TerminalSize(80, 24);
        }

        protected override string RunSTTYCommand(params string[] parameters)
        {
            var cmd = new List<string> { FindSTTY(), "-F", GetPseudoTerminalDevice() };
            cmd.AddRange(parameters);
            return Exec(cmd.ToArray());
        }

        protected override void Acquire()
        {
            base.Acquire();
        }

        private string FindSTTY() => STTY_LOCATION;

        private string GetPseudoTerminalDevice() => "/dev/pty0";

        private static string FindProgram(string programName)
        {
            var cygHome = Environment.GetEnvironmentVariable(CYGWIN_HOME_ENV);
            if (!string.IsNullOrEmpty(cygHome))
            {
                var candidate = Path.Combine(cygHome, "bin", programName);
                if (File.Exists(candidate))
                    return candidate;
            }

            var paths = (Environment.GetEnvironmentVariable("PATH") ?? string.Empty).Split(Path.PathSeparator);
            foreach (var p in paths)
            {
                var candidate = Path.Combine(p, programName);
                if (File.Exists(candidate))
                    return candidate;
            }
            return programName;
        }
    }
}
