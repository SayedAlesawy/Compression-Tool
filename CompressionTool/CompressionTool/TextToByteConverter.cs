using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionTool
{
    class TextToByteConverter
    {
        private List<byte> m_InputStream;
        private string m_FileName;
        private Dictionary<char, byte> m_Alphabet;

        private void LoadSymbolDictionary()
        {
            byte id = 0;

            string Text = System.IO.File.ReadAllText(@"G:\Compression-Tool\CompressionTool\CompressionTool\SymbolDictionary.txt", Encoding.UTF8);

            for (int i = 0; i < Text.Length; i++)
            {
                if (m_Alphabet.ContainsKey(Text[i]))
                    continue;

                m_Alphabet.Add(Text[i], id);
                id++;
            }
        }

        private void ConvertText(string Text)
        {
            for(int i = 0; i < Text.Length; i++)
            {
                char c = Text[i];
                m_InputStream.Add(m_Alphabet[Text[i]]);
            }
        }

        public TextToByteConverter(string FileName)
        {
            m_FileName = FileName;
            m_InputStream = new List<byte>();
            m_Alphabet = new Dictionary<char, byte>();
        }

        public List<byte> Convert()
        {
            InputReader InputReader = new InputReader(m_FileName);
            string Text = InputReader.GetFileContent();

            LoadSymbolDictionary();

            ConvertText(Text);

            return m_InputStream;
        }
    }
}
