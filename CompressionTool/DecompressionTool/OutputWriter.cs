using System.Text;
using System.IO;

namespace DecompressionTool
{
    class OutputWriter
    {
        public OutputWriter()
        {
            //Empty
        }

        public void WriteFinalDecompressedFile(string Text, string FileName)
        {
            string FilePath = @"..\..\DecompressedFiles\" + FileName + ".tsv";

            File.WriteAllText(FilePath, Text, Encoding.UTF8);
        }
    }
}
