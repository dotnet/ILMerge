#if UseSingularityPDB

///////////////////////////////////////////////////////////////////////////////
//
//  Microsoft Research Singularity PDB Info Library
//
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//
//  File:   PdbLine.cs
//
using System;

namespace Microsoft.Singularity.PdbInfo
{
    public struct PdbLine
    {
        public uint     offset;
        public uint     line;
        public ushort   colBegin;
        public ushort   colEnd;

        public PdbLine(uint offset, uint line, ushort colBegin, ushort colEnd)
        {
            this.offset = offset;
            this.line = line;
            this.colBegin = colBegin;
            this.colEnd = colEnd;
        }
    }
}
#endif