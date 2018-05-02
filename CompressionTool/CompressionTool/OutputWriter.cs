using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CompressionTool
{
    class OutputWriter
    {
        public OutputWriter()
        {
            //Empty
        }

        public void WriteFinalCompressedFile(List<byte> EncodedStream, string FileName)
        {
            string FilePath = @"..\..\EncodedOutput\" + FileName + ".tsv";

            byte[] CompressedData = EncodedStream.ToArray();

            File.WriteAllBytes(FilePath, CompressedData);
        }

        public void WriteFinalDecompressedFile(string Text, string FileName)
        {
            string FilePath = @"..\..\DecompressedFiles\" + FileName + ".tsv";

            File.WriteAllText(FilePath, Text, Encoding.UTF8);
        }
    }
}
