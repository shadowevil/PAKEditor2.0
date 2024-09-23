using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAKEditor
{
    public enum Endianness
    {
        LittleEndian,
        BigEndian
    }

    public struct Bit
    {
        private readonly int _value;

        private Bit(int value)
        {
            _value = value;
        }

        public static implicit operator Bit(int value)
        {
            if (value != 0 && value != 1)
                throw new ArgumentOutOfRangeException(nameof(value), "Bit can only be 0 or 1.");

            return new Bit(value);
        }

        public static implicit operator int(Bit bit)
        {
            return bit._value;
        }

        public override string ToString()
        {
            return _value.ToString();
        }
    }

    public static class BitConverter
    {
        public static IEnumerable<Bit> ByteToBits(byte b, Endianness endianness = Endianness.LittleEndian)
        {
            Bit[] bits = new Bit[8];
            for (int i = 0; i < 8; i++)
            {
                if (endianness == Endianness.BigEndian)
                {
                    // For BigEndian, place the MSB at index 0
                    bits[i] = (b >> (7 - i)) & 1;
                }
                else
                {
                    // For LittleEndian, place the LSB at index 0
                    bits[i] = (b >> i) & 1;
                }
            }
            return bits;
        }

        // Converts an array of Bits back to a Byte with specified endianness
        public static byte BitsToByte(IEnumerable<Bit> bits)
        {
            if (bits.Count() != 8)
                throw new ArgumentException("Bit array must contain exactly 8 bits.");

            byte result = 0;
            for (int i = 0; i < 8; i++)
            {
                int bitValue = (int)bits.ElementAt(i);
                result |= (byte)(bitValue << (7 - i));
            }
            return result;
        }

        // Converts an IEnumerable of Bytes to an IEnumerable of Bits with specified endianness
        public static IEnumerable<Bit> BytesToBits(IEnumerable<byte> bytes, Endianness endianness = Endianness.LittleEndian)
        {
            foreach (var b in bytes)
            {
                foreach (var bit in ByteToBits(b, endianness))
                {
                    yield return bit;
                }
            }
        }

        // Converts an IEnumerable of Bits to an IEnumerable of Bytes with specified endianness
        public static IEnumerable<byte> BitsToBytes(IEnumerable<Bit> bits)
        {
            var bitList = bits.ToList();
            if (bitList.Count % 8 != 0)
                throw new ArgumentException("Bit array length must be a multiple of 8.");

            for (int i = 0; i < bitList.Count; i += 8)
            {
                yield return BitsToByte(bitList.Skip(i).Take(8));
            }
        }

        // Converts an IEnumerable of Bits to an IEnumerable of Bytes with specified endianness, with padding if necessary
        public static IEnumerable<byte> BitsToBytesWithPadding(IEnumerable<Bit> bits, Endianness endianness = Endianness.LittleEndian)
        {
            var bitList = bits.ToList();
            int totalBits = bitList.Count;
            int paddingBits = 8 - (totalBits % 8);

            // If the number of bits is not a multiple of 8, we need to pad
            if (paddingBits < 8)
            {
                if (endianness == Endianness.BigEndian)
                {
                    // Pad at the start for BigEndian
                    bitList.InsertRange(0, Enumerable.Repeat((Bit)0, paddingBits));
                }
                else
                {
                    // Pad at the end for LittleEndian
                    bitList.AddRange(Enumerable.Repeat((Bit)0, paddingBits));
                }
            }

            // Process the bits 8 at a time to convert to bytes
            for (int i = 0; i < bitList.Count; i += 8)
            {
                yield return BitsToByte(bitList.Skip(i).Take(8));
            }
        }

        public static string ToString(IEnumerable<byte> bytes)
        {
            return System.BitConverter.ToString(bytes.ToArray());
        }
    }

}
