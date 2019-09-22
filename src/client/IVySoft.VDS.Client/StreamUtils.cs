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

        public static string get_string(this Stream stream)
        {
            return Encoding.UTF8.GetString(stream.pop_data());
        }

    }
}
