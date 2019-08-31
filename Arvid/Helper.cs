using System;
using System.Diagnostics;

namespace Arvid
{
    public static class Helper
    {
        public static void ClearSpan<T>(Span<T> buffer)
        {
            for (var i = 0; i < buffer.Length; i++)
                buffer[i] = default;
        }
        
        public static void RunCommand(string command, string errorMessage)
        {
            var process = Process.Start(command);
            if (null == process)
            {
                throw new Exception(errorMessage);
            }
            process.WaitForExit();
            if (process.ExitCode < 0)
            {
                throw new Exception(errorMessage);
            }
        }
    }
}