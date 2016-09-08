#if UseSingularityPDB

///////////////////////////////////////////////////////////////////////////////
//
//  Microsoft Research Singularity PDB Info Library
//
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//
//  File:   PdbSlot.cs
//
using System;
using Microsoft.Singularity.PdbInfo.CodeView;
using Microsoft.Singularity.PdbInfo.Features;

namespace Microsoft.Singularity.PdbInfo
{
    public class PdbSlot
    {
        public uint slot;
        public string name;
        public ushort flags;
        public uint segment;
        public uint address;

        internal PdbSlot(BitAccess bits, out uint typind)
        {
            AttrSlotSym slot;

            bits.ReadUInt32(out slot.index);
            bits.ReadUInt32(out slot.typind);
            bits.ReadUInt32(out slot.offCod);
            bits.ReadUInt16(out slot.segCod);
            bits.ReadUInt16(out slot.flags);
            bits.ReadCString(out slot.name);

            this.slot = slot.index;
            this.name = slot.name;
            this.flags = slot.flags;
            this.segment = slot.segCod;
            this.address = slot.offCod;

            typind = slot.typind;
        }
    }
}
#endif