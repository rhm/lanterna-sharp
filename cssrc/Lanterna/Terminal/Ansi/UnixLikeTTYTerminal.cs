using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Lanterna.Terminal.Ansi
{
    /// <summary>
    /// Base class for terminals using a TTY device on Unix-like systems.
    /// </summary>
    public abstract class UnixLikeTTYTerminal : UnixLikeTerminal
    {
        private readonly FileInfo _ttyDev;

        protected UnixLikeTTYTerminal(string ttyDev, Stream input, Stream output, Encoding encoding, CtrlCBehaviour behaviour)
            : base(input, output, encoding, behaviour)
        {
            _ttyDev = ttyDev != null ? new FileInfo(ttyDev) : null;
        }

        protected override string RunSTTYCommand(params string[] parameters)
        {
            var cmd = new List<string>(GetSTTYCommand());
            if (_ttyDev != null)
            {
                cmd.Add("-F");
                cmd.Add(_ttyDev.FullName);
            }
            cmd.AddRange(parameters);
            return Exec(cmd.ToArray());
        }

        protected string Exec(params string[] cmd)
        {
            var psi = new ProcessStartInfo
            {
                FileName = cmd[0],
                RedirectStandardInput = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            for (int i = 1; i < cmd.Length; i++)
                psi.ArgumentList.Add(cmd[i]);
            if (_ttyDev != null)
                psi.RedirectStandardInput = true;

            using var process = Process.Start(psi);
            if (process == null)
                return string.Empty;
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output.Trim();
        }

        protected virtual string[] GetSTTYCommand() => new[] { "/usr/bin/env", "stty" };
    }
}
