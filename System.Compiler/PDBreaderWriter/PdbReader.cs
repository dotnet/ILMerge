#if UseSingularityPDB

///////////////////////////////////////////////////////////////////////////////
//
//  Microsoft Research Singularity PDB Info Library
//
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//
//  File:   PdbReader.cs
//
using System;
using System.IO;

namespace Microsoft.Singularity.PdbInfo.Features
{
    public class PdbReader
    {
        public PdbReader(Stream reader, int pageSize)
        {
            this.pageSize = pageSize;
            this.reader = reader;
        }

        public void Seek(int page, int offset)
        {
            reader.Seek(page * pageSize + offset, SeekOrigin.Begin);
        }

        public void Read(byte[] bytes, int offset, int count)
        {
            reader.Read(bytes, offset, count);
        }

        public int PagesFromSize(int size)
        {
            return (size + pageSize - 1) / (pageSize);
        }

        public int PageSize
        {
            get { return pageSize; }
        }

        internal readonly int pageSize;
        internal readonly Stream reader;
    }
}
#endif