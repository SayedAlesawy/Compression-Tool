using System.Collections.Generic;

namespace CompressionTool
{
    class TextToByteConverter
    {
        private List<byte> m_InputStream;
        private string m_FileName;
        private Dictionary<char, byte> m_Alphabet;

        private void LoadSymbolDictionary()
        {
            InputReader InputReader = new InputReader();

            m_Alphabet = InputReader.ReadSymbolDictionary();
        }

        private void ConvertText(string Text)
        {
            for(int i = 0; i < Text.Length; i++)
            {
                m_InputStream.Add(m_Alphabet[Text[i]]);
            }
        }

        public TextToByteConverter(string FileName)
        {
            m_FileName = FileName;
            m_InputStream = new List<byte>();
            m_Alphabet = new Dictionary<char, byte>();
        }

        public int GetOriginalFileSize()
        {
            string FilePath = @"..\..\Dataset\" + m_FileName + ".tsv";
            byte[] tmp = System.IO.File.ReadAllBytes(FilePath);
            
            return tmp.Length;
        }

        public List<byte> Convert()
        {
            InputReader InputReader = new InputReader();
            string Text = InputReader.ReadOriginalFile(m_FileName);

            LoadSymbolDictionary();

            ConvertText(Text);

            return m_InputStream;
        }
    }
}
