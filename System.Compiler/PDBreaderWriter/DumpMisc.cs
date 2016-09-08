#if UseSingularityPDB

///////////////////////////////////////////////////////////////////////////////
//
//  Microsoft Research Singularity PDB Info Library
//
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//
//  File:   DumpMisc.cs
//
using System;
using System.Collections;
using System.IO;
using System.Text;
using Microsoft.Singularity.PdbInfo;
using Microsoft.Singularity.PdbInfo.CodeView;
using Microsoft.Singularity.PdbInfo.Features;

namespace Microsoft.Singularity.Applications
{
    //////////////////////////////////////////////////////////////////////////
    //
    public class DumpMisc
    {
        public static void Dump(BitSet bits)
        {
            uint word;

            for (int i = 0; bits.GetWord(i, out word); i++) {
                Console.Write("{0:x8}", word);
            }
        }

        public static void Dump(PdbFunction f, int indent)
        {
            string pad = new String(' ', indent);
            Console.WriteLine("            {0}Func: [{1}] addr={2:x4}:{3:x8} len={4:x4} token={5:x8}",
                              pad,
                              f.name, f.segment, f.address, f.length, f.token);
            if (f.metadata != null) {
                Console.Write("            {0} Meta: [", pad);
                for (int i = 0; i < f.metadata.Length; i++) {
                    Console.Write("{0:x2}", f.metadata[i]);
                }
                Console.WriteLine("]");
            }
            if (f.scopes != null) {
                for (int i = 0; i < f.scopes.Length; i++) {
                    Dump(f.scopes[i], indent + 1);
                }
            }
            if (f.lines != null) {
                for (int i = 0; i < f.lines.Length; i++) {
                    Dump(f.lines[i], indent + 1);
                }
            }
        }

        public static void Dump(PdbScope s, int indent)
        {
            string pad = new String(' ', indent);
            Console.WriteLine("            {0}Scope: addr={1:x4}:{2:x8} len={3:x4}",
                              pad, s.segment, s.address, s.length);
            if (s.slots != null) {
                for (int i = 0; i < s.slots.Length; i++) {
                    Dump(s.slots[i], indent + 1);
                }
            }
            if (s.scopes != null) {
                for (int i = 0; i < s.scopes.Length; i++) {
                    Dump(s.scopes[i], indent + 1);
                }
            }
        }

        public static void Dump(PdbSlot s, int indent)
        {
            string pad = new String(' ', indent);
            Console.WriteLine("            {0}Slot: {1,2} [{2}] addr={3:x4}:{4:x8}",
                              pad, s.slot, s.name, s.segment, s.address);
        }


        public static void Dump(PdbSource s, int indent)
        {
            string pad = new String(' ', indent);
            Console.WriteLine("            {0}[{1}] : {2}", pad, s.name, s.index);
        }

        public static void Dump(PdbLines s, int indent)
        {
            string pad = new String(' ', indent);
            Console.WriteLine("            {0}[{1}]", pad, s.file.name);
            for (int i = 0; i < s.lines.Length; i++) {
                Dump(s.lines[i], indent + 1);
            }
        }

        public static void Dump(PdbLine s, int indent)
        {
            string pad = new String(' ', indent);
            if (s.line == 0xfeefee && s.colBegin == 0 && s.colEnd == 0) {
                Console.WriteLine("            {0}off={1:x8} #---------",
                                  pad, s.offset, s.line, s.colBegin);
            }
            else {
                Console.WriteLine("            {0}off={1:x8} #{2,6},{3,2}",
                                  pad, s.offset, s.line, s.colBegin);
            }
        }

        public static void Dump(DbiSecCon s)
        {
            Console.WriteLine("          section={0}, offset={1}, size={2}, flags={3:x8}, mod={4}",
                              s.section,
                              s.offset,
                              s.size,
                              s.flags,
                              s.module);
            Console.WriteLine("          data={0:x8}, reloc={1:x8}",
                              s.dataCrc,
                              s.relocCrc);
        }


        public static void Dump(DbiModuleInfo s)
        {
            Console.WriteLine("        stream={0,4}. {1,3} [{2}] {3}/{4}",
                              s.stream, s.section.module, s.moduleName,
                              s.cbSyms, s.cbLines);
#if VERBOSE
            Console.WriteLine("          flags={0:x4}, sym={1}, lin={2}, c13={3}, files={4}",
                              s.stream,
                              s.flags,
                              s.cbSyms,
                              s.cbOldLines,
                              s.cbLines,
                              s.files);
            Dump(section);
            Console.WriteLine("          offsets={0:x8}, source={1}, compiler={2}",
                              s.offsets, s.niSource, s.niCompiler);
            Console.WriteLine("          mod=[{0}]", s.moduleName);
            Console.WriteLine("          obj=[{0}]", s.objectName);
#endif
        }

        public static void Dump(PdbFileHeader f)
        {
            Console.WriteLine("FileHeader({0}, size={1}, pages={2}, dir={3}.{4})",
                              f.Magic,
                              f.pageSize,
                              f.pagesUsed,
                              f.directoryRoot,
                              f.directorySize);
        }

        public static void Dump(DataStream d)
        {
            Console.Write("DataStream({0} bytes", d.Length);
            for (int i = 0; i < d.Pages; i++) {
                Console.Write(",{0}", d.GetPage(i));
            }
            Console.WriteLine(")");
        }

        static void DumpLine(byte[] bytes, int offset, int limit)
        {
            for (int j = 0; j < 16; j++) {
                if (j % 4 == 0) {
                    Console.Write(" ");
                }
                if (offset + j < limit) {
                    Console.Write("{0:x2}", bytes[offset + j]);
                }
                else {
                    Console.Write("  ");
                }
            }
            Console.Write(" ");
            for (int j = 0; j < 16; j++) {
                if (offset + j < limit) {
                    if (bytes[offset + j] >= ' ' && bytes[offset + j] < 127) {
                        Console.Write("{0}", unchecked((char)bytes[offset + j]));
                    }
                    else {
                        Console.Write(".");
                    }
                }
                else {
                    Console.Write(" ");
                }
            }
        }

