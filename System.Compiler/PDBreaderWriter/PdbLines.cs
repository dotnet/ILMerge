#if UseSingularityPDB

///////////////////////////////////////////////////////////////////////////////
//
//  Microsoft Research Singularity PDB Info Library
//
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//
//  File:   PdbLines.cs
//
using System;

namespace Microsoft.Singularity.PdbInfo
{
    public class PdbLines
    {
        public PdbSource    file;
        public PdbLine[]    lines;

        public PdbLines(PdbSource file, uint count)
        {
            this.file = file;
            this.lines = new PdbLine[count];
        }
    }
}
#endif