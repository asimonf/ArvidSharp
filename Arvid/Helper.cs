using System;
using System.Diagnostics;

namespace Arvid
{
    public static class Helper
    {
        // Max segment size is 48KiB or 24KiW
        public const int MaxSegmentSize = 24576;
        
        public static void ClearSpan<T>(Span<T> buffer)
        {
            for (var i = 0; i < buffer.Length; i++)
                buffer[i] = default;
        }

        public static string Bash(this string cmd, string errorMessage)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode < 0)
            {
                throw new Exception(errorMessage);
            }

            return result;
        }
    }
}