        static void DumpLineExt(byte[] bytes, int offset, int limit)
        {
            DumpLine(bytes, offset, limit);
            for (int j = offset; j < offset + 16; j += 4) {
                if (j + 4 <= limit) {
                    int value = (int)((bytes[j + 0] & 0xFF) |
                                      (bytes[j + 1] << 8) |
                                      (bytes[j + 2] << 16) |
                                      (bytes[j + 3] << 24));
                    if (value < -9999999 || value > 99999999) {
                        Console.Write("       .");
                    }
                    else {
                        Console.Write("{0,8}", value);
                    }
                }
                else {
                    Console.Write("        ");
                }
            }
        }

        public static void Dump(byte[] bytes, int offset, int limit)
        {
            for (int i = offset; i < limit; i += 16) {
                Console.Write("    {0,10}:", i);
                DumpLine(bytes, i, limit);
                Console.WriteLine();
            }
        }

        static void Dump(int label, byte[] bytes, int limit)
        {
            for (int i = 0; i < limit; i += 16) {
                Console.Write("         {0,10}:", label + i);
                DumpLine(bytes, i, limit);
                Console.WriteLine();
            }
        }

        static void DumpVerbose(DataStream stream, PdbReader reader)
        {
            byte[] bytes = new byte[reader.PageSize];

            if (stream.Length <= 0) {
                return;
            }

            int left = stream.Length;
            int pos = 0;

            for (int page = 0; left > 0; page++) {
                int todo = bytes.Length;
                if (todo > left) {
                    todo = left;
                }

                stream.Read(reader, pos, bytes, 0, todo);

                for (int i = 0; i < todo; i += 16) {
                    Console.Write("{0,7}:", pos + i);
                    DumpLineExt(bytes, i, todo);
                    Console.WriteLine();
                }

                left -= todo;
                pos += todo;
            }
        }

        static void DumpPdbStream(BitAccess bits,
                                  out int linkStream,
                                  out int nameStream,
                                  out int srchStream)
        {
            linkStream = 0;
            nameStream = 0;
            srchStream = 0;

            int ver;
            int sig;
            int age;
            Guid guid;
            bits.ReadInt32(out ver);    //  0..3  Version
            bits.ReadInt32(out sig);    //  4..7  Signature
            bits.ReadInt32(out age);    //  8..11 Age
            bits.ReadGuid(out guid);       // 12..27 GUID

            // Read string buffer.
            int buf;
            bits.ReadInt32(out buf);    // 28..31 Bytes of Strings

            Console.WriteLine("   ** PDB ver={0,8} sig={1:x8} age={2} guid={3}",
                              ver, sig, age, guid);
            int beg = bits.Position;
            int nxt = bits.Position + buf;

            bits.Position = nxt;

            // Read map index.
            int cnt;        // n+0..3 hash size.
            int max;        // n+4..7 maximum ni.

            bits.ReadInt32(out cnt);
            bits.ReadInt32(out max);
            Console.WriteLine("      cnt={0}, max={1}", cnt, max);

            BitSet present = new BitSet(bits);
            BitSet deleted = new BitSet(bits);
            if (!deleted.IsEmpty) {
                Console.Write("        deleted: ");
                Dump(deleted);
                Console.WriteLine();
            }
            int j = 0;
            for (int i = 0; i < max; i++) {
                if (present.IsSet(i)) {
                    int ns;
                    int ni;
                    bits.ReadInt32(out ns);
                    bits.ReadInt32(out ni);

                    string name;
                    int saved = bits.Position;
                    bits.Position = beg + ns;
                    bits.ReadCString(out name);
                    bits.Position = saved;

                    if (name == "/names") {
                        nameStream = ni;
                    }
                    else if (name == "/src/headerblock") {
                        srchStream = ni;
                    }
                    else if (name == "/LinkInfo") {
                        linkStream = ni;
                    }
                    Console.WriteLine("        {0,4}: [{1}]", ni, name);
                    j++;
                }
            }
            if (j != cnt) {
                throw new PdbDebugException("Count mismatch. ({0} != {1})", j, cnt);
            }

            // Read maxni.
            int maxni;
            bits.ReadInt32(out maxni);
            Console.WriteLine("        maxni={0}", maxni);
        }

        internal static int Hash(byte[] bytes, int mod)
        {
            byte b0 = 0;
            byte b1 = 0;
            byte b2 = 0;
            byte b3 = 0;
            int i = 0;

            // word
            while (i + 4 <= bytes.Length) {
                b0 ^= bytes[i + 0];
                b1 ^= bytes[i + 1];
                b2 ^= bytes[i + 2];
                b3 ^= bytes[i + 3];
                i += 4;
            }
            // odd half word
            if (i + 2 <= bytes.Length) {
                b0 ^= bytes[i + 0];
                b1 ^= bytes[i + 1];
            }
            // odd byte
            if (i < bytes.Length) {
                b0 ^= bytes[i + 0];
            }

            // make case insensitive.
            b0 |= 0x20;
            b1 |= 0x20;
            b2 |= 0x20;
            b3 |= 0x20;

            // put it together.
            uint hash = (uint)((b0 & 0xFF) | (b1 << 8) | (b2 << 16) | (b3 << 24));
            hash ^= (hash >> 11);
            hash ^= (hash >> 16);
            return (int)(hash % (uint)mod);
        }

