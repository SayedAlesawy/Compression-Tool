using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionTool
{
    class OutputWriter
    {
        private string m_FileName;
 
        public OutputWriter(string FileName)
        {
            m_FileName = FileName;
        }

        public void WriteToFile(string Text)
        {
            string FilePath = @"G:\Compression-Tool\CompressionTool\CompressionTool\EncodedOutput\" + m_FileName + ".txt";

            System.IO.File.WriteAllText(FilePath, Text);
        }
    }
}
