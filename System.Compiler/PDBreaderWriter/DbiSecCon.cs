#if UseSingularityPDB

///////////////////////////////////////////////////////////////////////////////
//
//  Microsoft Research Singularity PDB Info Library
//
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//
//  File:   DbiSecCon.cs
//
using System;

namespace Microsoft.Singularity.PdbInfo.Features
{
    public struct DbiSecCon
    {
        public DbiSecCon(BitAccess bits)
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
            if (pad1 != 0 || pad2 != 0) {
                throw new PdbException("Invalid DBI section. "+
                                       "(pad1={0}, pad2={1})",
                                       pad1, pad2);
            }
        }

        public short  section;                    // 0..1
        public short  pad1;                       // 2..3
        public int    offset;                     // 4..7
        public int    size;                       // 8..11
        public uint   flags;                      // 12..15
        public short  module;                     // 16..17
        public short  pad2;                       // 18..19
        public uint   dataCrc;                    // 20..23
        public uint   relocCrc;                   // 24..27
    }
}
#endif