using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAKEditor
{
    public class BitReader
    {
        public byte[] fileBytes { get; private set; }
        public Bit[] bits { get; private set; }
        public uint BitPosition { get; private set; }
        public uint BytePosition { get; private set; }
        public byte[] NextBytes => fileBytes.Skip((int)BytePosition).ToArray();

        public BitReader(string filePath, Endianness endianness = Endianness.BigEndian)
        {
            if(!File.Exists(filePath))
                throw new FileNotFoundException($"{filePath}");

            fileBytes = File.ReadAllBytes(filePath);
            bits = BitConverter.BytesToBits(fileBytes, endianness).ToArray();
            BitPosition = 0;
            BytePosition = 0;
        }

        public void SeekBits(uint position)
        {
            BitPosition = position;
            BytePosition = BitPosition / 8;
        }

        public void SeekBytes(uint position)
        {
            BytePosition = position;
            BitPosition = BytePosition * 8;
        }

        public Bit ReadBit()
        {
            Bit b = bits[BitPosition];
            BitPosition++;
            BytePosition = BitPosition / 8;
            return b;
        }

        public IEnumerable<Bit> ReadBits(uint numBits)
        {
            List<Bit> b = new List<Bit>();
            for(uint i=0;i<numBits;i++)
                b.Add(ReadBit());
            return b;
        }

        public byte ReadByte()
        {
            List<Bit> _bits = new List<Bit>();
            for(int i=0;i<8;i++)
                _bits.Add(ReadBit());
            return BitConverter.BitsToByte(_bits);
        }

        public IEnumerable<byte> ReadBytes(uint numBytes, Endianness endianness = Endianness.LittleEndian)
        {
            List<byte> _bytes = new List<byte>();
            for (uint i = 0; i < numBytes; i++)
            {
                _bytes.Add(ReadByte());
            }

            return _bytes;
        }

        public void AlignForward()
        {
            BytePosition++;
            BitPosition = BytePosition * 8;
        }

        public void AlignBackward()
        {
            BitPosition = BytePosition * 8;
        }

        public bool ReadBool(Endianness endianness = Endianness.LittleEndian)
        {
            return ReadBit() == 1;
        }

        public char ReadChar(uint bitLength = 16, Endianness endianness = Endianness.LittleEndian)
        {
            if (bitLength == 8)
                return (char)ReadByte();
            else
                return (char)ReadUInt16(endianness);
        }

        public string ReadString(uint length, uint bitLength = 16, Endianness endian = Endianness.LittleEndian)
        {
            StringBuilder sb = new StringBuilder();
            for(int i=0;i<length;i++)
            {
                sb.Append(ReadChar(bitLength, endian));
            }
            return sb.ToString();
        }

        public Int16 ReadInt16(Endianness endianness = Endianness.LittleEndian)
        {
            byte[] _bytes = BitConverter.BitsToBytes(ReadBits(16)).ToArray();

            Int16 v = 0;
            if (endianness == Endianness.BigEndian)
            {
                for (int i = _bytes.Length - 1; i >= 0; i--)
                    v |= (Int16)(_bytes[i] << (i * 8));
            }
            else
            {
                for (int i = 0; i < _bytes.Length; i++)
                    v |= (Int16)(_bytes[i] << (i * 8));
            }
            return v;
        }

        public UInt16 ReadUInt16(Endianness endianness = Endianness.LittleEndian)
        {
            return (UInt16)ReadInt16(endianness);
        }

        public Int32 ReadInt32(Endianness endianness = Endianness.LittleEndian)
        {
            byte[] _bytes = BitConverter.BitsToBytes(ReadBits(32)).ToArray();

            Int32 v = 0;
            if (endianness == Endianness.BigEndian)
            {
                for (int i = _bytes.Length - 1; i >= 0; i--)
                    v |= (Int32)(_bytes[i] << (i * 8));
            }
            else
            {
                for (int i = 0; i < _bytes.Length; i++)
                    v |= (Int32)(_bytes[i] << (i * 8));
            }
            return v;
        }

        public UInt32 ReadUInt32(Endianness endianness = Endianness.LittleEndian)
        {
            return (UInt32)ReadInt32(endianness);
        }

        public Int64 ReadInt64(Endianness endianness = Endianness.LittleEndian)
        {
            byte[] _bytes = BitConverter.BitsToBytes(ReadBits(64)).ToArray();

            Int64 v = 0;
            if (endianness == Endianness.BigEndian)
            {
                for (int i = _bytes.Length - 1; i >= 0; i--)
                    v |= (Int64)(_bytes[i] << (i * 8));
            } else
            {
                for (int i = 0; i < _bytes.Length; i++)
                    v |= (Int64)(_bytes[i] << (i * 8));
            }
            return v;
        }

        public UInt64 ReadUInt64(Endianness endianness = Endianness.LittleEndian)
        {
            return (UInt64)ReadInt64(endianness);
        }

        public float ReadFloat(Endianness endianness = Endianness.LittleEndian)
        {
            return (float)ReadInt32(endianness);
        }

        public double ReadDouble(Endianness endianness = Endianness.LittleEndian)
        {
            return (double)ReadInt64(endianness);
        }

        public decimal ReadDecimal(Endianness endianness = Endianness.LittleEndian)
        {
            byte[] _bytes = BitConverter.BitsToBytes(ReadBits(128)).ToArray();

            Int128 v = 0;
            if (endianness == Endianness.BigEndian)
            {
                for (int i = _bytes.Length - 1; i >= 0; i--)
                    v |= (Int128)(_bytes[i] << (i * 8));
            }
            else
            {
                for (int i = 0; i < _bytes.Length; i++)
                    v |= (Int128)(_bytes[i] << (i * 8));
            }
            return (decimal)v;
        }
    }
}