        static void DumpNameStream(BitAccess bits)
        {
            uint sig;
            int ver;
            bits.ReadUInt32(out sig);   //  0..3  Signature
            bits.ReadInt32(out ver);    //  4..7  Version

            // Read (or skip) string buffer.
            int buf;
            bits.ReadInt32(out buf);    //  8..11 Bytes of Strings

            Console.Write("   ** NAM sig={0:x8} ver={1,4} cb={2,4}",
                          sig, ver, buf);

            if (sig != 0xeffeeffe || ver != 1) {
                throw new PdbDebugException("Unknown Name Stream version. "+
                                            "(sig={0:x8}, ver={1})",
                                            sig, ver);
            }
#if true
            int beg = bits.Position;
            int nxt = bits.Position + buf;
            bits.Position = nxt;

            // Read hash table.
            int siz;
            bits.ReadInt32(out siz);    // n+0..3 Number of hash buckets.
            nxt = bits.Position;

            Console.WriteLine("      siz={0,4}", siz);
            for (int i = 0; i < siz; i++) {
                int ni;
                string name;

                bits.ReadInt32(out ni);

                if (ni != 0) {
                    int saved = bits.Position;
                    bits.Position = beg + ni;
                    bits.ReadCString(out name);
                    bits.Position = saved;

                    if (name.Length > 60) {
                        Console.WriteLine("        {0,6}: [{1}]+", ni, name.Substring(0,60));
                    }
                    else {
                        Console.WriteLine("        {0,6}: [{1}]", ni, name);
                    }
                }
            }
            bits.Position = nxt;

            // Read maxni.
            int maxni;
            bits.ReadInt32(out maxni);
            Console.WriteLine("        maxni={0}", maxni);
#endif
        }

        static void DumpTpiStream(BitAccess bits,
                                  out int tiHashStream,
                                  out int tiHPadStream)
        {
            tiHashStream = 0;
            tiHPadStream = 0;

            // Read the header structure.
            int ver;
            int hdr;
            int min;
            int max;
            int gprec;
            bits.ReadInt32(out ver);
            bits.ReadInt32(out hdr);
            bits.ReadInt32(out min);
            bits.ReadInt32(out max);
            bits.ReadInt32(out gprec);
            Console.WriteLine("   ** TPI ver={0}, hdr={1}, min={2}, max={3}, gprec={4}",
                              ver, hdr, min, max, gprec);

            // Read the hash structure.
            short stream;
            short padStream;
            int key;
            int buckets;
            int vals;
            int pairs;
            int adj;
            bits.ReadInt16(out stream);
            bits.ReadInt16(out padStream);
            bits.ReadInt32(out key);
            bits.ReadInt32(out buckets);
            bits.ReadInt32(out vals);
            bits.ReadInt32(out pairs);
            bits.ReadInt32(out adj);
            Console.WriteLine("      stream={0}, padstream={1}, key={2}, bux={3}, vals={4}, par={5}, adj={6}",
                              stream, padStream, key, buckets, vals, pairs, adj);
            tiHashStream = stream;
            tiHPadStream = padStream;

            int u1;
            int u2;
            int u3;
            bits.ReadInt32(out u1);
            bits.ReadInt32(out u2);
            bits.ReadInt32(out u3);

            Console.WriteLine("      pos={0}, u1={1}, u2={2}, u3={3}",
                              bits.Position, u1, u2, u3);

#if true
            int end = hdr + gprec;
            while (bits.Position < end) {
                ushort cbrec;
                ushort leaf;
                bits.ReadUInt16(out cbrec);
                bits.ReadUInt16(out leaf);
                Console.WriteLine("        [{0,4}] : {1}", cbrec, (LEAF)leaf);
                Dump(bits.Buffer, bits.Position, bits.Position + cbrec - 2);
                bits.Position += cbrec - 2;

                // [GalenH]: Need to figure out what RECs are for.
            }
#endif
        }

