using System.IO;
using NUnit.Framework;

namespace ILMerging.Tests
{
    internal static class TestFiles
    {
        public static string TestSnk { get; } = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.snk");
        public static string TestPfx { get; } = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.pfx");
    }
}
