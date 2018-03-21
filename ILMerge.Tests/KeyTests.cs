using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using ILMerging.Tests.Helpers;
using NUnit.Framework;

namespace ILMerging.Tests
{
    [TestFixture]
    public sealed class KeyTests
    {
        [Test]
        public void Can_sign_using_keyfile()
        {
            using (var outputFile = TempFile.WithExtension(".dll"))
            {
                var ilMerge = new ILMerge { KeyFile = TestFiles.TestSnk, OutputFile = outputFile };
                ilMerge.SetUpInputAssemblyForTest(Assembly.GetExecutingAssembly());
                ilMerge.Merge();

                Assert.That(
                    AssemblyName.GetAssemblyName(outputFile).GetPublicKey(),
                    Is.EqualTo(new StrongNameKeyPair(File.ReadAllBytes(TestFiles.TestSnk)).PublicKey));
            }
        }

        [Test]
        public void Sign_with_keyfile_should_not_throw_error()
        {
            using (var outputFile = TempFile.WithExtension(".dll"))
            {
                using (var logFile = TempFile.WithExtension(".log"))
                {
                    var ilMerge = new ILMerge {KeyFile = TestFiles.TestSnk, OutputFile = outputFile, LogFile = logFile};
                    ilMerge.SetUpInputAssemblyForTest(Assembly.GetExecutingAssembly());
                    ilMerge.Merge();

                    Assert.IsFalse(File.ReadLines(logFile).Contains("ILMerge error: The target assembly was not able to be strongly named (did you forget to use the /delaysign option?)."),"There was an error in the log output from signing the assembly with a keyfile");
                }
            }
        }

        [Test]
        public void Can_sign_using_keycontainer()
        {
            var keyContainerName = Guid.NewGuid().ToString();
            CspContainerUtils.ImportBlob(true, keyContainerName, KeyNumber.Signature, File.ReadAllBytes(TestFiles.TestSnk));
            try
            {
                using (var outputFile = TempFile.WithExtension(".dll"))
                {
                    var ilMerge = new ILMerge
                    {
                        KeyContainer = keyContainerName,
                        OutputFile = outputFile
                    };
                    ilMerge.SetUpInputAssemblyForTest(Assembly.GetExecutingAssembly());
                    ilMerge.Merge();

                    Assert.That(
                        AssemblyName.GetAssemblyName(outputFile).GetPublicKey(),
                        Is.EqualTo(new StrongNameKeyPair(File.ReadAllBytes(TestFiles.TestSnk)).PublicKey));
                }
            }
            finally
            {
                CspContainerUtils.Delete(true, keyContainerName, KeyNumber.Signature);
            }
        }

        [Test]
        public void Bad_keyfile_gives_diagnostic_warning()
        {
            using (var logFile = new TempFile())
            using (var outputFile = TempFile.WithExtension(".dll"))
            {
                var ilMerge = new ILMerge
                {
                    KeyFile = TestFiles.TestPfx,
                    OutputFile = outputFile,
                    LogFile = logFile
                };
                ilMerge.SetUpInputAssemblyForTest(Assembly.GetExecutingAssembly());

                ilMerge.Merge();

                var logText = File.ReadAllText(logFile);
                Assert.That(logText, Contains.Substring("Unable to obtain public key for StrongNameKeyPair."));
                Assert.That(logText, Contains.Substring("PFX"));
                Assert.That(logText, Contains.Substring("key container"));
            }
        }
    }
}