        static void DumpCvInfo(BitAccess bits, int begin, int limit)
        {
            int indent = 0;
            string pad = "";
            while (bits.Position < limit) {
                ushort siz;
                ushort rec;

                bits.ReadUInt16(out siz);
                int star = bits.Position;
                int stop = bits.Position + siz;
#if false
                Dump(bits.GetBuffer(), star, stop);
                bits.Position = star;
#endif
                bits.ReadUInt16(out rec);


                SYM cv = (SYM)rec;
                if (rec < 0x1000) {
                    if (cv != SYM.S_END &&
                        cv != SYM.S_OEM) {
                        throw new Exception("CV is unknown: " + rec);
                    }
                }

                switch (cv) {

                    case SYM.S_OEM: {          // 0x0404
                        OemSymbol oem;

                        bits.ReadGuid(out oem.idOem);
                        bits.ReadUInt32(out oem.typind);
                        // public byte[]   rgl;        // user data, force 4-byte alignment

                        if (oem.idOem == PdbFunction.msilMetaData) {
                            Console.WriteLine("        {0}META: ", pad);

                            Dump(bits.Buffer, star + 22, stop);
                            break;
                        }
                        else {
                            Console.WriteLine("        {0}OEMS: guid={1} ti={2}",
                                              pad, oem.idOem, oem.typind);
                            Dump(bits.Buffer, star + 22, stop);
                        }

                        break;
                    }

                    case SYM.S_OBJNAME: {      // 0x1101
                        ObjNameSym obj;

                        bits.ReadUInt32(out obj.signature);
                        bits.ReadCString(out obj.name);

                        Console.WriteLine("        {0}OBJN: sig={1:x8} [{2}]",
                                          pad, obj.signature, obj.name);
                        break;
                    }

                    case SYM.S_FRAMEPROC: {    // 0x1012
                        FrameProcSym frame;

                        bits.ReadUInt32(out frame.cbFrame);
                        bits.ReadUInt32(out frame.cbPad);
                        bits.ReadUInt32(out frame.offPad);
                        bits.ReadUInt32(out frame.cbSaveRegs);
                        bits.ReadUInt32(out frame.offExHdlr);
                        bits.ReadUInt16(out frame.secExHdlr);
                        bits.ReadUInt32(out frame.flags);

                        Console.WriteLine("        {0}FRAM: size={1}, pad={2}+{3}, exc={4:x4}:{5:x8}, flags={6:x3}",
                                          pad, frame.cbFrame, frame.cbPad, frame.offPad,
                                          frame.offExHdlr, frame.secExHdlr,
                                          frame.flags);
                        break;
                    }

                    case SYM.S_BLOCK32: {      // 0x1103
                        BlockSym32 block;

                        bits.ReadUInt32(out block.parent);
                        bits.ReadUInt32(out block.end);
                        bits.ReadUInt32(out block.len);
                        bits.ReadUInt32(out block.off);
                        bits.ReadUInt16(out block.seg);
                        bits.ReadCString(out block.name);

                        Console.WriteLine("        {0}BLCK: par={1}, addr={2:x4}:{3:x8} len={4:x4} [{5}], end={6}",
                                          pad, block.parent, block.seg, block.off,
                                          block.len, block.name, block.end);
                        indent++;
                        pad = new String('.', indent);
                        break;
                    }

                    case SYM.S_COMPILE2: {     // 0x1116
                        CompileSym com;

                        bits.ReadUInt32(out com.flags);
                        bits.ReadUInt16(out com.machine);
                        bits.ReadUInt16(out com.verFEMajor);
                        bits.ReadUInt16(out com.verFEMinor);
                        bits.ReadUInt16(out com.verFEBuild);
                        bits.ReadUInt16(out com.verMajor);
                        bits.ReadUInt16(out com.verMinor);
                        bits.ReadUInt16(out com.verBuild);
                        bits.ReadCString(out com.verSt);

                        Console.WriteLine("        {0}COMP: flg={1:x4} mach={2:x2} [{3}] {4}.{5}.{6}.{7}.{8}.{9}",
                                          pad,
                                          com.flags, com.machine, com.verSt,
                                          com.verFEMajor, com.verFEMinor, com.verFEBuild,
                                          com.verMajor, com.verMinor, com.verBuild);
                        break;
                    }

                    case SYM.S_BPREL32: {      // 0x110b
                        BpRelSym32 bp;

                        bits.ReadInt32(out bp.off);
                        bits.ReadUInt32(out bp.typind);
                        bits.ReadCString(out bp.name);
                        Console.WriteLine("        {0}BPRL: ti={1:x8} [{2}] off={3,6}",
                                          pad, bp.typind, bp.name, bp.off);
                        break;
                    }

                    case SYM.S_LPROC32:        // 0x110f
                    case SYM.S_GPROC32: {      // 0x1110
                        ProcSym32 proc;

                        bits.ReadUInt32(out proc.parent);
                        bits.ReadUInt32(out proc.end);
                        bits.ReadUInt32(out proc.next);
                        bits.ReadUInt32(out proc.len);
                        bits.ReadUInt32(out proc.dbgStart);
                        bits.ReadUInt32(out proc.dbgEnd);
                        bits.ReadUInt32(out proc.typind);
                        bits.ReadUInt32(out proc.off);
                        bits.ReadUInt16(out proc.seg);
                        bits.ReadUInt8(out proc.flags);
                        bits.ReadCString(out proc.name);

                        Console.WriteLine("        {0}PROC: ti={1,5} [{2}] addr={3:x4}:{4:x8} len={5} f={6:x4}, end={7}",
                                          pad, proc.typind, proc.name,
                                          proc.seg, proc.off, proc.len, proc.flags,
                                          proc.end);
                        if (proc.parent != 0 || proc.next != 0) {
                            Console.WriteLine("                 !!! Warning parent={0}, next={1}",
                                              proc.parent, proc.next);
                        }
                        indent++;
                        pad = new String('.',  indent);
                        break;
                    }

                    case SYM.S_MANSLOT: {      // 0x1120
                        AttrSlotSym slot;

                        bits.ReadUInt32(out slot.index);
                        bits.ReadUInt32(out slot.typind);
                        bits.ReadUInt32(out slot.offCod);
                        bits.ReadUInt16(out slot.segCod);
                        bits.ReadUInt16(out slot.flags);
                        bits.ReadCString(out slot.name);

                        Console.WriteLine("        {0}SLOT: ti={1:x8} [{2}] slot={3} flg={4:x4}",
                                          pad, slot.typind, slot.name, slot.index, slot.flags);
                        if (slot.segCod != 0 || slot.offCod != 0) {
                            Console.WriteLine("            !!! Warning: addr={0:x4}:{1:x8}",
                                              slot.segCod, slot.offCod);
                        }
                        break;
                    }

                    case SYM.S_UNAMESPACE: {   // 0x1124
                        UnamespaceSym ns;
                        bits.ReadCString(out ns.name);
                        Console.WriteLine("        {0}NAME: using [{1}]", pad, ns.name);
                        break;
                    }

                    case SYM.S_GMANPROC:        // 0x112a
                    case SYM.S_LMANPROC: {      // 0x112b
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
                        bits.ReadCString(out proc.name);

                        Console.WriteLine("        {0}PROC: token={1:x8} [{2}] addr={3:x4}:{4:x8} len={5:x4} f={6:x4}, end={7}",
                                          pad, proc.token, proc.name,
                                          proc.seg, proc.off, proc.len, proc.flags, proc.end);
                        if (proc.parent != 0 || proc.next != 0) {
                            Console.WriteLine("            !!! Warning par={0}, pnext={1}",
                                              proc.parent, proc.next);
                        }
                        if (proc.dbgStart != 0 || proc.dbgEnd != 0) {
                            Console.WriteLine("            !!! Warning DBG start={0}, end={1}",
                                              proc.dbgStart, proc.dbgEnd);
                        }
                        indent++;
                        pad = new String('.',  indent);
                        break;
                    }

                    case SYM.S_END: {           // 0x0006
                        indent--;
                        pad = new String('.',  indent);
                        Console.WriteLine("        {0}END {1}", pad, bits.Position - 4);
                        break;
                    }

                    case SYM.S_SECTION: {               // 0x1136
                        SectionSym sect;

                        bits.ReadUInt16(out sect.isec);
                        bits.ReadUInt8(out sect.align);
                        bits.ReadUInt8(out sect.bReserved);
                        bits.ReadUInt32(out sect.rva);
                        bits.ReadUInt32(out sect.cb);
                        bits.ReadUInt32(out sect.characteristics);
                        bits.ReadCString(out sect.name);

                        Console.WriteLine("        {0}SECT: sec={1,4} align={2}, flags={3:x8} [{4}]",
                                          pad, sect.isec, sect.align,
                                          sect.characteristics, sect.name);
                        break;
                    }

                    case SYM.S_COFFGROUP: {             // 0x1137
                        CoffGroupSym group;

                        bits.ReadUInt32(out group.cb);
                        bits.ReadUInt32(out group.characteristics);
                        bits.ReadUInt32(out group.off);
                        bits.ReadUInt16(out group.seg);
                        bits.ReadCString(out group.name);

                        Console.WriteLine("        {0}CGRP: flags={1:x8} [{2}] addr={3:x4}:{4:x8}",
                                          pad, group.characteristics,
                                          group.name, group.seg, group.off);
                        break;
                    }

                    case SYM.S_THUNK32: {               // 0x1102
                        ThunkSym32 thunk;

                        bits.ReadUInt32(out thunk.parent);
                        bits.ReadUInt32(out thunk.end);
                        bits.ReadUInt32(out thunk.next);
                        bits.ReadUInt32(out thunk.off);
                        bits.ReadUInt16(out thunk.seg);
                        bits.ReadUInt16(out thunk.len);
                        bits.ReadUInt8(out thunk.ord);
                        bits.ReadCString(out thunk.name);

                        Console.WriteLine("        {0}THNK: addr={1:x4}:{2:x8} [{3}], end={4}",
                                          pad, thunk.seg, thunk.off, thunk.name, thunk.end);
                        indent++;
                        pad = new String('.', indent);
                        break;
                    }

                    default: {
                        Console.WriteLine("        {0}{1}:", pad, cv);
                        Dump(bits.Buffer, star + 2, stop);
                        break;
                    }
                }

                bits.Position = stop;
            }
            if (indent != 0) {
                throw new Exception("indent isn't 0.");
            }
        }

