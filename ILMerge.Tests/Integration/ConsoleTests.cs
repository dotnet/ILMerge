using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using ILMerging.Tests.Helpers;
using NUnit.Framework;

namespace ILMerging.Tests.Integration
{
    [TestFixture, Category("Integration")]
    public sealed class ConsoleTests
    {
        [TestCase(true, TestName = "{m}(with mscorsn in path)")]
        [TestCase(false, TestName = "{m}(without mscorsn in path)")]
        public void No_DLL_load_crashes_when_given_PFX(bool withMscorsnInPath)
        {
            var ilMergeExePath = typeof(ILMerge).Assembly.Location;
            var inputAssembly = Assembly.GetExecutingAssembly();

            using (var outputFile = TempFile.WithExtension(".dll"))
            {
                var startInfo = new ProcessStartInfo(
                    ilMergeExePath,
                    $"{ShadowCopyUtils.GenerateILMergeLibCliSwitches(inputAssembly)} /keyfile:\"{TestFiles.TestPfx}\" /out:\"{outputFile}\" \"{inputAssembly.Location}\"")
                {
                    WorkingDirectory = Path.GetDirectoryName(inputAssembly.Location)
                };

                if (withMscorsnInPath)
                    startInfo.EnvironmentVariables["PATH"] = $"{Environment.GetEnvironmentVariable("PATH")};{RuntimeEnvironment.GetRuntimeDirectory()}";

                // The system runs .NET executables as 64-bit no matter what the architecture of the calling process is.
                var result = ProcessUtils.Run(startInfo);

                Assert.That(result.ToString(), Does.Not.Contain("Unable to load DLL 'mscorsn.dll'"));
                Assert.That(result.ToString(), Does.Not.Contain("An attempt was made to load a program with an incorrect format."));


                // Test failures:

                if (withMscorsnInPath && !Environment.Is64BitOperatingSystem) Assert.Inconclusive("This test can only be run on a 64-bit OS.");
                
                Assert.That(
                    result.ToString(),
                    Does.Not.Contain("Unhandled Exception: System.IO.FileNotFoundException"),
                    "The test is not being run properly. If you are using ReSharper, disable shadow copy. " +
                    "If you are using NCrunch, go to NCrunch's configuration for the ILMerge project and " +
                    "make sure \"Copy referenced assemblies to workspace\" is set to True. " +
                    "(Both ReSharper and NCrunch settings are saved in the repo, so this should not happen.)");
            }
        }
    }
}
