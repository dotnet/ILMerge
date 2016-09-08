#if UseSingularityPDB

///////////////////////////////////////////////////////////////////////////////
//
//  Microsoft Research Singularity PDB Info Library
//
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//
//  File:   PdbException.cs
//
using System;
using System.IO;

namespace Microsoft.Singularity.PdbInfo
{
    public class PdbException : IOException
    {
        public PdbException(String format, params object[] args)
            : base(String.Format(format, args))
        {
        }
    }
}
#endif