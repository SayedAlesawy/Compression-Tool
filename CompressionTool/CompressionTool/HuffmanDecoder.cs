using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CompressionTool
{
    class HuffmanDecoder
    {
        private Dictionary<string, char> m_DecodingDictionary;
        private Dictionary<char, int> m_Alphabet;
        private Dictionary<int, char> m_InverseAlphabet;
        private Dictionary<byte, int> m_LengthSymbolCount;
        private Dictionary<byte, List<char>> m_SymbolsPerLength;
        private string m_DecodedText;
        private byte[] m_CompressedData;
        private byte m_BytePadding;
        private byte m_MaxCodewordLength;

        private void LoadSymbolDictionary()
        {
            int id = 0;

            String Text = System.IO.File.ReadAllText(@"G:\Compression-Tool\CompressionTool\CompressionTool\SymbolDictionary.txt", Encoding.UTF8);

            for (int i = 0; i < Text.Length; i++)
            {
                if (m_Alphabet.ContainsKey(Text[i]))
                    continue;

                m_Alphabet.Add(Text[i], id);
                id++;
            }
        }

        private void BuildInverseSymbolDictionary()
        {
            LoadSymbolDictionary();

            foreach (KeyValuePair<char, int> entry in m_Alphabet)
            {
                m_InverseAlphabet.Add(entry.Value, entry.Key);
            }
        }

        private void ReadCompressedFile(string FileName)
        {
            string FilePath = @"G:\Compression-Tool\CompressionTool\CompressionTool\EncodedOutput\" + FileName + ".txt";

            m_CompressedData = File.ReadAllBytes(FilePath);

            m_BytePadding = m_CompressedData[m_InverseAlphabet.Count];
        }

        private void GetLengthSymbolCount()
        {
            for (int i = 0; i < m_InverseAlphabet.Count; i++)
            {
                if (m_CompressedData[i] == 0) continue;

                if (m_LengthSymbolCount.ContainsKey(m_CompressedData[i]))
                {
                    m_LengthSymbolCount[m_CompressedData[i]]++;
                }
                else
                {
                    m_LengthSymbolCount.Add(m_CompressedData[i], 1);
                }

                m_MaxCodewordLength = Math.Max(m_MaxCodewordLength, m_CompressedData[i]);
            }
        }

        private void GetSymbolsPerLength()
        {
            for (int i = 0; i < m_InverseAlphabet.Count ; i++)
            {
                if (m_SymbolsPerLength.ContainsKey(m_CompressedData[i]))
                {
                    m_SymbolsPerLength[m_CompressedData[i]].Add(m_InverseAlphabet[i]);
                }
                else
                {
                    List<char> tmp = new List<char>();
                    tmp.Add(m_InverseAlphabet[i]);
                    m_SymbolsPerLength.Add(m_CompressedData[i], tmp);
                }
            }
        }

        private void DecodeSymbols()
        {
            GetLengthSymbolCount();

            GetSymbolsPerLength();

            string code = ""; long next = 0;

            for (byte len = 1; len <= m_MaxCodewordLength; len++)
            {
                if (m_SymbolsPerLength.ContainsKey(len))
                {
                    List<char> Symbols = m_SymbolsPerLength[len];
                    Symbols.Sort();

                    for (int i = 0; i < Symbols.Count; i++)
                    {
                        int rem = len - code.Length;
                        for (int j = 0; j < rem; j++) code += '0';

                        m_DecodingDictionary.Add(code, Symbols[i]);

                        int PreSize = code.Length;
                        next = Convert.ToInt64(code, 2) + 1;
                        code = Convert.ToString(next, 2);

                        int LeftPadding = PreSize - code.Length; string Padding = "";

                        for (int p = 0; p < LeftPadding; p++) Padding += '0';

                        code = Padding + code;
                    }
                }
            }
        }

        private string DecodePartialText(string PartialText)
        {
            Console.WriteLine("Called");
            string Partial = ""; int LastIndex = 0;

            for(int i = 0; i < PartialText.Length; i++)
            {
                char c = PartialText[i];

                Partial += PartialText[i];

                if (m_DecodingDictionary.ContainsKey(Partial))
                {
                    m_DecodedText += m_DecodingDictionary[Partial];
                    LastIndex = i + 1;
                    Partial = "";
                }
            }

            return PartialText.Substring(LastIndex, PartialText.Length - LastIndex);
        }

        private void DecodeText()
        {
            string ToBeDecoded = ""; 

            for (int i = m_InverseAlphabet.Count+1; i < m_CompressedData.Length - 1; i++)
            {
                string NewByte = Convert.ToString(m_CompressedData[i], 2);
                string LeftPadding = ""; int PaddingCnt = 8 - NewByte.Length;

                for (int p = 0; p < PaddingCnt; p++) LeftPadding += '0';

                NewByte = LeftPadding + NewByte;

                ToBeDecoded += NewByte;

                if (ToBeDecoded.Length >= 100000)
                {
                    ToBeDecoded = DecodePartialText(ToBeDecoded);
                }
            }

            if (ToBeDecoded.Length > 0)
            {
                ToBeDecoded = DecodePartialText(ToBeDecoded);
            }

            string tmp = Convert.ToString(m_CompressedData[m_CompressedData.Length - 1], 2);
            int TmpLeftPadding = 8 - tmp.Length; string TmpPadding = "";

            for (int p = 0; p < TmpLeftPadding; p++) TmpPadding += '0';
            tmp = TmpPadding + tmp;

            string LastByte = tmp.Substring(0, tmp.Length - m_BytePadding);
            ToBeDecoded += LastByte;

            DecodePartialText(ToBeDecoded);
        }

        private void ProduceDecompressedFile(string FileName)
        {
            string FilePath = @"G:\Compression-Tool\CompressionTool\CompressionTool\DecompressedFiles\" + FileName + ".txt";

            System.IO.File.WriteAllText(FilePath, m_DecodedText, Encoding.UTF8);
        }

        public HuffmanDecoder()
        {
            m_DecodingDictionary = new Dictionary<string, char>();
            m_InverseAlphabet = new Dictionary<int, char>();
            m_Alphabet = new Dictionary<char, int>();
            m_LengthSymbolCount = new Dictionary<byte, int>();
            m_SymbolsPerLength = new Dictionary<byte, List<char>>();
            m_BytePadding = 0;
            m_MaxCodewordLength = 1;
        }

        public void Decode(string FileName)
        {
            BuildInverseSymbolDictionary();

            ReadCompressedFile(FileName);

            DecodeSymbols();

            DecodeText();

            ProduceDecompressedFile(FileName);

            /*
            foreach (KeyValuePair<string, char> entry in m_DecodingDictionary)
            {
                Console.WriteLine(entry.Key);
            }
            */
        }
    }
}
