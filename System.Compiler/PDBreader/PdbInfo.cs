using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Cci.Pdb;
using System.Diagnostics;

namespace System.Compiler
{
  internal class PdbInfo
  {
    private unsafe Metadata.Reader reader;
    string sourceServerData;
    Dictionary<uint, PdbTokenLine> tokenToSourceMapping;
    //private uint[] remapTable;
    private Dictionary<uint, PdbFunction> pdbFunctionMap;


    public unsafe PdbInfo(IO.FileStream inputStream, Metadata.Reader reader)
    {
      this.reader = reader;
      this.pdbFunctionMap = PdbFile.LoadFunctionMap(inputStream, out tokenToSourceMapping, out sourceServerData, reader);
      //inputStream.Seek(0L, IO.SeekOrigin.Begin);
      //this.remapTable = PdbFile.LoadRemapTable(inputStream);
    }

#if false
    public Method GetMethodFromPdbToken(uint token)
    {
      // remap if necessary
      if (this.remapTable != null)
      {
        token = 0x06000000 | this.remapTable[token & 0xffffff];
      }
      return reader.GetMemberFromToken((int)token) as Method;
    }
#endif

    public PdbFunction GetMethodInfo(uint token)
    {
      PdbFunction result;
      this.pdbFunctionMap.TryGetValue(token, out result);
      return result;
    }
  }
}
