#if UseSingularityPDB

///////////////////////////////////////////////////////////////////////////////
//
//  Microsoft Research Singularity PDB Info Library
//
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//
//  File:   PdbFunction.cs
//
using System;
using System.Collections;
using Microsoft.Singularity.PdbInfo.CodeView;
using Microsoft.Singularity.PdbInfo.Features;

namespace Microsoft.Singularity.PdbInfo
{
    public class PdbFunction
    {
        static public readonly Guid msilMetaData = new Guid(0xc6ea3fc9,0x59b3,0x49d6,0xbc,0x25,
                                                            0x09,0x02,0xbb,0xab,0xb4,0x60);
        static public readonly IComparer byAddress = new PdbFunctionsByAddress();
        static public readonly IComparer byToken = new PdbFunctionsByToken();

        public uint token;
        public uint slotToken;
        public string name;
        public string module;
        public ushort flags;

        public uint segment;
        public uint address;
        public uint length;

        public byte[] metadata;
        public PdbScope[] scopes;
        public PdbLines[] lines;

        private static string StripNamespace(string module)
        {
            int li = module.LastIndexOf('.');
            if (li > 0) {
                return module.Substring(li + 1);
            }
            return module;
        }


        internal static PdbFunction[] LoadManagedFunctions(string module,
                                                           BitAccess bits, uint limit,
                                                           bool readStrings)
        {
            string mod = StripNamespace(module);
            int begin = bits.Position;
            int count = 0;

            while (bits.Position < limit) {
                ushort siz;
                ushort rec;

                bits.ReadUInt16(out siz);
                int star = bits.Position;
                int stop = bits.Position + siz;
                bits.Position = star;
                bits.ReadUInt16(out rec);

                switch ((SYM)rec) {
                    case SYM.S_GMANPROC:
                    case SYM.S_LMANPROC:
                        ManProcSym proc;
                        bits.ReadUInt32(out proc.parent);
                        bits.ReadUInt32(out proc.end);
                        bits.Position = (int)proc.end;
                        count++;
                        break;

                    case SYM.S_END:
                        bits.Position = stop;
                        break;

                    default:
                        Console.WriteLine("{0,6}: {1:x2} {2}",
                                          bits.Position, rec, (SYM)rec);
                        bits.Position = stop;
                        break;
                }
            }
            if (count == 0) {
                return null;
            }

            bits.Position = begin;
            PdbFunction[] funcs = new PdbFunction[count];
            int func = 0;

            while (bits.Position < limit) {
                ushort siz;
                ushort rec;

                bits.ReadUInt16(out siz);
                int star = bits.Position;
                int stop = bits.Position + siz;
                bits.ReadUInt16(out rec);

                switch ((SYM)rec) {

                    case SYM.S_GMANPROC:
                    case SYM.S_LMANPROC:
                        ManProcSym proc;
                        int offset = bits.Position;

                        bits.ReadUInt32(out proc.parent);
                        bits.ReadUInt32(out proc.end);
                        bits.ReadUInt32(out proc.next);
                        bits.ReadUInt32(out proc.len);
                        bits.ReadUInt32(out proc.dbgStart);
                        bits.ReadUInt32(out proc.dbgEnd);
                        bits.ReadUInt32(out proc.token);
                        bits.ReadUInt32(out proc.off);
                        bits.ReadUInt16(out proc.seg);
                        bits.ReadUInt8(out proc.flags);
                        bits.ReadUInt16(out proc.retReg);
                        if (readStrings) {
                            bits.ReadCString(out proc.name);
                        }
                        else {
                            bits.SkipCString(out proc.name);
                        }
                        //Console.WriteLine("token={0:X8} [{1}::{2}]", proc.token, module, proc.name);

                        bits.Position = stop;
                        funcs[func++] = new PdbFunction(module, proc, bits);
                        break;

                    default: {
                        throw new PdbDebugException("Unknown SYMREC {0}", (SYM)rec);
                        // bits.Position = stop;
                    }
                }
            }
            return funcs;
        }

        internal static void CountScopesAndSlots(BitAccess bits, uint limit,
                                                 out int scopes, out int slots)
        {
            int pos = bits.Position;
            BlockSym32 block;
            slots = 0;
            scopes = 0;

            while (bits.Position < limit) {
                ushort siz;
                ushort rec;

                bits.ReadUInt16(out siz);
                int star = bits.Position;
                int stop = bits.Position + siz;
                bits.Position = star;
                bits.ReadUInt16(out rec);

                switch ((SYM)rec) {
                    case SYM.S_BLOCK32: {
                        bits.ReadUInt32(out block.parent);
                        bits.ReadUInt32(out block.end);

                        scopes++;
                        bits.Position = (int)block.end;
                        break;
                    }

                    case SYM.S_MANSLOT:
                        slots++;
                        bits.Position = stop;
                        break;

                    default:
                        bits.Position = stop;
                        break;
                }
            }
            bits.Position = pos;
        }

