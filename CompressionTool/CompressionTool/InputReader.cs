using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionTool
{
    class InputReader
    {
        private string m_FileName;
        private string m_FileContent;

        private void ReadFile()
        {
            //To be changed to relative path
            string FilePath = @"G:\Compression-Tool\CompressionTool\CompressionTool\Dataset\" + m_FileName + ".tsv";

            m_FileContent = System.IO.File.ReadAllText(FilePath);
        }

        public InputReader (string FileName)
        {
            m_FileName = FileName;
        }

        public string GetFileContent()
        {
            ReadFile();
            return m_FileContent;
        }
    }
}
