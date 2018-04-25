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
        private Dictionary<string, byte> m_DecodingCodeBook;
        private Dictionary<byte, int> m_LengthSymbolCount;
        private Dictionary<byte, List<byte>> m_SymbolsPerLength;
        private byte m_MaxCodewordLength;

        private void GetLengthSymbolCount(List<byte> AlphabetHeader)
        {
            for (int i = 0; i < AlphabetHeader.Count; i++)
            {
                if (AlphabetHeader[i] == 0) continue;

                if (m_LengthSymbolCount.ContainsKey(AlphabetHeader[i]))
                {
                    m_LengthSymbolCount[AlphabetHeader[i]]++;
                }
                else
                {
                    m_LengthSymbolCount.Add(AlphabetHeader[i], 1);
                }

                m_MaxCodewordLength = Math.Max(m_MaxCodewordLength, AlphabetHeader[i]);
            }
        }

        private void GetSymbolsPerLength(List<byte> AlphabetHeader)
        {
            for (int i = 0; i < AlphabetHeader.Count; i++)
            {
                if (m_SymbolsPerLength.ContainsKey(AlphabetHeader[i]))
                {
                    m_SymbolsPerLength[AlphabetHeader[i]].Add((byte)i);
                }
                else
                {
                    List<byte> tmp = new List<byte>();
                    tmp.Add((byte)i);
                    m_SymbolsPerLength.Add(AlphabetHeader[i], tmp);
                }
            }
        }

        private void DecodeSymbols(List<byte> AlphabetHeader)
        {
            GetLengthSymbolCount(AlphabetHeader);

            GetSymbolsPerLength(AlphabetHeader);

            string code = ""; long next = 0;

            for (byte len = 1; len <= m_MaxCodewordLength; len++)
            {
                if (m_SymbolsPerLength.ContainsKey(len))
                {
                    List<byte> Symbols = m_SymbolsPerLength[len];
                    Symbols.Sort();

                    for (int i = 0; i < Symbols.Count; i++)
                    {
                        int rem = len - code.Length;
                        for (int j = 0; j < rem; j++) code += '0';

                        m_DecodingCodeBook.Add(code, Symbols[i]);

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

        public HuffmanDecoder()
        {
            m_LengthSymbolCount = new Dictionary<byte, int>();
            m_SymbolsPerLength = new Dictionary<byte, List<byte>>();
            m_DecodingCodeBook = new Dictionary<string, byte>();
            m_MaxCodewordLength = 1;
        }

        public Dictionary<string, byte> GetCodeBook(List<byte> AlphabetHeader)
        {
            DecodeSymbols(AlphabetHeader);

            return m_DecodingCodeBook;
        }
    }
}