        internal PdbFunction()
        {
        }

        internal PdbFunction(string module, ManProcSym proc, BitAccess bits)
        {
            this.token = proc.token;
            this.module = module;
            this.name = proc.name;
            this.flags = proc.flags;
            this.segment = proc.seg;
            this.address = proc.off;
            this.length = proc.len;
            this.slotToken = 0;

            if (proc.seg != 1) {
                throw new PdbDebugException("Segment is {0}, not 1.", proc.seg);
            }
            if (proc.parent != 0 || proc.next != 0) {
                throw new PdbDebugException("Warning parent={0}, next={1}",
                                            proc.parent, proc.next);
            }
            if (proc.dbgStart != 0 || proc.dbgEnd != 0) {
                throw new PdbDebugException("Warning DBG start={0}, end={1}",
                                            proc.dbgStart, proc.dbgEnd);
            }

            int scopeCount;
            int slotCount;
            CountScopesAndSlots(bits, proc.end, out scopeCount, out slotCount);
            scopes = new PdbScope[scopeCount];
            int scope = 0;

            while (bits.Position < proc.end) {
                ushort siz;
                ushort rec;

                bits.ReadUInt16(out siz);
                int star = bits.Position;
                int stop = bits.Position + siz;
                bits.Position = star;
                bits.ReadUInt16(out rec);

                switch ((SYM)rec) {
                    case SYM.S_OEM: {          // 0x0404
                        OemSymbol oem;

                        bits.ReadGuid(out oem.idOem);
                        bits.ReadUInt32(out oem.typind);
                        // public byte[]   rgl;        // user data, force 4-byte alignment

                        if (oem.idOem == msilMetaData) {
                            metadata = new byte[stop - bits.Position];
                            bits.ReadBytes(metadata);
                            bits.Position = stop;
                            break;
                        }
                        else {
                            throw new PdbDebugException("OEM section: guid={0} ti={1}",
                                                        oem.idOem, oem.typind);
                            // bits.Position = stop;
                        }
                    }

                    case SYM.S_BLOCK32: {
                        BlockSym32 block = new BlockSym32();

                        bits.ReadUInt32(out block.parent);
                        bits.ReadUInt32(out block.end);
                        bits.ReadUInt32(out block.len);
                        bits.ReadUInt32(out block.off);
                        bits.ReadUInt16(out block.seg);
                        bits.SkipCString(out block.name);
                        bits.Position = stop;

                        scopes[scope] = new PdbScope(block, bits, out slotToken);
                        bits.Position = (int)block.end;
                        break;
                    }

                    case SYM.S_UNAMESPACE:
                        bits.Position = stop;
                        break;

                    case SYM.S_END:
                        bits.Position = stop;
                        break;

                    default: {
                        throw new PdbDebugException("Unknown SYM: {0}", (SYM)rec);
                        // bits.Position = stop;
                    }
                }
            }

            if (bits.Position != proc.end) {
                throw new PdbDebugException("Not at S_END");
            }

            ushort esiz;
            ushort erec;
            bits.ReadUInt16(out esiz);
            bits.ReadUInt16(out erec);

            if (erec != (ushort)SYM.S_END) {
                throw new PdbDebugException("Missing S_END");
            }
        }

        internal class PdbFunctionsByAddress : IComparer
        {
            public int Compare(Object x, Object y)
            {
                PdbFunction fx = (PdbFunction)x;
                PdbFunction fy = (PdbFunction)y;

                if (fx.segment < fy.segment) {
                    return -1;
                }
                else if (fx.segment > fy.segment) {
                    return 1;
                }
                else if (fx.address < fy.address) {
                    return -1;
                }
                else if (fx.address > fy.address) {
                    return 1;
                }
                else {
                    return 0;
                }
            }
        }

        internal class PdbFunctionsByToken : IComparer
        {
            public int Compare(Object x, Object y)
            {
                PdbFunction fx = (PdbFunction)x;
                PdbFunction fy = (PdbFunction)y;

                if (fx.token < fy.token) {
                    return -1;
                }
                else if (fx.token > fy.token) {
                    return 1;
                }
                else {
                    return 0;
                }
            }

        }
    }
}
#endif