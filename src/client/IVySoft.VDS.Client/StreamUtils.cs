using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IVySoft.VDS.Client
{
    static class StreamUtils
    {
        public static byte[] pop_data(this Stream stream)
        {
            var len = read_number(stream);
            var result = new byte[len];
            if (len != stream.Read(result, 0, len))
            {
                throw new Exception("EOF");
            }

            return result;
        }
        public static void push_data(this Stream stream, byte[] data)
        {
            stream.write_number(data.Length);
            stream.Write(data, 0, data.Length);
        }

        public static int read_number(this Stream stream)
        {
            var value = stream.ReadByte();

            if (0x80 > value)
            {
                return value;
            }

            int result = 0;
            for (int i = (value & 0x7F); i > 0; --i)
            {
                value = stream.ReadByte();
                result <<= 8;
                result |= value;
            }

            return result + 0x80;
        }
        public static void write_number(this Stream stream, int value)
        {
            // 0 .... 7 bit
            if (128 > value)
            {
                stream.WriteByte((byte)value);
                return;
            }

            value -= 128;

            var data = new byte[8];
            int index = 0;
            do
            {
                data[index++] = (byte)(value & 0xFF);
                value >>= 8;
            } while (0 != value);

            stream.WriteByte((byte)(0x80 | index));
            while (index > 0)
            {
                stream.WriteByte(data[--index]);
            }
        }

        public static Int64 get_int64(this Stream stream)
        {
            Int64 result = 0;
            for (var i = 0; i < 8; ++i)
            {
                var value = stream.ReadByte();
                result <<= 8;
                result |= value;
            }

            return result;
        }
        public static void push_int64(this Stream stream, Int64 value)
        {
            stream.WriteByte((byte)((value >> 56) & 0xFF));
            stream.WriteByte((byte)((value >> 48) & 0xFF));
            stream.WriteByte((byte)((value >> 40) & 0xFF));
            stream.WriteByte((byte)((value >> 32) & 0xFF));
            stream.WriteByte((byte)((value >> 24) & 0xFF));
            stream.WriteByte((byte)((value >> 16) & 0xFF));
            stream.WriteByte((byte)((value >> 8) & 0xFF));
            stream.WriteByte((byte)(value & 0xFF));
        }

        public static string get_string(this Stream stream)
        {
            return Encoding.UTF8.GetString(stream.pop_data());
        }
        public static void push_string(this Stream stream, string value)
        {
            stream.push_data(Encoding.UTF8.GetBytes(value));
        }

    }
}
