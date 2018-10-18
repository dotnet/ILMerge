// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information. 

using System;
using System.IO;

namespace Microsoft.Cci.Pdb
{
  internal class PdbReader
  {
    internal PdbReader(Stream reader, int pageSize)
    {
      this.pageSize = pageSize;
      this.reader = reader;
    }

    internal void Seek(int page, int offset)
    {
      reader.Seek(page * pageSize + offset, SeekOrigin.Begin);
    }

    internal void Read(byte[] bytes, int offset, int count)
    {
      reader.Read(bytes, offset, count);
    }

    internal int PagesFromSize(int size)
    {
      return (size + pageSize - 1) / (pageSize);
    }

    //internal int PageSize {
    //  get { return pageSize; }
    //}

    internal readonly int pageSize;
    internal readonly Stream reader;
  }
}
