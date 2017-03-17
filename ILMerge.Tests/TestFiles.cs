using System.IO;
using NUnit.Framework;

namespace ILMerging.Tests
{
    internal static class TestFiles
    {
        private static string FromCurrentDir(string fileName)
            => Path.Combine(TestContext.CurrentContext.TestDirectory, fileName);

        public static string TestSnk => FromCurrentDir("test.snk");

        public static string TestPfx => FromCurrentDir("test.pfx");
    }
}
