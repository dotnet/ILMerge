#if UseSingularityPDB

///////////////////////////////////////////////////////////////////////////////
//
//  Microsoft Research Singularity PDB Info Library
//
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//
//  File:   PdbDebugException.cs
//
using System;
using System.IO;

namespace Microsoft.Singularity.PdbInfo
{
    public class PdbDebugException : IOException
    {
        public PdbDebugException(String format, params object[] args)
            : base(String.Format(format, args))
        {
        }
    }
}
#endif