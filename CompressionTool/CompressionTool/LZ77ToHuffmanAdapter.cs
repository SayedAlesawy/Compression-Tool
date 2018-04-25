using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CompressionTool
{
    class LZ77ToHuffmanAdapter
    {
        private Dictionary<byte, string> m_LiteralsCodeBook;
        private Dictionary<byte, string> m_MatchLengthsCodebook;
        private Dictionary<byte, string> m_BackwardDistanceCodebook;
        private List<byte> m_BackwardDistanceHeader;
        private List<byte> m_LiteralsHeader;
        private List<byte> m_MatchLengthsHeader;
        private List<byte> m_InputStream;
        private List<byte> m_CompressedOutput;
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
        private byte m_InputBytePadding;
        private byte m_OutputBytePadding;
        private string m_TextBuffer;

        private void LoadFileLZ77(string FileName)
        {
            string FilePath = @"..\..\EncodedMetaOutput\" + FileName + ".tsv";

            byte[] temp = File.ReadAllBytes(FilePath);

            m_InputBytePadding = temp[0];

            m_InputStream = temp.ToList<byte>();

            m_InputStream.RemoveAt(0);

            //for (int i = 0; i < 101; i++) Console.WriteLine("{0}", m_InputStream[i]);
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

            string LastByte = tmp.Substring(0, tmp.Length - m_InputBytePadding);

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

        private void LZ77ToHuffman()
        {
            int CurrentCode = CodeUnkown;
            string ToBeEncoded = "";

            BufferText();

            while (m_TextBuffer.Length > 0)
            {
                if (CurrentCode == CodeUnkown)
                {
                    char FlagBit = GetFlagBit();
                    ToBeEncoded += FlagBit;

                    if (FlagBit == Uncompressed)
                        CurrentCode = CodeLiteral;

                    else
                        CurrentCode = CodeDistance;
                }

                else if (CurrentCode == CodeDistance)
                {
                    string BackwardDistance = GetDistanceBits();
                    byte temp = Convert.ToByte(BackwardDistance.Substring(0, 8), 2);
                    ToBeEncoded += m_BackwardDistanceCodebook[temp];

                    ToBeEncoded += BackwardDistance.Substring(8, 7);

                    CurrentCode = CodeLength;
                }

                else if (CurrentCode == CodeLength)
                {
                    byte MatchLength = Convert.ToByte(GetLengthByte(), 2);

                    ToBeEncoded += m_MatchLengthsCodebook[MatchLength];

                    CurrentCode = CodeUnkown;
                }

                else if (CurrentCode == CodeLiteral)
                {
                    byte Literal = Convert.ToByte(GetLiteralByte(), 2);

                    ToBeEncoded += m_LiteralsCodeBook[Literal];

                    CurrentCode = CodeUnkown;
                }

                if (m_TextBuffer.Length == 0)
                {
                    BufferText();
                }

                ToBeEncoded = ToBinary(ToBeEncoded);
            }

            if (ToBeEncoded.Length > 0)
            {
                m_OutputBytePadding = (byte)(8 - ToBeEncoded.Length);

                ToBeEncoded = ToBeEncoded.PadRight(8, '0');

                ToBinary(ToBeEncoded);
            }

            m_CompressedOutput.Insert(180 + 256 + 256, m_OutputBytePadding);
        }

        private string ToBinary(string EncodedText)
        {
            int StartIndex = 0, ByteCount = 0;

            while (StartIndex <= EncodedText.Length - 8)
            {
                byte StringByte = Convert.ToByte(EncodedText.Substring(StartIndex, 8), 2);
                m_CompressedOutput.Add(StringByte);
                StartIndex += 8;
                ByteCount++;
            }

            return EncodedText.Substring(StartIndex, EncodedText.Length - 8 * ByteCount);
        }

        private void BuildHeader()
        {
            for(int i = 0; i < m_LiteralsHeader.Count; i++)
            {
                m_CompressedOutput.Add(m_LiteralsHeader[i]);
            }

            for (int i = 0; i < m_MatchLengthsHeader.Count; i++)
            {
                m_CompressedOutput.Add(m_MatchLengthsHeader[i]);
            }

            for (int i = 0; i < m_BackwardDistanceHeader.Count; i++)
            {
                m_CompressedOutput.Add(m_BackwardDistanceHeader[i]);
            }
        }

        private void ProduceFile(string FileName)
        {
            OutputWriter writer = new OutputWriter(FileName);

            writer.WriteToFile(m_CompressedOutput);
        }

        public LZ77ToHuffmanAdapter(Dictionary<byte, string> LiteralsCodeBook, List<byte> LiteralsHeader, 
            Dictionary<byte, string> MatchLengthsCodebook, List<byte> MatchLengthsHeader,
            Dictionary<byte, string> BackwardDistanceCodeBook, List<byte> BackwardDistanceHeader)
        {
            m_LiteralsCodeBook = LiteralsCodeBook;
            m_LiteralsHeader = LiteralsHeader;
            m_MatchLengthsCodebook = MatchLengthsCodebook;
            m_MatchLengthsHeader = MatchLengthsHeader;
            m_BackwardDistanceCodebook = BackwardDistanceCodeBook;
            m_BackwardDistanceHeader = BackwardDistanceHeader;
            m_InputStream = new List<byte>();
            m_CompressedOutput = new List<byte>();
            m_TextBuffer = "";
            m_BufferingSize = 10000;
            m_InputStreamIndex = 0;
            m_LiteralCodewordLength = 8;
            m_BackwardDistanceCodewordLength = 15;
            m_MatchLengthCodewordLength = 8;
            CodeUnkown = 0;
            CodeDistance = 1;
            CodeLength = 2;
            CodeLiteral = 3;
            Uncompressed = '0';
            m_OutputBytePadding = 0;
            m_InputBytePadding = 0;
        }

        public void Deflate(string FileName)
        {
            LoadFileLZ77(FileName);

            BuildHeader();

            LZ77ToHuffman();

            ProduceFile(FileName);


        }
    }
}
