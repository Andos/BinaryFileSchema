using System;
using System.IO;
using System.IO.Compression;

namespace TestFileGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
			//The purpose of this test project is simply to generate a simple text file that gets compressed using GZip

            byte[] charName = new byte[] { (byte)'A', (byte)'n', (byte)'d', (byte)'e', (byte)'r', (byte)'s', (byte)' ',
				(byte)'R', (byte)'i', (byte)'g', (byte)'g', (byte)'e', (byte)'l', (byte)'s', (byte)'e', (byte)'n' };

            FileStream outfile = new FileStream("compressed.dat", FileMode.Create);
            GZipStream gzipCompressor = new GZipStream(outfile, CompressionMode.Compress);
            gzipCompressor.Write(charName, 0, charName.Length);

            gzipCompressor.Close();
            outfile.Close();

            FileStream infile = new FileStream("compressed.dat", FileMode.Open);
            outfile = new FileStream("decompressed.txt", FileMode.Create);
            GZipStream gzipDecompressor = new GZipStream(infile, CompressionMode.Decompress);
            byte[] readBuffer = new byte[1000];
            int count = gzipDecompressor.Read(readBuffer, 0, 1000);
            outfile.Write(readBuffer, 0, count);
            infile.Close();
            outfile.Close();            
        }
    }
}
