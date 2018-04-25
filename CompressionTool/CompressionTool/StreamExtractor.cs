using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CompressionTool
{
    class StreamExtractor
    {
        private List<byte> m_Literals;
        private List<byte> m_MatchLengths;
        private List<byte> m_InputStream;
        private List<byte> m_BackwardDistances;
        private List<byte> m_LiteralsHeader;
        private List<byte> m_MatchLengthsHeader;
        private List<byte> m_BackwardDistancesHeader;
        private int m_LiteralsHeaderSize;
        private int m_MatchLengthsHeaderSize;
        private int m_BackwardDistanceHeaderSize;
        private int Huffman;
        private int LZ77;
        private int m_BufferingSize;
        private int m_InputStreamIndex;
        private int m_LiteralCodewordLength;
        private int m_BackwardDistanceCodewordLength;
        private int m_MatchLengthCodewordLength;
        private int CodeUnkown;
        private int CodeDistance;
        private int CodeLength;
        private int CodeLiteral;
        private char Uncompressed;
        private string m_FileName;
        private byte m_BytePadding;
        private string m_TextBuffer;

        private void LoadFileLZ77()
        {
            string FilePath = @"..\..\EncodedMetaOutput\" + m_FileName + ".tsv";

            byte[] temp = File.ReadAllBytes(FilePath);

            m_BytePadding = temp[0];

            m_InputStream = temp.ToList<byte>();

            m_InputStream.RemoveAt(0);
        }

        private void LoadFileHuffman()
        {
            string FilePath = @"..\..\EncodedOutput\" + m_FileName + ".tsv";

            byte[] temp = File.ReadAllBytes(FilePath);

            m_InputStream = temp.ToList<byte>();
        }

        private void StrategySelection(int ExtractionAlgo)
        {
            if(ExtractionAlgo == Huffman)
            {
                LoadFileHuffman();

                HuffmanExtract();
            }
            else if(ExtractionAlgo == LZ77)
            {
                LoadFileLZ77();

                LZ77Extract();
            }
        }

        private void HuffmanExtract()
        {
            for (int i = 0; i < m_LiteralsHeaderSize; i++)
            {
                m_LiteralsHeader.Add(m_InputStream[i]);
            }

            for (int i = m_LiteralsHeaderSize; i < m_LiteralsHeaderSize + m_MatchLengthsHeaderSize; i++)
            {
                m_MatchLengthsHeader.Add(m_InputStream[i]);
            }

            for (int i = m_LiteralsHeaderSize + m_MatchLengthsHeaderSize; i < m_LiteralsHeaderSize + m_MatchLengthsHeaderSize + m_BackwardDistanceHeaderSize; i++)
            {
                m_BackwardDistancesHeader.Add(m_InputStream[i]);
            }

            m_BytePadding = m_InputStream[m_LiteralsHeaderSize + m_MatchLengthsHeaderSize + m_BackwardDistanceHeaderSize];
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
            if (m_TextBuffer.Length < m_MatchLengthCodewordLength)
                BufferText();

            string LengthByte = m_TextBuffer.Substring(0, m_MatchLengthCodewordLength);

            m_TextBuffer = m_TextBuffer.Remove(0, m_MatchLengthCodewordLength);

            return LengthByte;
        }

        private string GetLiteralByte()
        {
            if (m_TextBuffer.Length < m_LiteralCodewordLength)
                BufferText();

            string LiteralByte = m_TextBuffer.Substring(0, m_LiteralCodewordLength);

            m_TextBuffer = m_TextBuffer.Remove(0, m_LiteralCodewordLength);

            return LiteralByte;
        }

        private string GetDistanceBits()
        {
            if (m_TextBuffer.Length < m_BackwardDistanceCodewordLength)
                BufferText();

            string DistanceBits = m_TextBuffer.Substring(0, m_BackwardDistanceCodewordLength);

            m_TextBuffer = m_TextBuffer.Remove(0, m_BackwardDistanceCodewordLength);

            return DistanceBits;
        }

        private void TrimPadding()
        {
            string tmp = Convert.ToString(m_InputStream[m_InputStream.Count - 1], 2);

            m_InputStreamIndex++;

            tmp = tmp.PadLeft(8, '0');

            string LastByte = tmp.Substring(0, tmp.Length - m_BytePadding);

            m_TextBuffer += LastByte;
        }

        private void BufferText()
        {
            if (m_TextBuffer.Length >= m_BufferingSize) return;

            String ToBeDecoded = "";

            for (int i = m_InputStreamIndex; i < m_InputStream.Count && ToBeDecoded.Length < m_BufferingSize; i++)
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

        private void LZ77Extract()
        {
            int CurrentCode = CodeUnkown;
           
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

                else if (CurrentCode == CodeDistance)
                {
                    string BackwardDistance = GetDistanceBits();

                    byte temp = Convert.ToByte(BackwardDistance.Substring(0, 8), 2);

                    m_BackwardDistances.Add(temp);

                    CurrentCode = CodeLength;
                }

                else if (CurrentCode == CodeLength)
                {
                    byte MatchLength = Convert.ToByte(GetLengthByte(), 2);

                    m_MatchLengths.Add(MatchLength);

                    CurrentCode = CodeUnkown;
                }

                else if (CurrentCode == CodeLiteral)
                {
                    byte Literal = Convert.ToByte(GetLiteralByte(), 2);

                    m_Literals.Add(Literal);

                    CurrentCode = CodeUnkown;
                }

                if (m_TextBuffer.Length == 0)
                {
                    BufferText();
                }
            }
        }


        public StreamExtractor(string FileName, int ExtractionAlgo)
        {
            m_FileName = FileName;
            m_Literals = new List<byte>();
            m_MatchLengths = new List<byte>();
            m_InputStream = new List<byte>();
            m_BackwardDistances = new List<byte>();
            m_LiteralsHeader = new List<byte>();
            m_MatchLengthsHeader = new List<byte>();
            m_BackwardDistancesHeader = new List<byte>();
            m_TextBuffer = "";
            m_BufferingSize = 10000;
            m_InputStreamIndex = 0;
            m_LiteralCodewordLength = 8;
            m_BackwardDistanceCodewordLength = 15;
            m_MatchLengthCodewordLength = 8;
            m_LiteralsHeaderSize = 180;
            m_MatchLengthsHeaderSize = 256;
            m_BackwardDistanceHeaderSize = 256;
            CodeUnkown = 0;
            CodeDistance = 1;
            CodeLength = 2;
            CodeLiteral = 3;
            Uncompressed = '0';
            Huffman = 0;
            LZ77 = 1;

            StrategySelection(ExtractionAlgo);
        }

        public List<byte> ExtractLiterals()
        {
            return m_Literals;
        }

        public List<byte> ExtractMatchLengths()
        {
            return m_MatchLengths;
        }

        public List<byte> ExtractBackwardDistances()
        {
            return m_BackwardDistances;
        }

        public List<byte> ExtractLiteralsHeader()
        {
            return m_LiteralsHeader;
        }

        public List<byte> ExtractMatchLengthsHeader()
        {
            return m_MatchLengthsHeader;
        }

        public List<byte> ExtractBackwardDistancesHeader()
        {
            return m_BackwardDistancesHeader;
        }

        public byte ExtractBytePadding()
        {
            return m_BytePadding;
        }
    }
}
