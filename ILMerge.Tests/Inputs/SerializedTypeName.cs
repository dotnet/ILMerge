namespace SerializedTypeName
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class M : ITwoTypeArgs<int, int>
    {
        internal static void Main()
        {
            var asm = Assembly.GetCallingAssembly();
            var types = asm.DefinedTypes;
            Console.WriteLine(types.First());
        }

        IEnumerator<int> ITwoTypeArgs<int, int>.Mk()
        {
            yield return 42;
        }
    }

    public interface ITwoTypeArgs<T1, T2>
    {
        IEnumerator<int> Mk();
    }
}