        static void DumpSymbols(BitAccess bits, int begin, int limit)
        {
            // Dump the symbols.
            Console.WriteLine("      Symbols:");
            bits.Position = begin;
            int sig;
            bits.ReadInt32(out sig);
            if (sig != 4) {
                throw new Exception("Invalid signature.");
            }

            DumpCvInfo(bits, bits.Position, limit);
        }

        static void DumpLines(BitAccess bits,
                              int begin,
                              int limit)
        {
            uint lastAddr = 0;

            // Read the files first
            Console.WriteLine("      Lines:");
            bits.Position = begin;
            while (bits.Position < limit) {
                int sig;
                int siz;
                bits.ReadInt32(out sig);
                bits.ReadInt32(out siz);
                int place = bits.Position;
                int endSym = bits.Position + siz;

                switch ((DEBUG_S_SUBSECTION)sig) {
                    case DEBUG_S_SUBSECTION.FILECHKSMS:
                        int beg = bits.Position;
                        while (bits.Position < endSym) {
                            CV_FileCheckSum chk;
                            int nif = bits.Position - beg;
                            bits.ReadUInt32(out chk.name);
                            bits.ReadUInt8(out chk.len);
                            bits.ReadUInt8(out chk.type);

                            int where = bits.Position;
                            bits.Position += chk.len;
                            bits.Align(4);
                            Console.WriteLine("            nif={0,4}, ni={1,5}, type={2:x2}, len={3}",
                                              nif, chk.name, chk.type, chk.len);
                            Dump(bits.Buffer, where, where + chk.len);
                        }
                        bits.Position = endSym;
                        break;

                    case DEBUG_S_SUBSECTION.LINES:
                        bits.Position = endSym;
                        break;

                    default:
                        Console.WriteLine("            ??? {0}", (DEBUG_S_SUBSECTION)sig);
                        Dump(bits.Buffer, bits.Position, bits.Position + siz);
                        bits.Position = endSym;
                        break;
                }
            }

            // Read the lines next.
            bits.Position = begin;
            while (bits.Position < limit) {
                int sig;
                int siz;
                bits.ReadInt32(out sig);
                bits.ReadInt32(out siz);
                int endSym = bits.Position + siz;

                switch ((DEBUG_S_SUBSECTION)sig) {
                    case DEBUG_S_SUBSECTION.LINES: {
                        CV_LineSection sec;

                        bits.ReadUInt32(out sec.off);
                        bits.ReadUInt16(out sec.sec);
                        bits.ReadUInt16(out sec.flags);
                        bits.ReadUInt32(out sec.cod);
                        Console.WriteLine("          addr={0:x4}:{1:x8}, flg={2:x4}, cod={3,8}",
                                          sec.sec, sec.off, sec.flags, sec.cod);
                        if (sec.off < lastAddr) {
                            throw new PdbDebugException("address {0} follows {1}", sec.off, lastAddr);
                        }
                        else if (sec.off > lastAddr) {
                            lastAddr = sec.off;
                        }

                        while (bits.Position < endSym) {
                            CV_SourceFile file;
                            bits.ReadUInt32(out file.index);
                            bits.ReadUInt32(out file.count);
                            bits.ReadUInt32(out file.linsiz);   // Size of payload.
                            Console.WriteLine("            nif={0,4}, cnt={1,4}",
                                              file.index, file.count);

                            int plin = bits.Position;
                            int pcol = bits.Position + 8 * (int)file.count;

                            //Dump(bits.Buffer, bits.Position, bits.Position + file.linsiz);
                            for (int i = 0; i < file.count; i++) {
                                CV_Line line;
                                CV_Column column = new CV_Column();

                                bits.Position = plin + 8 * i;
                                bits.ReadUInt32(out line.offset);
                                bits.ReadUInt32(out line.flags);

                                uint delta = (line.flags & 0x7f000000) >> 24;
                                bool statement = ((line.flags & 0x80000000) == 0);
                                if ((sec.flags & 1) != 0) {
                                    bits.Position = pcol + 4 * i;
                                    bits.ReadUInt16(out column.offColumnStart);
                                    bits.ReadUInt16(out column.offColumnEnd);
                                }
                                Console.WriteLine("              pc={0:x8} # {1,8}.{2,2}.{3,2}",
                                                  line.offset,
                                                  line.flags & 0xffffff,
                                                  column.offColumnStart,
                                                  column.offColumnEnd);
                            }
                        }
                        break;
                    }
                }
                bits.Position = endSym;
            }
        }

