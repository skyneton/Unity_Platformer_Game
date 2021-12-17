using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Network
{
    public class ByteBuf
    {
        private byte[] readBuf;
        private readonly List<byte> buf = new List<byte>();

        public bool Available => buf.Count > 0;
        public int Length => readBuf.Length - Position;
        public int WriteLength => buf.Count;
        public int Position = 0;

        public ByteBuf(byte[] data)
        {
            readBuf = data;
        }
        public ByteBuf() {}

        public static IEnumerable<byte> GetVarInt(int integer)
        {
            var buf = new List<byte>();
            while ((integer & -128) != 0)
            {
                buf.Add((byte) (integer & 127 | 128));
                integer = (int) ((uint) integer >> 7);
            }
            
            buf.Add((byte) integer);
            return buf.ToArray();
        }
        
        public static int ReadVarInt(NetworkStream stream)
        {
            var value = 0;
            var size = 0;
            int b;

            while (((b = stream.ReadByte()) & 0x80) == 0x80)
            {
                value |= (b & 0x7F) << (size++ * 7);
                if (size > 5)
                {
                    throw new IOException("VarInt too long.");
                }
            }

            return value | ((b & 0x7F) << (size * 7));
        }
        
        public static int ReadVarInt(byte[] data)
        {
            var value = 0;
            var size = 0;
            int b;

            while (((b = data[size]) & 0x80) == 0x80)
            {
                value |= (b & 0x7F) << (size++ * 7);
                if (size > 5)
                {
                    throw new IOException("VarInt too long.");
                }
            }

            return value | ((b & 0x7F) << (size * 7));
        }

        public int ReadByte()
        {
            return readBuf[Position++];
        }

        public byte[] Read(int length)
        {
            var buffer = new byte[length];
            Buffer.BlockCopy(readBuf, Position, buffer, 0, length);
            Position += length;
            
            return buffer;
        }

        public byte[] Peek(int length)
        {
            var buffer = new byte[length];
            Buffer.BlockCopy(readBuf, Position, buffer, 0, length);
            
            return buffer;
        }

        public bool ReadBool()
        {
            return ReadByte() != 0;
        }
        
        public int ReadVarInt()
        {
            var value = 0;
            var size = 0;
            int b;

            while (((b = ReadByte()) & 0x80) == 0x80)
            {
                value |= (b & 0x7F) << (size++ * 7);
                if (size > 5)
                {
                    throw new IOException("VarInt too long.");
                }
            }

            return value | ((b & 0x7F) << (size * 7));
        }

        public int ReadInt()
        {
            return BitConverter.ToInt32(Read(4), 0);
        }

        public string ReadString()
        {
            return Encoding.UTF8.GetString(Read(ReadVarInt()));
        }

        public long ReadLong()
        {
            return BitConverter.ToInt64(Read(8), 0);
        }

        public short ReadShort()
        {
            return BitConverter.ToInt16(Read(2), 0);
        }

        public float ReadFloat()
        {
            return BitConverter.ToSingle(Read(4), 0);
        }

        public double ReadDouble()
        {
            return BitConverter.ToDouble(Read(8), 0);
        }

        public void Write(IEnumerable<byte> data)
        {
            buf.AddRange(data);
        }

        public void Write(byte[] data, int length)
        {
            var arr = new byte[length];
            Buffer.BlockCopy(data, 0, arr, 0, length);
            buf.AddRange(arr);
        }

        public void WriteByte(byte b)
        {
            buf.Add(b);
        }

        public void WriteVarInt(int integer)
        {
            buf.AddRange(GetVarInt(integer));
        }

        public void WriteInt(int data)
        {
            buf.AddRange(BitConverter.GetBytes(data));
        }

        public void WriteString(string data)
        {
            byte[] stringData = Encoding.UTF8.GetBytes(data);
            WriteVarInt(stringData.Length);
            buf.AddRange(stringData);
        }

        public void WriteShort(short data)
        {
            buf.AddRange(BitConverter.GetBytes(data));
        }

        public void WriteUShort(ushort data)
        {
            buf.AddRange(BitConverter.GetBytes(data));
        }

        public void WriteBool(bool data)
        {
            buf.AddRange(BitConverter.GetBytes(data));
        }

        public void WriteDouble(double data)
        {
            buf.AddRange(BitConverter.GetBytes(data));
        }

        public void WriteFloat(float data)
        {
            buf.AddRange(BitConverter.GetBytes(data));
            // buf.AddRange(HostToNetworkOrder(data));
        }

        public void WriteLong(long data)
        {
            buf.AddRange(BitConverter.GetBytes(data));
        }

        public byte[] Flush()
        {
            buf.InsertRange(0, GetVarInt(buf.Count));
            var data = buf.ToArray();

            readBuf = null;
            buf.Clear();

            return data;
        }

        public byte[] GetBytes()
        {
            return buf.ToArray();
        }

        public byte[] GetReadBytes()
        {
            return readBuf;
        }
    }
}