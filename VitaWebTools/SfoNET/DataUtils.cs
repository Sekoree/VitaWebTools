using System;
using System.IO;
using System.Linq;
using System.Text;

namespace BasicDataTypes
{
    class DataUtils
    {
        public static void CopyString(byte[] str, string text, int index)
        {
            var textBytes = Encoding.UTF8.GetBytes(text);
            Array.ConstrainedCopy(textBytes, 0, str, index, textBytes.Length);
        }

        public static void CopyInt32(byte[] str, int value, int index)
        {
            var valueBytes = BitConverter.GetBytes(value);
            Array.ConstrainedCopy(valueBytes, 0, str, index, valueBytes.Length);
        }
        public static void CopyInt32BE(byte[] str, int value, int index)
        {
            var valueBytes = BitConverter.GetBytes(value);
            var valueBytesBe = valueBytes.Reverse().ToArray();
            Array.ConstrainedCopy(valueBytesBe, 0, str, index, valueBytesBe.Length);
        }

        // Read From Streams
        public static uint ReadUInt32(Stream str)
        {
            var intBytes = new byte[0x4];
            _ = str.Read(intBytes, 0x00, intBytes.Length);
            return BitConverter.ToUInt32(intBytes, 0x00);
        }
        public static uint ReadInt32(Stream str)
        {
            var intBytes = new byte[0x4];
            _ = str.Read(intBytes, 0x00, intBytes.Length);
            return BitConverter.ToUInt32(intBytes, 0x00);
        }
        public static ulong ReadUInt64(Stream str)
        {
            var intBytes = new byte[0x8];
            _ = str.Read(intBytes, 0x00, intBytes.Length);
            return BitConverter.ToUInt64(intBytes, 0x00);
        }
        public static long ReadInt64(Stream str)
        {
            var intBytes = new byte[0x8];
            _ = str.Read(intBytes, 0x00, intBytes.Length);
            return BitConverter.ToInt64(intBytes, 0x00);
        }
        public static ushort ReadUInt16(Stream str)
        {
            var intBytes = new byte[0x2];
            _ = str.Read(intBytes, 0x00, intBytes.Length);
            return BitConverter.ToUInt16(intBytes, 0x00);
        }
        public static short ReadInt16(Stream str)
        {
            byte[] intBytes = new byte[0x2];
            _ = str.Read(intBytes, 0x00, intBytes.Length);
            return BitConverter.ToInt16(intBytes, 0x00);
        }

        public static uint ReadUint32At(Stream str, int location)
        {
            var oldPos = str.Position;
            str.Seek(location, SeekOrigin.Begin);
            var pOut = ReadUInt32(str);
            str.Seek(oldPos, SeekOrigin.Begin);
            return pOut;
        }

        public static byte[] ReadBytesAt(Stream str, int location, int length)
        {
            var oldPos = str.Position;
            str.Seek(location, SeekOrigin.Begin);
            var work_buf = new byte[length];
            _ = str.Read(work_buf, 0x0, work_buf.Length);
            str.Seek(oldPos, SeekOrigin.Begin);
            return work_buf;
        }

        public static string ReadStringAt(Stream str,int location)
        {
            var oldPos = str.Position;
            str.Seek(location, SeekOrigin.Begin);
            var pOut = ReadString(str);
            str.Seek(oldPos,SeekOrigin.Begin);
            return pOut;
        }
        public static string ReadString(Stream str)
        {
            using MemoryStream ms = new MemoryStream();
            while (true)
            {
                var c = (byte)str.ReadByte();
                if (c == 0)
                    break;
                ms.WriteByte(c);
            }
            ms.Seek(0x00, SeekOrigin.Begin);
            var pOut = Encoding.UTF8.GetString(ms.ToArray());
            ms.Dispose();

            return pOut;
        }

        // Write To Streams

        public static void WriteUInt32(Stream str, uint numb)
        {
            var intBytes = BitConverter.GetBytes(numb);
            str.Write(intBytes, 0x00, intBytes.Length);
        }
        public static void WriteInt32(Stream str, int numb)
        {
            var intBytes = BitConverter.GetBytes(numb);
            str.Write(intBytes, 0x00, intBytes.Length);
        }
        public static void WriteUInt64(Stream dst, ulong value)
        {
            var valueBytes = BitConverter.GetBytes(value);
            dst.Write(valueBytes, 0x00, 0x8);
        }
        public static void WriteInt64(Stream dst, long value)
        {
            var valueBytes = BitConverter.GetBytes(value);
            dst.Write(valueBytes, 0x00, 0x8);
        }
        public static void WriteUInt16(Stream dst, ushort value)
        {
            var valueBytes = BitConverter.GetBytes(value);
            dst.Write(valueBytes, 0x00, 0x2);
        }
        public static void WriteInt16(Stream dst, short value)
        {
            var valueBytes = BitConverter.GetBytes(value);
            dst.Write(valueBytes, 0x00, 0x2);
        }

        public static void WriteInt32BE(Stream str, int numb)
        {
            var intBytes = BitConverter.GetBytes(numb);
            var intBytesBe = intBytes.Reverse().ToArray();
            str.Write(intBytesBe, 0x00, intBytesBe.Length);
        }
        public static void WriteString(Stream str, string text, int len = -1)
        {
            if (len < 0)
            {
                len = text.Length;
            }

            var textBytes = Encoding.UTF8.GetBytes(text);
            str.Write(textBytes, 0x00, textBytes.Length);
        }

    }
}