        static void DumpDbiModule(BitAccess bits,
                                  DbiModuleInfo info)
        {
            Console.WriteLine("   ** Module [{0}]", info.moduleName);
            DumpSymbols(bits, 0, info.cbSyms);
            DumpLines(bits,
                      info.cbSyms + info.cbOldLines,
                      info.cbSyms + info.cbOldLines + info.cbLines);
        }

        static void DumpDbiStream(BitAccess bits,
                                  out int globalsStream,
                                  out int publicsStream,
                                  out int symbolsStream,
                                  out DbiModuleInfo[] modules)
        {
            globalsStream = 0;
            publicsStream = 0;
            symbolsStream = 0;
            DbiHeader dh = new DbiHeader(bits);

            Console.WriteLine("   ** DBI sig={0}, ver={1}, age={2}, vers={3:x4}, "+
                              "pdb={4}, pdb2={5}",
                              dh.sig, dh.ver, dh.age, dh.vers, dh.pdbver, dh.pdbver2);
            if (dh.sig != -1 || dh.ver != 19990903) {
                throw new IOException("Unknown DBI Stream version");
            }

            Console.WriteLine("      mach={0:x4}, flags={1:x4}, globals={2}, publics={3}, symbols={4}",
                              dh.machine,
                              dh.flags,
                              dh.gssymStream,
                              dh.pssymStream,
                              dh.symrecStream);
            Console.WriteLine("      gpmod={0}, seccon={1}, secmap={2}, filinf={3}, ",
                              dh.gpmodiSize,
                              dh.secconSize,
                              dh.secmapSize,
                              dh.filinfSize);
            Console.WriteLine("      tsmap={0}, dbghdr={1}, ecinf={2}, mfc={3}",
                              dh.tsmapSize,
                              dh.dbghdrSize,
                              dh.ecinfoSize,
                              dh.mfcIndex);
            Console.WriteLine("      sizes={0}",
                              bits.Position +
                              dh.gpmodiSize +
                              dh.secconSize +
                              dh.secmapSize +
                              dh.filinfSize +
                              dh.tsmapSize +
                              dh.dbghdrSize +
                              dh.ecinfoSize);

            globalsStream = dh.gssymStream;
            publicsStream = dh.pssymStream;
            symbolsStream = dh.symrecStream;

            // Read gpmod section.
            ArrayList modList = new ArrayList();
            int end = bits.Position + dh.gpmodiSize;
            while (bits.Position < end) {
                DbiModuleInfo mod = new DbiModuleInfo(bits, true);
                Dump(mod);

                modList.Add(mod);
            }
            if (bits.Position != end) {
                throw new Exception("off!");
            }

            // Read seccon section.
            end = bits.Position + dh.secconSize;
            Console.WriteLine("    SecCon:");
            Dump(bits.Buffer, bits.Position, end);
            bits.Position = end;

            // Read secmap section.
            end = bits.Position + dh.secmapSize;
            Console.WriteLine("    SecMap:");
            Dump(bits.Buffer, bits.Position, end);
            bits.Position = end;

            // Read filinf section.
            end = bits.Position + dh.filinfSize;
            Console.WriteLine("    FilInf:");
            Dump(bits.Buffer, bits.Position, end);
            bits.Position = end;

            // Read tsmap section.
            end = bits.Position + dh.tsmapSize;
            Console.WriteLine("    TSMap:");
            Dump(bits.Buffer, bits.Position, end);
            bits.Position = end;

            // Read ecinfo section.
            end = bits.Position + dh.ecinfoSize;
            Console.WriteLine("    ECInfo:");
            Dump(bits.Buffer, bits.Position, end);
            DumpNameStream(bits);
            bits.Position = end;

            // Read dbghdr section.
            end = bits.Position + dh.dbghdrSize;
            Console.WriteLine("    DbgHdr:");
            if (dh.dbghdrSize > 0) {
                int beg = bits.Position;
                Dump(bits.Buffer, bits.Position, end);
                bits.Position = beg;
                DbiDbgHdr ddh = new DbiDbgHdr(bits);
                Console.WriteLine("      sechdr={0}, ridmap={1}",
                                  ddh.snSectionHdr,
                                  ddh.snTokenRidMap);
            }
            bits.Position = end;

            if (modList.Count > 0) {
                modules = (DbiModuleInfo[])modList.ToArray(typeof(DbiModuleInfo));
            }
            else {
                modules = null;
            }
        }

