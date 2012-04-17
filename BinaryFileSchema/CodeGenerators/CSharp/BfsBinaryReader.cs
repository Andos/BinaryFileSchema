using System;
using System.IO;
using System.Text;
using System.Net;
using System.IO.Compression;

//namespace BfsHelperClasses
//{

    public class BfsBinaryReader
    {
        BinaryReader reader;
        ASCIIEncoding asciienc;
        UTF7Encoding utf7enc;
        UTF32Encoding utf32enc;

        public long Position { get { return GetPosition(); } set { Seek(value, SeekOrigin.Begin); } }
        public long Length { get { return reader.BaseStream.Length; } }

        public Stream BaseStream { get { return reader.BaseStream; } }

        public enum Endianness { LittleEndian, BigEndian }

        public BfsBinaryReader(BinaryReader binaryReader, Endianness fileEndianness)
        {
            reader = binaryReader;
            FileEndianness = fileEndianness;
            asciienc = new ASCIIEncoding();
            utf7enc = new UTF7Encoding();
            utf32enc = new UTF32Encoding();
        }

        public Endianness FileEndianness { get; set; }

        public void TestReadCompressed()
        {
            GZipStream gstream = new GZipStream(reader.BaseStream, CompressionMode.Decompress);
            Console.WriteLine("Can seek: " + gstream.CanSeek.ToString() );
        }

        public void SkipBytes(int count)
        {
            reader.BaseStream.Seek(count, SeekOrigin.Current);
        }

        public byte[] ReadByteArray(int count)
        {
            return reader.ReadBytes(count);
        }

        public bool ReadBool()
        {
            return reader.ReadBoolean();
        }

        public byte ReadUbyte()
        {
            return reader.ReadByte();
        }

        public byte ReadSbyte()
        {
            return (byte)reader.ReadSByte();
        }

        public short ReadShort()
        {
            if (FileEndianness == Endianness.BigEndian)
                return IPAddress.HostToNetworkOrder(reader.ReadInt16());
            else
                return reader.ReadInt16();
        }

        public short ReadUshort()
        {
            if (FileEndianness == Endianness.BigEndian)
                return IPAddress.HostToNetworkOrder(reader.ReadInt16());
            else
                return (short)reader.ReadUInt16();
        }

        public int ReadInt()
        {
            if (FileEndianness == Endianness.BigEndian)
                return IPAddress.HostToNetworkOrder(reader.ReadInt32());
            else
                return reader.ReadInt32();
        }

        public int ReadUint()
        {
            if (FileEndianness == Endianness.BigEndian)
                return (int)IPAddress.HostToNetworkOrder(reader.ReadInt32());
            else
                return (int)reader.ReadUInt32();
        }

        public long ReadLong()
        {
            if (FileEndianness == Endianness.BigEndian)
                return IPAddress.HostToNetworkOrder(reader.ReadInt32());
            else
                return reader.ReadInt32();
        }

        //returning long instead of ulong to be CLS compliant. (might fail because of the same range+conversion)
        public long ReadUlong()
        {
            if (FileEndianness == Endianness.BigEndian)
                return (long)IPAddress.HostToNetworkOrder(reader.ReadInt64());
            else
                return (long)reader.ReadUInt64();
        }

        public string ReadASCIIString(string expected)
        {
            byte[] tmp_string = reader.ReadBytes(expected.Length);
            string text = asciienc.GetString(tmp_string);
            if (text != expected)
                throw new FormatException("Error reading ASCII string! Expected: " + expected + ", got: " + text);
            return text;
        }

        public string ReadUTF7String(string expected)
        {
            byte[] tmp_string = reader.ReadBytes(expected.Length);
            string text = utf7enc.GetString(tmp_string);
            if (text != expected)
                throw new FormatException("Error reading UTF7 string! Expected: " + expected + ", got: " + text);
            return text;
        }

        public string ReadUTF32String(string expected)
        {
            byte[] tmp_string = reader.ReadBytes(expected.Length * 4);
            string text = utf32enc.GetString(tmp_string);
            if (text != expected)
                throw new FormatException("Error reading UTF32 string! Expected: " + expected + ", got: " + text);
            return text;
        }

        public long GetPosition()
        {
            return reader.BaseStream.Position;
        }

        public long Seek(long offset, SeekOrigin origin)
        {
            return reader.BaseStream.Seek(offset, origin);
        }
    }

    public class StopCaseTester
    {
        StopCase [] stopcases;
        BfsBinaryReader file;
        int stoppedAtCase = 0;
        int stopcasesLeft;
        bool stopsAtEOF;

        public StopCaseTester( BfsBinaryReader file, bool stopsAtEOF, StopCase [] stopcases ) 
        {
            this.stopcases = stopcases;
            this.file = file;
            this.stopsAtEOF = stopsAtEOF;
            stoppedAtCase = 0;
        }

        public bool CanContinue()
        {
            long seek = file.Position;
            stopcasesLeft = stopcases.Length;
            bool[] dead = new bool[stopcases.Length];
            int index = 0;

            while (stopcasesLeft > 0)
            {
                if (stopsAtEOF && file.BaseStream.Position == file.BaseStream.Length)
                    return false;

                byte b = file.ReadUbyte();
                for (int i = 0; i < stopcases.Length; i++)
                {
                    if (dead[i] == false)
                    {
                        if (stopcases[i].stopcase[index] != b)
                        {
                            dead[i] = true;
                            stopcasesLeft--;
                        }

                        if (index == stopcases[i].stopcase.Length - 1 && dead[i] == false)
                        {
                            stoppedAtCase = i;
                            file.Position = seek;
                            return false;
                        }
                    }
                }
                index++;
            }
            file.Position = seek;
            return true;
        }

        public StopCase StoppedAtCase()
        {
            return stopcases[stoppedAtCase];
        }
    }

    public struct StopCase
    {
        public byte[] stopcase;
        public bool isIncluded;
        public bool isSkipped;
        public int matches;
        public StopCase(byte [] stopcase, bool included, bool skipped)
        {
            this.stopcase = stopcase;
            this.isIncluded = included;
            this.isSkipped = skipped;
            matches = 0;
        }
    }

    public class CircularBuffer
    {
        int size;
        int position;
        int bufferUsage;
        byte[] buffer;

        public CircularBuffer(int size)
        {
            this.size = size;
            buffer = new byte[size];
            position = 0;
            bufferUsage = 0;
        }

        public void ReadByte(byte b)
        {
            buffer[position] = b;
            position = (position + 1) % size;
            bufferUsage = Math.Max(bufferUsage + 1, size);
        }

        public void ReadBytes(byte [] bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
                ReadByte(bytes[i]);
        }

        public bool BufferMatches(string text)
        {
            char[] chars = text.ToCharArray();
            byte[] bytes = new byte[chars.Length];
            for (int i = 0; i < chars.Length; i++)
                bytes[i] = (byte)chars[i];
            return BufferMatches(bytes);
        }

        public bool BufferMatches(byte[] bytes)
        {
            if (bufferUsage < bytes.Length)
                return false;

            for (int i = 0; i < bytes.Length; i++)
            {
                int u = (position + i) % size;
                if (buffer[u] != bytes[i])
                    return false;
            }
            return true;
        }

    }

//}