using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace ILMerging.Tests.Helpers
{
    [DebuggerDisplay("{ToString(),nq}")]
    public sealed class TempFile : IDisposable
    {
        public TempFile(string path)
        {
            this.path = path;
        }

        private string path;
        public string Path => path;

        public static implicit operator string(TempFile tempFile) => tempFile.path;

        public override string ToString() => path;

        public void Dispose()
        {
            var path = Interlocked.Exchange(ref this.path, null);
            if (path != null) File.Delete(path);
        }
    }
}
