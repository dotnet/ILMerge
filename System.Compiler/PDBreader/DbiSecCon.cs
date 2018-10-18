// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information. 

using System;

namespace Microsoft.Cci.Pdb
{
  internal struct DbiSecCon
  {
    internal DbiSecCon(BitAccess bits)
    {
      bits.ReadInt16(out section);
      bits.ReadInt16(out pad1);
      bits.ReadInt32(out offset);
      bits.ReadInt32(out size);
      bits.ReadUInt32(out flags);
      bits.ReadInt16(out module);
      bits.ReadInt16(out pad2);
      bits.ReadUInt32(out dataCrc);
      bits.ReadUInt32(out relocCrc);
      //if (pad1 != 0 || pad2 != 0) {
      //  throw new PdbException("Invalid DBI section. "+
      //                                 "(pad1={0}, pad2={1})",
      //                         pad1, pad2);
      //}
    }

    readonly internal short section;                    // 0..1
    readonly internal short pad1;                       // 2..3
    readonly internal int offset;                     // 4..7
    readonly internal int size;                       // 8..11
    readonly internal uint flags;                      // 12..15
    readonly internal short module;                     // 16..17
    readonly internal short pad2;                       // 18..19
    readonly internal uint dataCrc;                    // 20..23
    readonly internal uint relocCrc;                   // 24..27
  }
}