        static void DumpFile(Stream read, bool verbose)
        {
            BitAccess bits = new BitAccess(4096);
            PdbFileHeader head = new PdbFileHeader(read, bits);
            PdbReader reader = new PdbReader(read, head.pageSize);
            MsfDirectory dir = new MsfDirectory(reader, head, bits);

            Console.WriteLine("  PDB Processing[{0}]:", head.Magic);
            Console.WriteLine("    cbPage: {0}, fmap: {1}, pages: {2}, dirs: {3}, streams: {4}",
                              head.pageSize,
                              head.freePageMap,
                              head.pagesUsed,
                              head.directoryRoot,
                              dir.streams.Length);

            byte[] buffer = new byte[head.pageSize];

            int linkStream = 0;
            int nameStream = 0;
            int srchStream = 0;
            int tiHashStream = 0;
            int tiHPadStream = 0;
            int globalsStream = 0;
            int publicsStream = 0;
            int symbolsStream = 0;
            DbiModuleInfo[] modules = null;

            for (int i = 0; i < dir.streams.Length; i++) {
                if (dir.streams[i].Length <= 0) {
                    Console.WriteLine("{0,4}:{1,7} bytes", i, dir.streams[i].Length);
                }
                else {
                    Console.Write("{0,4}:{1,7} bytes {2,5}:",
                                  i,
                                  dir.streams[i].Length,
                                  dir.streams[i].GetPage(0));

                    int todo = 16;
                    if (todo > dir.streams[i].Length) {
                        todo = dir.streams[i].Length;
                    }
                    dir.streams[i].Read(reader, 0, buffer, 0, todo);

                    DumpLine(buffer, 0, todo);
                    Console.WriteLine();


                    DbiModuleInfo module = null;
                    if (modules != null) {
                        for (int m = 0; m < modules.Length; m++) {
                            if (modules[m].stream == i) {
                                module = modules[m];
                                break;
                            }
                        }
                    }

                    if (i == 1) {                   // <pdb>
                        dir.streams[i].Read(reader, bits);
                        DumpPdbStream(bits, out linkStream, out nameStream, out srchStream);
                        Console.WriteLine();
                    }
                    else if (i == 2) {
                        dir.streams[i].Read(reader, bits);
                        DumpTpiStream(bits, out tiHashStream, out tiHPadStream);
                        Console.WriteLine();
                    }
                    else if (i == 3) {
                        dir.streams[i].Read(reader, bits);
                        DumpDbiStream(bits,
                                      out globalsStream,
                                      out publicsStream,
                                      out symbolsStream,
                                      out modules);
                        Console.WriteLine();
                    }
                    else if (linkStream > 0 && i == linkStream) {
                        Console.WriteLine("   ** LNK");
                        if (verbose) {
                            DumpVerbose(dir.streams[i], reader);
                        }
                        Console.WriteLine();
                    }
                    else if (nameStream > 0 && i == nameStream) {
                        dir.streams[i].Read(reader, bits);
                        DumpNameStream(bits);
                        Console.WriteLine();
                    }
                    else if (srchStream > 0 && i == srchStream) {
                        Console.WriteLine("   ** SRC");
                        if (verbose) {
                            DumpVerbose(dir.streams[i], reader);
                        }
                        Console.WriteLine();
                    }
                    else if (tiHashStream > 0 && i == tiHashStream) {
                        Console.WriteLine("   ** TI#");
                        if (verbose) {
                            DumpVerbose(dir.streams[i], reader);
                        }
                        Console.WriteLine();
                    }
                    else if (tiHPadStream > 0 && i == tiHPadStream) {
                        Console.WriteLine("   ** TI#PAD");
                        if (verbose) {
                            DumpVerbose(dir.streams[i], reader);
                        }
                        Console.WriteLine();
                    }
                    else if (globalsStream > 0 && i == globalsStream) {
                        Console.WriteLine("   ** GLOBALS");
                        if (verbose) {
                            DumpVerbose(dir.streams[i], reader);
                        }
                        Console.WriteLine();
                    }
                    else if (publicsStream > 0 && i == publicsStream) {
                        Console.WriteLine("   ** PUBLICS");
                        if (verbose) {
                            DumpVerbose(dir.streams[i], reader);
                        }
                        Console.WriteLine();
                    }
                    else if (symbolsStream > 0 && i == symbolsStream) {
                        Console.WriteLine("   ** SYMBOLS");
                        if (verbose) {
                            dir.streams[i].Read(reader, bits);
                            DumpCvInfo(bits, 0, dir.streams[i].Length);
                        }
                        Console.WriteLine();
                    }
                    else if (module != null) {
                        dir.streams[i].Read(reader, bits);
                        DumpDbiModule(bits, module);
                        Console.WriteLine();
                    }
                    else if (verbose) {
                        DumpVerbose(dir.streams[i], reader);
                        Console.WriteLine();
                    }

                }
                if (i == 0) {
                    Console.WriteLine("+------------------------------------------------------------------------------");
                }
            }
        }

        static void CopyFile(Stream read, PdbWriter writer)
        {
            BitAccess bits = new BitAccess(4096);

            // Read the header and directory from the old file.
            // System.Diagnostics.Debugger.Break();
            PdbFileHeader head = new PdbFileHeader(read, bits);
            PdbReader reader = new PdbReader(read, head.pageSize);
            MsfDirectory dir = new MsfDirectory(reader, head, bits);

            byte[] buffer = new byte[head.pageSize];

            // Copy the streams.
            DataStream[] streams = new DataStream [dir.streams.Length];
            for (int i = 0; i < dir.streams.Length; i++) {
                streams[i] = new DataStream();

                DataStream source = dir.streams[i];
                if (source.Length <= 0) {
                    continue;
                }

                int left = source.Length;
                int pos = 0;

                for (int page = 0; left > 0; page++) {
                    int todo = buffer.Length;
                    if (todo > left) {
                        todo = left;
                    }

                    dir.streams[i].Read(reader, pos, buffer, 0, todo);
                    streams[i].Write(writer, buffer, todo);

                    left -= todo;
                    pos += todo;
                }
            }

            writer.WriteMeta(streams, bits);
        }

