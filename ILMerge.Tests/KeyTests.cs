using System.IO;
using System.Reflection;
using ILMerging.Tests.Helpers;
using NUnit.Framework;

namespace ILMerging.Tests
{
    [TestFixture]
    public sealed class KeyTests
    {
        private const string KeyFile = "test.snk";

        [Test]
        public void Can_sign_using_keyfile()
        {
            using (var outputFile = TempFile.WithExtension(".dll"))
            {
                var ilMerge = new ILMerge { KeyFile = KeyFile, OutputFile = outputFile };
                ilMerge.SetUpInputAssemblyForTest(Assembly.GetExecutingAssembly());
                ilMerge.Merge();

                Assert.That(
                    AssemblyName.GetAssemblyName(outputFile).GetPublicKey(),
                    Is.EqualTo(new StrongNameKeyPair(File.ReadAllBytes(KeyFile)).PublicKey));
            }
        }
    }
}
