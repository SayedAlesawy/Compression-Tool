﻿using System;
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
            string FilePath = @"..\..\EncodedOutput\" + m_FileName + ".tsv";

            byte[] CompressedData = EncodedStream.ToArray();
            
            File.WriteAllBytes(FilePath, CompressedData);
        }

        public void WriteToMetaFile(List<byte> EncodedStream)
        {
            string FilePath = @"..\..\EncodedMetaOutput\" + m_FileName + ".tsv";

            byte[] CompressedData = EncodedStream.ToArray();

            File.WriteAllBytes(FilePath, CompressedData);
        }

        public void WriteToInverseMetaFile(List<byte> EncodedStream)
        {
            string FilePath = @"..\..\EncodedInverseMetaOutput\" + m_FileName + ".tsv";

            byte[] CompressedData = EncodedStream.ToArray();

            File.WriteAllBytes(FilePath, CompressedData);
        }
    }
}
