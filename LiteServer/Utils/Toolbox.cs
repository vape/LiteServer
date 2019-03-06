using System;

namespace LiteServer.Utils
{
    public static class Toolbox
    {
        // Copyright (c) 2008-2013 Hafthor Stefansson
        // Distributed under the MIT/X11 software license
        // Ref: http://www.opensource.org/licenses/mit-license.php.
        /// <summary>
        /// Compares two byte arrays in a fast way
        /// </summary>
        public static unsafe bool UnsafeCompare(byte[] a1, byte[] a2)
        {
            if (a1 == a2) return true;
            if (a1 == null || a2 == null || a1.Length != a2.Length)
                return false;
            fixed (byte* p1 = a1, p2 = a2)
            {
                byte* x1 = p1, x2 = p2;
                int l = a1.Length;
                for (int i = 0; i < l / 8; i++, x1 += 8, x2 += 8)
                    if (*((long*)x1) != *((long*)x2)) return false;
                if ((l & 4) != 0) { if (*((int*)x1) != *((int*)x2)) return false; x1 += 4; x2 += 4; }
                if ((l & 2) != 0) { if (*((short*)x1) != *((short*)x2)) return false; x1 += 2; x2 += 2; }
                if ((l & 1) != 0) if (*((byte*)x1) != *((byte*)x2)) return false;
                return true;
            }
        }

        /// <summary>
        /// Converts System.Guid to java binary representation
        /// </summary>
        /// <param name="binary">Binary representation in java format</param>
        /// <returns>.NET Guid</returns>
        public static Guid ToGuid(this byte[] binary)
        {
            byte[] net = new byte[16];
            for (int i = 8; i < 16; i++)
            {
                net[i] = binary[i];
            }

            net[3] = binary[0];
            net[2] = binary[1];
            net[1] = binary[2];
            net[0] = binary[3];
            net[5] = binary[4];
            net[4] = binary[5];
            net[6] = binary[7];
            net[7] = binary[6];

            return new Guid(net);
        }

        /// <summary>
        /// Converts System.Guid to java binary representation
        /// </summary>
        /// <param name="guid">.NET Guid</param>
        /// <returns>Binary representation</returns>
        public static byte[] ToBytes(this Guid guid)
        {
            byte[] binary = new byte[16];
            byte[] net = guid.ToByteArray();
            for (int i = 8; i < 16; i++)
            {
                binary[i] = net[i];
            }

            binary[0] = net[3];
            binary[1] = net[2];
            binary[2] = net[1];
            binary[3] = net[0];
            binary[4] = net[5];
            binary[5] = net[4];
            binary[6] = net[7];
            binary[7] = net[6];

            return binary;
        }

        private static int GetHexVal(char hex)
        {
            int val = (int)hex;
            return val - (val < 58 ? 48 : 55);
        }
    }
}
