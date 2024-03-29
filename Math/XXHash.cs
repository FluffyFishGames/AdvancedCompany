﻿// Klak - Utilities for creative coding with Unity
//
// Copyright (C) 2016 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedCompany.Math
{
    public struct XXHash
    {
        const uint PRIME32_1 = 2654435761U;
        const uint PRIME32_2 = 2246822519U;
        const uint PRIME32_3 = 3266489917U;
        const uint PRIME32_4 = 668265263U;
        const uint PRIME32_5 = 374761393U;

        static uint rotl32(uint x, int r)
        {
            return (x << r) | (x >> 32 - r);
        }

        public static uint GetHash(int data, int seed)
        {
            uint h32 = (uint)seed + PRIME32_5;
            h32 += 4U;
            h32 += (uint)data * PRIME32_3;
            h32 = rotl32(h32, 17) * PRIME32_4;
            h32 ^= h32 >> 15;
            h32 *= PRIME32_2;
            h32 ^= h32 >> 13;
            h32 *= PRIME32_3;
            h32 ^= h32 >> 16;
            return h32;
        }

        public int Counter;
        public int Seed;

        public XXHash(int seed)
        {
            this.Seed = seed;
        }

        internal uint GetHash(int data)
        {
            return GetHash(data, Seed);
        }

        public float Value
        {
            get
            {
                return (float)GetHash(Counter) / (float)int.MaxValue;
            }
        }

        public int Range(int max)
        {
            Counter++;
            return (int)GetHash(Counter) % max;
        }

        public int Range(int min, int max)
        {
            Counter++;
            return (int)GetHash(Counter) % (max - min) + min;
        }
    }
}