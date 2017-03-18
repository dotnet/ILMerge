using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ILMerging.Tests.Helpers
{
    public static class ProcessUtils
    {
        public static ProcessResult Run(ProcessStartInfo startInfo)
        {
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.CreateNoWindow = true;

            using (var process = new Process { StartInfo = startInfo })
            {
                var standardStreamData = new List<StandardStreamData>();
                var currentData = new StringBuilder();
                var currentDataIsError = false;

                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data == null) return;
                    if (currentDataIsError)
                    {
                        if (currentData.Length != 0)
                            standardStreamData.Add(new StandardStreamData(currentDataIsError, currentData.ToString()));
                        currentData.Clear();
                        currentDataIsError = false;
                    }
                    currentData.AppendLine(e.Data);
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data == null) return;
                    if (!currentDataIsError)
                    {
                        if (currentData.Length != 0)
                            standardStreamData.Add(new StandardStreamData(currentDataIsError, currentData.ToString()));
                        currentData.Clear();
                        currentDataIsError = true;
                    }
                    currentData.AppendLine(e.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                if (currentData.Length != 0)
                    standardStreamData.Add(new StandardStreamData(currentDataIsError, currentData.ToString()));

                return new ProcessResult(process.ExitCode, standardStreamData.ToArray());
            }
        }

        [DebuggerDisplay("{ToString(),nq}")]
        public struct ProcessResult
        {
            public ProcessResult(int exitCode, StandardStreamData[] standardStreamData)
            {
                ExitCode = exitCode;
                StandardStreamData = standardStreamData;
            }

            public int ExitCode { get; }
            public StandardStreamData[] StandardStreamData { get; }

            public override string ToString() => ToString(true);

            /// <param name="showStreamSource">If true, appends "[stdout] " or "[stderr] " to the beginning of each line.</param>
            public string ToString(bool showStreamSource)
            {
                var r = new StringBuilder("Exit code ").Append(ExitCode);

                if (StandardStreamData.Length != 0) r.AppendLine();

                foreach (var data in StandardStreamData)
                {
                    if (showStreamSource)
                    {
                        var lines = data.Data.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                        // StandardStreamData.Data always ends with a blank line, so skip that
                        for (var i = 0; i < lines.Length - 1; i++)
                            r.Append(data.IsError ? "[stderr] " : "[stdout] ").AppendLine(lines[i]);
                    }
                    else
                    {
                        r.Append(data.Data);
                    }
                }

                return r.ToString();
            }
        }

        [DebuggerDisplay("{ToString(),nq}")]
        public struct StandardStreamData
        {
            public StandardStreamData(bool isError, string data)
            {
                IsError = isError;
                Data = data;
            }

            public bool IsError { get; }
            public string Data { get; }

            public override string ToString() => (IsError ? "[stderr] " : "[stdout] ") + Data;
        }
    }
}
