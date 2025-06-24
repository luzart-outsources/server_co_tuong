using NetworkClient.Network.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NetworkClient.Models
{
    public class myReader
    {
        public sbyte[] buffer;

        private int posRead;

        public myReader(sbyte[] data)
        {
            buffer = data;
        }

        public sbyte readSByte()
        {
            if (posRead < buffer.Length)
            {
                return buffer[posRead++];
            }
            posRead = buffer.Length;
            return 0;
        }

        public byte readByte()
        {
            return NetworkUtils.SByteToByte(readSByte());
        }

        public bool readBool()
        {
            return readSByte() > 0;
        }

        public short readShort()
        {
            short num = 0;
            for (int i = 0; i < 2; i++)
            {
                num = (short)(num << 8);
                num = (short)(num | (short)(0xFF & buffer[posRead++]));
            }
            return num;
        }

        public int readInt()
        {
            int num = 0;
            for (int i = 0; i < 4; i++)
            {
                num <<= 8;
                num |= 0xFF & buffer[posRead++];
            }
            return num;
        }

        public long readLong()
        {
            long num = 0L;
            for (int i = 0; i < 8; i++)
            {
                num <<= 8;
                num |= 0xFF & buffer[posRead++];
            }
            return num;
        }

        public string readString()
        {
            short num = readShort();
            byte[] array = new byte[num];
            for (int i = 0; i < num; i++)
            {
                array[i] = NetworkUtils.SByteToByte(readSByte());
            }
            UTF8Encoding uTF8Encoding = new UTF8Encoding();
            return uTF8Encoding.GetString(array);
        }

        public sbyte[] readArrSByte()
        {
            int size = readInt();
            sbyte[] re = new sbyte[size];
            for (int i = 0; i < size; i++)
            {
                re[i] = readSByte();
            }
            return re;
        }
        public byte[] readArrByte()
        {
            int size = readInt();
            byte[] re = new byte[size];
            for (int i = 0; i < size; i++)
            {
                re[i] = readByte();
            }
            return re;
        }
        public bool[] readArrBool()
        {
            int size = readInt();
            bool[] re = new bool[size];
            for (int i = 0; i < size; i++)
            {
                re[i] = readBool();
            }
            return re;
        }
        public short[] readArrShort()
        {
            int size = readInt();
            short[] re = new short[size];
            for (int i = 0; i < size; i++)
            {
                re[i] = readShort();
            }
            return re;
        }
        public int[] readArrInt()
        {
            int size = readInt();
            int[] re = new int[size];
            for (int i = 0; i < size; i++)
            {
                re[i] = readInt();
            }
            return re;
        }
        public long[] readArrLong()
        {
            int size = readInt();
            long[] re = new long[size];
            for (int i = 0; i < size; i++)
            {
                re[i] = readLong();
            }
            return re;
        }
        public string[] readArrString()
        {
            int size = readInt();
            string[] re = new string[size];
            for (int i = 0; i < size; i++)
            {
                re[i] = readString();
            }
            return re;
        }

        public void Clear()
        {
            buffer = null;
        }
    }
}
