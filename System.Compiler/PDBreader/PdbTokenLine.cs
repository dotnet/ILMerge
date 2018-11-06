// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information. 

using System;

namespace Microsoft.Cci.Pdb
{
  internal class PdbTokenLine
  {
    internal uint token;
    internal uint file_id;
    internal uint line;
    internal uint column;
    internal uint endLine;
    internal uint endColumn;
    internal PdbSource sourceFile;
    internal PdbTokenLine/*?*/ nextLine;

    internal PdbTokenLine(uint token, uint file_id, uint line, uint column, uint endLine, uint endColumn)
    {
      this.token = token;
      this.file_id = file_id;
      this.line = line;
      this.column = column;
      this.endLine = endLine;
      this.endColumn = endColumn;
    }
  }
}
