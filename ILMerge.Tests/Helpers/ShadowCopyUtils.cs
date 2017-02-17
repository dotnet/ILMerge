using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ILMerging.Tests.Helpers
{
    public static class ShadowCopyUtils
    {
        public static IEnumerable<Assembly> GetTransitiveClosure(params Assembly[] assemblies)
        {
            var finishedAssemblies = new HashSet<Assembly>();

            using (var en = StackEnumerator.Create(assemblies))
                foreach (var assembly in en)
                {
                    if (!finishedAssemblies.Add(assembly)) continue;
                    yield return assembly;
                    en.Recurse(assembly.GetReferencedAssemblies().Select(Assembly.Load));
                }
        }

        /// <summary>
        /// Necessary because of test runners like ReSharper and NCrunch which shadow copy each assembly to an isolated directory
        /// </summary>
        public static IEnumerable<string> GetTransitiveClosureDirectories(params Assembly[] assemblies)
        {
            return GetTransitiveClosure(assemblies)
                .Where(_ => !_.GlobalAssemblyCache)
                .Select(_ => Path.GetDirectoryName(_.Location))
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Necessary because of test runners like ReSharper and NCrunch which shadow copy each assembly to an isolated directory
        /// </summary>
        public static string GenerateILMergeLibCliSwitches(params Assembly[] assemblies)
        {
            return string.Join(" ", GetTransitiveClosureDirectories(Assembly.GetExecutingAssembly()).Select(_ => $"/lib:\"{_}\""));
        }
    }
}
