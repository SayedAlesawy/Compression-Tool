using System.Collections.Generic;
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
    }
}