        static void SplitStreams(Stream read, string split)
        {
            BitAccess bits = new BitAccess(4096);

            // Read the header and directory from the old file.
            // System.Diagnostics.Debugger.Break();
            PdbFileHeader head = new PdbFileHeader(read, bits);
            PdbReader reader = new PdbReader(read, head.pageSize);
            MsfDirectory dir = new MsfDirectory(reader, head, bits);

            byte[] buffer = new byte[head.pageSize];

            // Copy the streams.
            DataStream[] streams = new DataStream [dir.streams.Length];
            for (int i = 0; i < dir.streams.Length; i++) {
                streams[i] = new DataStream();

                DataStream source = dir.streams[i];
                if (source.Length <= 0) {
                    continue;
                }

                string name = String.Format("{0}.{1:d4}", split, i);
                Console.WriteLine("{0,4}: --> {1}", i, name);

                FileStream writer = new FileStream(name,
                                                   FileMode.Create,
                                                   FileAccess.Write);

                int left = source.Length;
                int pos = 0;

                for (int page = 0; left > 0; page++) {
                    int todo = buffer.Length;
                    if (todo > left) {
                        todo = left;
                    }

                    dir.streams[i].Read(reader, pos, buffer, 0, todo);
                    writer.Write(buffer, 0, todo);

                    left -= todo;
                    pos += todo;
                }
                writer.Close();
            }
        }

        private static void Usage()
        {
            Console.WriteLine("Usage:\n" +
                              "    DumpMisc {options} [pdbs]\n" +
                              "Options:\n" +
                              "    /a          Dump all information.\n" +
                              "    /o:dest     Copy content into new file..\n" +
                              "    /s          Split file into streams.\n" +
                              "    /v          Verbose output.\n" +
                              "");
        }

        static int Main(string[] args)
        {
            bool good = false;
            bool split = false;
            bool verbose = false;
            bool all = false;

            FileStream dest = null;

            if (args.Length == 0) {
                Usage();
                return 1;
            }

            for (int i = 0; i < args.Length; i++) {
                string arg = args[i];

                if (arg.Length >= 2 && (arg[0] == '-' || arg[0] == '/')) {
                    string name = null;
                    string value = null;

                    int n = arg.IndexOf(':');

                    if (n > -1) {
                        name = arg.Substring(1, n - 1).ToLower();

                        if (n < arg.Length + 1) {
                            value = arg.Substring(n + 1);
                        }
                    }
                    else {
                        name = arg.Substring(1).ToLower();
                    }

                    bool badArg = false;

                    switch (name) {

                        case "a":
                        case "all":
                            all = true;
                            break;

                        case "o":
                        case "out":
                            dest = new FileStream(value,
                                                  FileMode.Create,
                                                  FileAccess.Write);
                            break;

                        case "s":
                        case "split":
                            split = true;
                            break;

                        case "v":
                        case "verbose":
                            verbose = true;
                            break;

                        default :
                            badArg = true;
                            break;
                    }

                    if (badArg) {
                        Console.WriteLine("Malformed argument: \"{0}\"", arg);
                        Usage();
                        return 1;
                    }
                }
                else {
                    try {
                        string path = Path.GetDirectoryName(arg);
                        string match = Path.GetFileName(arg);

                        if (path == null || path == String.Empty) {
                            path = ".";
                        }
                        Console.WriteLine("[{0}] [{1}]", path, match);
                        String[] files = Directory.GetFiles(path, match);
                        if (files == null) {
                            Console.WriteLine("Could not find: {0}", arg);
                            return 2;
                        }

                        if (files != null) {
                            foreach (string file in files) {
                                try {
                                    FileStream stream = new FileStream(file,
                                                                       FileMode.Open,
                                                                       FileAccess.Read);
                                    if (split) {
                                        SplitStreams(stream, file);
                                    }
                                    else if (dest != null) {
                                        CopyFile(stream, new PdbWriter(dest, 2048));
                                    }
                                    else if (all) {
                                        DumpFile(stream, verbose);
                                    }
                                    else {
                                        Console.WriteLine("{0}:", file);
                                        BitAccess bits = new BitAccess(512 * 1024);
                                        PdbFunction[] funcs = PdbFile.LoadFunctions(stream,
                                                                                    bits,
                                                                                    true);

                                        Console.WriteLine("  {0} functions", funcs.Length);
#if true

                                        for (int f = 0; f < funcs.Length; f++) {
                                            if (verbose) {
                                                Dump(funcs[f], 0);
                                            }
                                            else {
                                                Console.WriteLine("    {0:x4}:{1:x8} {2:x8} {3}",
                                                                  funcs[f].segment,
                                                                  funcs[f].address,
                                                                  funcs[f].token,
                                                                  funcs[f].name);
                                            }
                                        }
#endif
                                        GC.Collect();
                                    }
                                    good = true;
                                }
                                catch (IOException e) {
                                    Console.Error.WriteLine("{0}: I/O Failure: {1}",
                                                            file, e.Message);
                                    good = false;
                                }
                            }
                        }
                    }
                    catch (IOException e) {
                        Console.Error.WriteLine("{0}: I/O Failure: {1}",
                                                arg, e.Message);
                        good = false;
                    }
                }
            }
            if (!good) {
                return 1;
            }
            return 0;
        }
    }
}
#endif