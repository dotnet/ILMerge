using System;
using System.Collections.Generic;
using System.Compiler;
using System.Linq;
using System.Text;

namespace Microsoft.Cci.Pdb
{
  public interface ILocalScope
  {
    /// <summary>
    /// The offset of the first operation in the scope.
    /// </summary>
    uint Offset { get; }

    /// <summary>
    /// The length of the scope. Offset+Length equals the offset of the first operation outside the scope, or equals the method body length.
    /// </summary>
    uint Length { get; }

    /// <summary>
    /// The definition of the method in which this local scope is defined.
    /// </summary>
    Method MethodDefinition
    {
      get;
    }

  }


  internal sealed class PdbIteratorScope : ILocalScope
  {

    internal PdbIteratorScope(uint offset, uint length)
    {
      this.offset = offset;
      this.length = length;
    }

    public uint Offset
    {
      get { return this.offset; }
    }
    uint offset;

    public uint Length
    {
      get { return this.length; }
    }
    uint length;

    public Method MethodDefinition
    {
      get { return this.methodDefinition; }
      set { this.methodDefinition = value; }
    }
    Method methodDefinition;
  }

}
