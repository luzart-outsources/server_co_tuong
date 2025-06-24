using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkClient.Network.Common
{
    public class NetworkUtils
    {
        public static byte[] IntTo3Bytes(int value)
        {
            if (value < 0 || value > 0xFFFFFF)
                throw new ArgumentOutOfRangeException("Giá trị phải nằm trong khoảng 0 - 16777215 (3 byte)");

            byte[] result = new byte[3];
            result[0] = (byte)((value >> 16) & 0xFF); 
            result[1] = (byte)((value >> 8) & 0xFF);
            result[2] = (byte)(value & 0xFF);         
            return result;
        }
        public static int Bytes3ToInt(byte[] data)
        {
            if (data == null || data.Length != 3)
                throw new ArgumentException("Mảng byte phải có đúng 3 phần tử");

            int value = (data[0] << 16) | (data[1] << 8) | data[2];
            return value;
        }


        public static byte[] deserializer(byte[] data, byte[] Keys)
        {
            byte[] result = new byte[data.Length];
            int keyIndex = 0;
            int keyLength = Keys.Length;

            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (byte)(data[i] ^ Keys[keyIndex]);
                keyIndex++;
                if (keyIndex >= keyLength) keyIndex = 0;
            }

            return result;
        }

        public static byte[] serializer(byte[] data, byte[] Keys)
        {
            byte[] result = new byte[data.Length];
            int keyIndex = 0;
            int keyLength = Keys.Length;

            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (byte)(data[i] ^ Keys[keyIndex]);
                keyIndex++;
                if (keyIndex >= keyLength) keyIndex = 0;
            }

            return result;
        }

        public static byte SByteToByte(sbyte var)
        {
            if (var > 0)
            {
                return (byte)var;
            }
            return (byte)(var + 256);
        }

        public static byte[] SByteToByte(sbyte[] var)
        {
            byte[] array = new byte[var.Length];
            for (int i = 0; i < var.Length; i++)
            {
                if (var[i] > 0)
                {
                    array[i] = (byte)var[i];
                }
                else
                {
                    array[i] = (byte)(var[i] + 256);
                }
            }
            return array;
        }

        public static sbyte ByteToSByte(byte var)
        {
            if (var > sbyte.MaxValue)
                return (sbyte)(var - 256);
            return (sbyte)var;
        }

        public static sbyte[] ByteToSByte(byte[] var)
        {
            sbyte[] array = new sbyte[var.Length];
            for (int i = 0; i < var.Length; i++)
            {
                array[i] = ByteToSByte(var[i]);
            }
            return array;
        }
    }
}
