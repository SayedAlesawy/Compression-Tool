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
        private string m_FileName;
 
        public OutputWriter(string FileName)
        {
            m_FileName = FileName;
        }

        public void WriteToFile(List<byte> EncodedStream)
        {
            string FilePath = @"G:\Compression-Tool\CompressionTool\CompressionTool\EncodedOutput\" + m_FileName + ".txt";

            FileStream OutputFile = new FileStream(FilePath, FileMode.Create);

            for (int i = 0; i < EncodedStream.Count; i++)
                OutputFile.WriteByte(EncodedStream[i]);
        }
    }
}
