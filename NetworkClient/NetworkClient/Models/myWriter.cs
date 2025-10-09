using NetworkClient.Network.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NetworkClient.Models
{
    public class myWriter
    {
        public sbyte[] buffer = new sbyte[2048];

        private int posWrite;

        private int lenght = 2048;
        public myWriter()
        {

        }
        public myWriter(byte cmd)
        {
            this.writeByte(cmd);
        }

        public sbyte[] getData()
        {
            if (posWrite <= 0)
            {
                return null;
            }
            
            sbyte[] array = new sbyte[posWrite];
            Buffer.BlockCopy(buffer, 0, array, 0, posWrite);
            return array;
        }

        private void writeSByteUncheck(sbyte value)
        {
            buffer[posWrite++] = value;
        }





        public void writeSByte(int value)
        {
            checkLenght(1);
            buffer[posWrite++] = (sbyte)value;
        }

        public void writeByte(int value)
        {
            writeSByte(NetworkUtils.ByteToSByte((byte)value));
        }

        public void writeBool(bool value)
        {
            writeSByte((sbyte)(value ? 1 : 0));
        }

        public void writeShort(int value)
        {
            checkLenght(2);
            for (int num = 1; num >= 0; num--)
            {
                writeSByteUncheck((sbyte)(value >> num * 8));
            }
        }

        public void writeInt(int value)
        {
            checkLenght(4);
            for (int num = 3; num >= 0; num--)
            {
                writeSByteUncheck((sbyte)(value >> num * 8));
            }
        }

        public void writeLong(long value)
        {
            checkLenght(8);
            for (int num = 7; num >= 0; num--)
            {
                writeSByteUncheck((sbyte)(value >> num * 8));
            }
        }

        public void writeString(string value)
        {
            Encoding unicode = Encoding.Unicode;
            Encoding encoding = Encoding.GetEncoding(65001);
            byte[] bytes = unicode.GetBytes(value);
            byte[] array = Encoding.Convert(unicode, encoding, bytes);
            writeShort((short)array.Length);
            checkLenght(array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                sbyte value2 = (sbyte)array[i];
                writeSByteUncheck(value2);
            }
        }




        public void writeArrSByte(sbyte[] value)
        {
            //checkLenght(value.Length *1);
            writeInt(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                writeSByte(value[i]);
            }
        }

        public void writeArrByte(byte[] value)
        {
            //checkLenght(value.Length * 1);
            writeInt(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                writeByte(value[i]);
                //writeSByteUncheck(value[i]);
            }
        }

        public void writeArrBool(bool[] value)
        {
            //checkLenght(value.Length * 1);
            writeInt(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                writeBool(value[i]);
            }
        }

        public void writeArrShort(short[] value)
        {
            //checkLenght(value.Length * 2);
            writeInt(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                writeShort(value[i]);
            }
        }
        public void writeArrInt(int[] value)
        {
            //checkLenght(value.Length * 4);
            writeInt(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                writeInt(value[i]);
            }
        }
        public void writeArrLong(long[] value)
        {
            //checkLenght(value.Length * 8);
            writeInt(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                writeLong(value[i]);
            }
        }
        public void writeArrString(string[] value)
        {
            //checkLenght(value.Length);
            writeInt(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                writeString(value[i]);
            }
        }




        public void checkLenght(int ltemp)
        {
            if (posWrite + ltemp > lenght)
            {
                sbyte[] array = new sbyte[lenght + 1024 + ltemp];
                for (int i = 0; i < lenght; i++)
                {
                    array[i] = buffer[i];
                }
                buffer = null;
                buffer = array;
                lenght += 1024 + ltemp;
            }
        }



        public void Clear()
        {
            buffer = null;
        }
    }
}
