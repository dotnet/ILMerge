#if UseSingularityPDB

///////////////////////////////////////////////////////////////////////////////
//
//  Microsoft Research Singularity PDB Info Library
//
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//
//  File:   BitSet.cs
//
using System;

namespace Microsoft.Singularity.PdbInfo.Features
{
    public struct BitSet
    {
        public BitSet(BitAccess bits)
        {
            bits.ReadInt32(out size);    // 0..3 : Number of words
            words = new uint[size];
            bits.ReadUInt32(words);
        }

        public BitSet(int size)
        {
            this.size = size;
            words = new uint[size];
        }

        public bool IsSet(int index)
        {
            int word = index / 32;
            return ((words[word] & GetBit(index)) != 0);
        }

        public void Set(int index)
        {
            int word = index / 32;
            words[word] |= GetBit(index);
        }

        public void Clear(int index)
        {
            int word = index / 32;
            words[word] &= ~GetBit(index);
        }

        private uint GetBit(int index)
        {
            return ((uint)1 << (index % 32));
        }

        private uint ReverseBits(uint value)
        {
            uint o = 0;
            for (int i = 0; i < 32; i++) {
                o = (o << 1) | (value & 1);
                value >>= 1;
            }
            return o;
        }

        public bool IsEmpty
        {
            get { return size == 0; }
        }

        public bool GetWord(int index, out uint word)
        {
            if (index < size) {
                word = ReverseBits(words[index]);
                return true;
            }
            word = 0;
            return false;
        }

        private int size;
        private uint[] words;
    }

}
#endif