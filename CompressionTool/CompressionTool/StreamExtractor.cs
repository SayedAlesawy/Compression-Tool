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
        private int m_InputStreamIndex;
        private byte m_BytePadding;
        private string m_TextBuffer;

        private void ExtractPadding()
        {
            m_BytePadding = m_InputStream[0];

            m_InputStream.RemoveAt(0);
        }

        private char GetFlagBit()
        {
            BufferText();

            char FlagBit = m_TextBuffer[0];

            m_TextBuffer = m_TextBuffer.Remove(0, Constants.Bit);

            return FlagBit;
        }

        private string GetLengthByte()
        {
            BufferText();

            string LengthByte = m_TextBuffer.Substring(0, Constants.MatchLengthCodewordLength);

            m_TextBuffer = m_TextBuffer.Remove(0, Constants.MatchLengthCodewordLength);

            return LengthByte;
        }

        private string GetLiteralByte()
        {
            BufferText();

            string LiteralByte = m_TextBuffer.Substring(0, Constants.LiteralCodewordLength);

            m_TextBuffer = m_TextBuffer.Remove(0, Constants.LiteralCodewordLength);

            return LiteralByte;
        }

        private string GetDistanceBits()
        {
            BufferText();

            string DistanceBits = m_TextBuffer.Substring(0, Constants.BackwardDistanceCodewordLength);

            m_TextBuffer = m_TextBuffer.Remove(0, Constants.BackwardDistanceCodewordLength);

            return DistanceBits;
        }

        private void TrimPadding()
        {
            string PaddedByte = Convert.ToString(m_InputStream[m_InputStream.Count - 1], 2);

            m_InputStreamIndex++;

            PaddedByte = PaddedByte.PadLeft(Constants.Byte, '0');

            string LastByte = PaddedByte.Substring(0, PaddedByte.Length - m_BytePadding);

            m_TextBuffer += LastByte;
        }

        private void BufferText()
        {
            if (m_TextBuffer.Length >= Constants.BufferingSize) return;

            String ToBeDecoded = "";

            for (int i = m_InputStreamIndex; i < m_InputStream.Count && m_TextBuffer.Length + ToBeDecoded.Length < Constants.BufferingSize; i++)
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

                    NewByte = NewByte.PadLeft(Constants.Byte, '0');

                    ToBeDecoded += NewByte;
                }
            }

            m_TextBuffer += ToBeDecoded;
        }

        private void ExtractMetaStreams()
        {
            int CurrentCode = Constants.CodeUnkown;
           
            BufferText();

            while (m_TextBuffer.Length > 0)
            {
                if (CurrentCode == Constants.CodeUnkown)
                {
                    char FlagBit = GetFlagBit();

                    if (FlagBit == Constants.Uncompressed)
                        CurrentCode = Constants.CodeLiteral;

                    else if(FlagBit == Constants.Compressed)
                        CurrentCode = Constants.CodeDistance;
                }

                else if (CurrentCode == Constants.CodeDistance)
                {
                    string BackwardDistance = GetDistanceBits();

                    byte HigherDistanceByte = Convert.ToByte(BackwardDistance.Substring(0, Constants.Byte), 2);

                    m_BackwardDistances.Add(HigherDistanceByte);

                    CurrentCode = Constants.CodeLength;
                }

                else if (CurrentCode == Constants.CodeLength)
                {
                    byte MatchLength = Convert.ToByte(GetLengthByte(), 2);

                    m_MatchLengths.Add(MatchLength);

                    CurrentCode = Constants.CodeUnkown;
                }

                else if (CurrentCode == Constants.CodeLiteral)
                {
                    byte Literal = Convert.ToByte(GetLiteralByte(), 2);

                    m_Literals.Add(Literal);

                    CurrentCode = Constants.CodeUnkown;
                }

                if (m_TextBuffer.Length == 0)
                {
                    BufferText();
                }
            }
        }

        public StreamExtractor(List<byte> InputStream)
        {
            m_Literals = new List<byte>();
            m_MatchLengths = new List<byte>();
            m_InputStream = InputStream;
            m_BackwardDistances = new List<byte>();
            m_TextBuffer = "";
            m_InputStreamIndex = 0;

            ExtractPadding();

            ExtractMetaStreams();
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

        public byte ExtractBytePadding()
        {
            return m_BytePadding;
        }
    }
}
