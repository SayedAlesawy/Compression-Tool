using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CompressionTool
{
    class InputReader
    {
        public InputReader()
        {
            //empty
        }

        public string ReadOriginalFile(string FileName)
        {
            string FilePath = @"..\..\Dataset\" + FileName + ".tsv";

            return File.ReadAllText(FilePath); 
        }

        public List<byte> ReadFinalEncodedFile(string FileName)
        {
            string FilePath = @"..\..\EncodedOutput\" + FileName + ".tsv";

            byte[] InputStream = File.ReadAllBytes(FilePath);

           return InputStream.ToList<byte>();
        }

        public Dictionary<char, byte> ReadSymbolDictionary()
        {
            byte id = 0;

            Dictionary<char, byte> Alphabet = new Dictionary<char, byte>();

            string Text = File.ReadAllText(@"..\..\SymbolDictionary.txt", Encoding.UTF8);

            for (int i = 0; i < Text.Length; i++)
            {
                if (Alphabet.ContainsKey(Text[i]))
                    continue;

                Alphabet.Add(Text[i], id);
                id++;
            }

            return Alphabet;
        }
    }
}
