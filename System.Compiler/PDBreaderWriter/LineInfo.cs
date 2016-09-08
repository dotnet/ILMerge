#if UseSingularityPDB

///////////////////////////////////////////////////////////////////////////////
//
//  Microsoft Research Singularity PDB Info Library
//
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//
//  File:   LineInfo.cs
//
//  Program to read and output line information using the PdbInfo Library.
//
using System;
using System.IO;

using Microsoft.Singularity.PdbInfo;

namespace Microsoft.Singularity.Applications
{
    public class LineInfo
    {
        public static void Dump(PdbFunction f, int indent)
        {
            string pad = new String(' ', indent);
            Console.WriteLine("            {0}Func: [{1}.{2}] addr={3:x4}:{4:x8} len={5:x4} token={6:x8}",
                              pad,
                              StripNamespace(f.module), f.name,
                              f.segment, f.address,
                              f.length, f.token);
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

        public static string StripNamespace(string module)
        {
            int li = module.LastIndexOf('.');
            if (li > 0) {
                return module.Substring(li + 1);
            }
            return module;
        }

        private static void Usage()
        {
            Console.WriteLine("Usage:\n" +
                              "    LineInfo {options} [pdbs]\n" +
                              "Options:\n" +
                              "    /q          Quiet output.\n" +
                              "");
        }

        static int Main(string[] args)
        {
            bool good = false;
            bool quiet = false;

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

                        case "q":
                        case "quiet":
                            quiet = true;
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
                                    Console.WriteLine("{0}:", file);

                                    GC.Collect();
                                    GC.Collect();
                                    GC.Collect();
                                    GC.Collect();
                                    GC.Collect();
                                    long gcBefore = GC.GetTotalMemory(true);

                                    PdbFunction[] funcs = PdbFile.LoadFunctions(stream,
                                                                                true);


                                    long gcAfter = GC.GetTotalMemory(false);
#if false
                                    bits = null;
                                    head = null;
                                    reader = null;
                                    dir = null;         // 50KB -- pages for streams.
                                    modules = null;     // 40KB --
                                    names = null;       // 50KB -- lots of names.
                                    funcList = null;    // 40KB -- lots of functions.
#endif
                                    long gcClean = GC.GetTotalMemory(false);
                                    long gcFinal = GC.GetTotalMemory(true);

                                    Console.WriteLine(" gcBefore = {0,12}", gcBefore);
                                    Console.WriteLine(" gcAfter =  {0,12}", gcAfter);
                                    Console.WriteLine(" gcClean =  {0,12}", gcClean);
                                    Console.WriteLine(" gcFinal =  {0,12} => used={1,12} temp=[{2,13}]",
                                                      gcFinal,
                                                      gcFinal - gcBefore,
                                                      gcClean - gcFinal);

                                    Console.WriteLine("  {0} functions", funcs.Length);
#if true

                                    for (int f = 0; f < funcs.Length; f++) {
                                        if (quiet) {
                                            Console.WriteLine("    {0:x4}:{1:x8} token={2:X8} [{3}::{4}]",
                                                              funcs[f].segment,
                                                              funcs[f].address,
                                                              funcs[f].token,
                                                              StripNamespace(funcs[f].module),
                                                              funcs[f].name);
                                        }
                                        else {
                                            Dump(funcs[f], 0);
                                        }
                                    }
#endif
                                    GC.Collect();
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