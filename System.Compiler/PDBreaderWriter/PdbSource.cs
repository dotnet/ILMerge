#if UseSingularityPDB

///////////////////////////////////////////////////////////////////////////////
//
//  Microsoft Research Singularity PDB Info Library
//
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//
//  File:   PdbSource.cs
//
using System;

namespace Microsoft.Singularity.PdbInfo
{
    public class PdbSource
    {
        public uint     index;
        public string   name;

        public PdbSource(uint index, string name)
        {
            this.index = index;
            this.name = name;
        }
    }
}
#endif