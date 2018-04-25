using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace CompressionTool
{
    class LZ77Decoder
    {
        private List<byte> m_SearchBuffer;
        private List<byte> m_InputStream;
        private byte m_BytePadding;
        private int m_InputStreamIndex;
        private int m_BufferingSize;
        private int CodeUnkown;
        private int CodeDistance;
        private int CodeLength;
        private int CodeLiteral;
        private int m_MinMatchLength;
        private int m_MinBackwardDistance;
        private int m_SearchBufferMaxSize;
        private string m_TextBuffer;
        private int m_LiteralCodewordLength;
        private int m_BackwardDistanceCodewordLength;
        private int m_MatchLengthCodewordLength;
        private char Uncompressed;
        private StringBuilder m_DecodedOutput;
        private List<byte> m_OutputStream;
        private Dictionary<char, byte> m_Alphabet;
        private Dictionary<byte, char> m_InverseAlphabet;

        private void ReadCompressedFile(string FileName)
        {
            string FilePath = @"..\..\EncodedInverseMetaOutput\" + FileName + ".tsv";

            byte[] temp = File.ReadAllBytes(FilePath);

            m_BytePadding = temp[0]; 

            for (int i = 1; i < temp.Length; i++)
            {
                m_InputStream.Add(temp[i]);
            }
        }

        private void LoadSymbolDictionary()
        {
            byte id = 0;

            string Text = System.IO.File.ReadAllText(@"..\..\SymbolDictionary.txt", Encoding.UTF8);

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

            foreach (KeyValuePair<char, byte> entry in m_Alphabet)
            {
                m_InverseAlphabet.Add(entry.Value, entry.Key);
            }
        }

        private void SlideWindow(List<byte> RecentlyDecoded)
        {
            for(int i = 0; i < RecentlyDecoded.Count; i++)
            {
                m_SearchBuffer.Add(RecentlyDecoded[i]);
            }

            RetireOldMatches();
        }

        private void RetireOldMatches()
        {
            if (m_SearchBuffer.Count <= m_SearchBufferMaxSize) return;

            while (m_SearchBuffer.Count > m_SearchBufferMaxSize)
            {
                m_SearchBuffer.RemoveAt(0);
            }
        }

        private char GetFlagBit()
        {
            if (m_TextBuffer.Length < 1)
                BufferText();

            char FlagBit = m_TextBuffer[0];

            m_TextBuffer = m_TextBuffer.Remove(0, 1);

            return FlagBit;
        }

        private string GetLengthByte()
        {
            //if (m_TextBuffer.Length < m_MatchLengthCodewordLength)
                BufferText();

            string LengthByte = m_TextBuffer.Substring(0, m_MatchLengthCodewordLength);

            m_TextBuffer = m_TextBuffer.Remove(0, m_MatchLengthCodewordLength);

            return LengthByte;
        }

        private string GetLiteralByte()
        {
            //if (m_TextBuffer.Length < m_LiteralCodewordLength)
                BufferText();

            string LiteralByte = m_TextBuffer.Substring(0, m_LiteralCodewordLength);

            m_TextBuffer = m_TextBuffer.Remove(0, m_LiteralCodewordLength);

            return LiteralByte;
        }

        private string GetDistanceBits()
        {
            //if (m_TextBuffer.Length < m_BackwardDistanceCodewordLength)
                BufferText();

            string DistanceBits = m_TextBuffer.Substring(0, m_BackwardDistanceCodewordLength);

            m_TextBuffer = m_TextBuffer.Remove(0, m_BackwardDistanceCodewordLength);

            return DistanceBits;
        }
       
        private void TrimPadding()
        {
            string tmp = Convert.ToString(m_InputStream[m_InputStream.Count - 1], 2);

            m_InputStreamIndex++;

            tmp = tmp.PadLeft(8,'0');
       
            string LastByte = tmp.Substring(0, tmp.Length - m_BytePadding);

            m_TextBuffer += LastByte;
        }

        private void BufferText()
        {
            if (m_TextBuffer.Length >= m_BufferingSize) return;

            String ToBeDecoded = "";

            for (int i = m_InputStreamIndex; i < m_InputStream.Count && m_TextBuffer.Length + ToBeDecoded.Length < m_BufferingSize; i++)
            {
                m_InputStreamIndex++;

                if (i == m_InputStream.Count - 1)
                {
                    m_TextBuffer += ToBeDecoded;
                    TrimPadding();
                    return;
                }
                else
                {
                    string NewByte = Convert.ToString(m_InputStream[i], 2);

                    NewByte = NewByte.PadLeft(8, '0');

                    ToBeDecoded += NewByte;
                }
            }

            m_TextBuffer += ToBeDecoded;
        }

        private void LZ77Decode()
        {
            int CurrentCode = CodeUnkown;
            int BackwardDistance = 0, MatchLength = 0;

            BufferText();

            while (m_TextBuffer.Length > 0)
            {
                if (CurrentCode == CodeUnkown)
                {
                    char FlagBit = GetFlagBit();

                    if (FlagBit == Uncompressed)
                        CurrentCode = CodeLiteral;

                    else
                        CurrentCode = CodeDistance;
                }

                else if(CurrentCode == CodeDistance)
                {
                    BackwardDistance = Convert.ToInt32(GetDistanceBits(), 2) + m_MinBackwardDistance;

                    CurrentCode = CodeLength;
                }

                else if(CurrentCode == CodeLength)
                {
                    MatchLength = Convert.ToInt32(GetLengthByte(), 2) + m_MinMatchLength;

                    List<byte> ToBeDecoded = new List<byte>();
                    int Start = m_SearchBuffer.Count - BackwardDistance, End = Start + MatchLength;

                    for (int i = Start; i < End; i++)
                    {
                        ToBeDecoded.Add(m_SearchBuffer[i]);

                        m_OutputStream.Add(m_SearchBuffer[i]);
                    }

                    SlideWindow(ToBeDecoded);

                    CurrentCode = CodeUnkown;
                    BackwardDistance = 0; MatchLength = 0;
                }

                else if(CurrentCode == CodeLiteral)
                {
                    byte Literal = Convert.ToByte(GetLiteralByte(), 2);

                    m_SearchBuffer.Add(Literal);

                    RetireOldMatches();

                    m_OutputStream.Add(Literal);

                    CurrentCode = CodeUnkown;
                }

                if(m_TextBuffer.Length == 0)
                {
                    BufferText();
                }
            }
        }

        void ProduceOutputFile(string FileName)
        {
            for(int i = 0; i < m_OutputStream.Count; i++)
            {
                char Character = m_InverseAlphabet[m_OutputStream[i]];

                m_DecodedOutput.Append(Character);
            }

            string FilePath = @"..\..\DecompressedFiles\" + FileName + ".tsv";

            System.IO.File.WriteAllText(FilePath, m_DecodedOutput.ToString(), Encoding.UTF8);
        }

        public LZ77Decoder(int SearchBufferMaxSize)
        {
            m_InputStream = new List<byte>();
            m_SearchBuffer = new List<byte>();
            m_OutputStream = new List<byte>();
            m_DecodedOutput = new StringBuilder();
            m_Alphabet = new Dictionary<char, byte>();
            m_InverseAlphabet = new Dictionary<byte, char>();
            m_InputStreamIndex = 0;
            m_TextBuffer = "";
            m_BufferingSize = 10000;
            CodeUnkown = 0;
            CodeDistance = 1;
            CodeLength = 2;
            CodeLiteral = 3;
            m_MinMatchLength = 3;
            m_MinBackwardDistance = 1;
            m_LiteralCodewordLength = 8;
            m_BackwardDistanceCodewordLength = 15;
            m_MatchLengthCodewordLength = 8;
            m_SearchBufferMaxSize = SearchBufferMaxSize;
            Uncompressed = '0';
        }

        public void Decode(string FileName)
        {
            BuildInverseSymbolDictionary();

            //Console.WriteLine("{0}", m_Alphabet['\r']);

            ReadCompressedFile(FileName);

            LZ77Decode();

            ProduceOutputFile(FileName);
        }
    }
}
