using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Cci.Pdb
{
  internal struct PdbLine
  {
    readonly internal uint offset;
    readonly internal uint lineBegin;
    readonly internal uint lineEnd;
    readonly internal ushort colBegin;
    readonly internal ushort colEnd;

    internal PdbLine(uint offset, uint lineBegin, ushort colBegin, uint lineEnd, ushort colEnd)
    {
      this.offset = offset;
      this.lineBegin = lineBegin;
      this.colBegin = colBegin;
      this.lineEnd = lineEnd;
      this.colEnd = colEnd;
    }
  }
  internal class PdbLines
  {
    readonly internal PdbSource file;
    readonly internal PdbLine[] lines;

    internal PdbLines(PdbSource file, uint count)
    {
      this.file = file;
      this.lines = new PdbLine[count];
    }
  }
}
