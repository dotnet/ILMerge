// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Cci.Pdb
{
  internal class PdbSlot
  {
    internal uint slot;
    internal uint typeToken;
    internal string name;
    internal ushort flags;
    //internal uint segment;
    //internal uint address;

    internal PdbSlot(BitAccess bits)
    {
      AttrSlotSym slot;

      bits.ReadUInt32(out slot.index);
      bits.ReadUInt32(out slot.typind);
      bits.ReadUInt32(out slot.offCod);
      bits.ReadUInt16(out slot.segCod);
      bits.ReadUInt16(out slot.flags);
      bits.ReadCString(out slot.name);

      this.slot = slot.index;
      this.typeToken = slot.typind;
      this.name = slot.name;
      this.flags = slot.flags;
      //this.segment = slot.segCod;
      //this.address = slot.offCod;

    }
  }
}
