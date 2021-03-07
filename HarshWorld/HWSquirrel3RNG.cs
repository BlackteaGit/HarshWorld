/*
   squirrel3
   ~~~~~~~~~
   Provides a random genarator based on noise function which is fast and has good distribution. It was
   introduced by Squirrel Eiserloh at 'Math for Game Programmers: Noise-based
   RNG', GDC17.

 */


using System;

namespace HarshWorld
{
    public static class Squirrel3RNG
    {
        private static uint m_seed;
        private static int m_position;

        static Squirrel3RNG()
        {
            m_seed = (uint)(Environment.TickCount + System.Diagnostics.Process.GetCurrentProcess().Id);
            m_position = 0;
        }

        /*
        public SquirrelRNG(uint seed)
        {
            m_seed = seed;
            m_position = 0;
        }
        */

        //SRand-like (seed-related) methods
        public static void ResetSeed() { m_seed = (uint)(Environment.TickCount + System.Diagnostics.Process.GetCurrentProcess().Id); m_position = 0; }
        public static void ResetSeed(uint seed, int position = 0) { m_seed = seed; m_position = position; }
        public static uint GetSeed() { return m_seed; }
        public static void SetCurrentPosition(int position) { m_position = position; }
        public static int GetCurrentPosition() { return m_position; }


        public static uint Next()
        {
            var position = m_position;
            if (m_position < Int32.MaxValue)
            {
                m_position += 1;
            }
            else
            {
                SetCurrentPosition(Int32.MinValue);
            }
            return Get1dNoiseUint(position, m_seed);
        }

        public static int Next(int maxValueNotInclusiveInt)
        {
            uint maxValueNotInclusive = unchecked((uint)maxValueNotInclusiveInt);
            var position = m_position;
            if (m_position < Int32.MaxValue)
            {
                m_position += 1;
            }
            else
            {
                SetCurrentPosition(Int32.MinValue);
            }
            return (int)(Get1dNoiseInt(position, m_seed) * (1.0 / Int32.MaxValue) * (double)maxValueNotInclusive);
        }

        public static int Next(int minValue, int maxValue)
        {
            if (maxValue < minValue)
            {
                //swap min and max
                int temp = minValue;
                minValue = maxValue;
                maxValue = temp;
            }
            return minValue + Next(maxValue - minValue);
        }

        public static double NextDouble()
        {
            var position = m_position;
            if (m_position < Int32.MaxValue)
            {
                m_position += 1;
            }
            else
            {
                SetCurrentPosition(Int32.MinValue);
            }
            return Get1dNoiseInt(position, m_seed) * (1.0 / Int32.MaxValue);
        }

        // Rand-like (sequential random rolls) methods: each one advances the RNG to its next position
        /*
        public uint RollRandomUint32();
        public ushort RollRandomUint16();
        public byte RollRandomByte();
        public uint RollRandomIntLessThan(uint maxValueNotInclusive);
        public int RollRandomIntInRange(int minValueInclusive, int maxValueInclusive);
        public float RollRandomFloatZeroToOne();
        public float RollRandomFloatInRange(float minValueInclusive, float maxValueInclusive);
        public bool RollRandomChance(float probabilityOfReturningTrue);
        public void RollRandomDirection2D(ref float out_x, ref float out_y);
        */
        public static uint Get1dNoiseUint(int positionX, uint seed)
        {
            const uint BITNOISE1 = 0x68E31DA4;
            const uint BITNOISE2 = 0xB5297A4D;
            const uint BITNOISE3 = 0x1B56C4E9;

            uint mangledBits = (uint)positionX;
            mangledBits *= BITNOISE1;
            mangledBits += seed;
            mangledBits ^= (mangledBits >> 8);
            mangledBits += BITNOISE2;
            mangledBits ^= (mangledBits << 8);
            mangledBits *= BITNOISE3;
            mangledBits ^= (mangledBits >> 8);
            return mangledBits;
        }

        public static int Get1dNoiseInt(int positionX, uint seed)
        {
            const int BITNOISE1 = 0x68E31DA4;
            const int BITNOISE2 = 0xB5297A4;
            const int BITNOISE3 = 0x1B56C4E9;

            int mangledBits = positionX;
            mangledBits *= BITNOISE1;
            mangledBits += (int)seed;
            mangledBits ^= (mangledBits >> 8);
            mangledBits += BITNOISE2;
            mangledBits ^= (mangledBits << 8);
            mangledBits *= BITNOISE3;
            mangledBits ^= (mangledBits >> 8);
            if (mangledBits == Int32.MaxValue) mangledBits--;
            if (mangledBits < 0) mangledBits += Int32.MaxValue;
            return mangledBits;
        }
    }
}
