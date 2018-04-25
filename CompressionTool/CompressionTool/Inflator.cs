using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CompressionTool
{
    class Inflator
    {
        private Dictionary<string, byte> m_LiteralsCodeBook;
        private Dictionary<string, byte> m_MatchLengthsCodeBook;
        private Dictionary<string, byte> m_BackwardDistancesCodeBook;
        private List<byte> m_InputStream;
        private List<byte> m_LiteralsHeader;
        private List<byte> m_MatchLengthsHeader;
        private List<byte> m_BackwardDistancesHeader;
        private List<byte> m_OutputStream;
        private int m_BufferingSize;
        private int m_InputStreamIndex;
        private string m_TextBuffer;
        private int m_LiteralsHeaderSize;
        private int m_MatchLengthsHeaderSize;
        private int m_BackwardDistanceHeaderSize;
        private byte m_MetaBytePadding;
        private byte m_InputBytePadding;
        private int CodeUnkown;
        private int CodeDistance;
        private int CodeLength;
        private int CodeLiteral;
        private char Uncompressed;

        private void ReadCompressedFile(string FileName)
        {
            string FilePath = @"..\..\EncodedOutput\" + FileName + ".tsv";

            byte[] temp = File.ReadAllBytes(FilePath);

            m_InputStream = temp.ToList<byte>();
        }

        private void ExtractHeaders()
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

            m_InputBytePadding = m_InputStream[m_LiteralsHeaderSize + m_MatchLengthsHeaderSize + m_BackwardDistanceHeaderSize];

            m_InputStream.RemoveRange(0, m_LiteralsHeaderSize + m_MatchLengthsHeaderSize + m_BackwardDistanceHeaderSize + 1);
        }

        private void BuildCodeBooks()
        {
            HuffmanDecoder LiteralsDecoder = new HuffmanDecoder();
            m_LiteralsCodeBook = LiteralsDecoder.GetCodeBook(m_LiteralsHeader);

            HuffmanDecoder MatchLengthDecoder = new HuffmanDecoder();
            m_MatchLengthsCodeBook = MatchLengthDecoder.GetCodeBook(m_MatchLengthsHeader);

            HuffmanDecoder BackwardDistanceDecoder = new HuffmanDecoder();
            m_BackwardDistancesCodeBook = BackwardDistanceDecoder.GetCodeBook(m_BackwardDistancesHeader);
        }

        private char GetFlagBit()
        {
            if (m_TextBuffer.Length < 1)
                BufferText();

            char FlagBit = m_TextBuffer[0];

            m_TextBuffer = m_TextBuffer.Remove(0, 1);

            return FlagBit;
        }

        private string DecodeLiteral()
        {
            BufferText();

            string Partial = ""; string DecodedLiteral = "";

            for (int i = 0; i < m_TextBuffer.Length; i++)
            {
                Partial += m_TextBuffer[i];

                if (m_LiteralsCodeBook.ContainsKey(Partial))
                {
                    string tmp = Convert.ToString(m_LiteralsCodeBook[Partial], 2);
                    DecodedLiteral = tmp.PadLeft(8, '0');
                    m_TextBuffer = m_TextBuffer.Remove(0, i + 1);
                    break;
                }
            }

            return DecodedLiteral;
        }

        private string DecodeMatchLength()
        {
            BufferText();

            string Partial = ""; string DecodedMatchLength = "";

            for (int i = 0; i < m_TextBuffer.Length; i++)
            {
                Partial += m_TextBuffer[i];

                if (m_MatchLengthsCodeBook.ContainsKey(Partial))
                {
                    string tmp = Convert.ToString(m_MatchLengthsCodeBook[Partial], 2);
                    DecodedMatchLength = tmp.PadLeft(8, '0');
                    m_TextBuffer = m_TextBuffer.Remove(0, i + 1);
                    break;
                }
            }

            return DecodedMatchLength;
        }

        private string DecodeBackwardDistance()
        {
            BufferText();

            string Partial = ""; string DecodedBackwardDistance = "";

            for (int i = 0; i < m_TextBuffer.Length; i++)
            {
                Partial += m_TextBuffer[i];

                if (m_BackwardDistancesCodeBook.ContainsKey(Partial))
                {
                    string tmp = Convert.ToString(m_BackwardDistancesCodeBook[Partial], 2);
                    DecodedBackwardDistance = tmp.PadLeft(8, '0');
                    m_TextBuffer = m_TextBuffer.Remove(0, i + 1);
                    DecodedBackwardDistance += m_TextBuffer.Substring(0, 7);
                    m_TextBuffer = m_TextBuffer.Remove(0, 7);
                    break;
                }
            }

            return DecodedBackwardDistance;
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

        private void DecodeStream()
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
                    ToBeEncoded += DecodeBackwardDistance();

                    CurrentCode = CodeLength;
                }

                else if (CurrentCode == CodeLength)
                {
                    ToBeEncoded += DecodeMatchLength();

                    CurrentCode = CodeUnkown;
                }

                else if (CurrentCode == CodeLiteral)
                {
                    ToBeEncoded += DecodeLiteral();

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
                m_MetaBytePadding = (byte)(8 - ToBeEncoded.Length);

                ToBeEncoded = ToBeEncoded.PadRight(8, '0');

                ToBinary(ToBeEncoded);
            }

            m_OutputStream.Insert(0, m_MetaBytePadding);
        }

        private string ToBinary(string EncodedText)
        {
            int StartIndex = 0, ByteCount = 0;

            while (StartIndex <= EncodedText.Length - 8)
            {
                byte StringByte = Convert.ToByte(EncodedText.Substring(StartIndex, 8), 2);
                m_OutputStream.Add(StringByte);
                StartIndex += 8;
                ByteCount++;
            }

            return EncodedText.Substring(StartIndex, EncodedText.Length - 8 * ByteCount);
        }

        private void ProduceOutputFile(string FileName)
        {
            OutputWriter Writer = new OutputWriter(FileName);

            Writer.WriteToInverseMetaFile(m_OutputStream);
        }

        public Inflator()
        {
            m_LiteralsCodeBook = new Dictionary<string, byte>();
            m_MatchLengthsCodeBook = new Dictionary<string, byte>();
            m_BackwardDistancesCodeBook = new Dictionary<string, byte>();
            m_InputStream = new List<byte>();
            m_LiteralsHeader = new List<byte>();
            m_MatchLengthsHeader = new List<byte>();
            m_BackwardDistancesHeader = new List<byte>();
            m_OutputStream = new List<byte>();
            m_LiteralsHeaderSize = 180;
            m_MatchLengthsHeaderSize = 256;
            m_BackwardDistanceHeaderSize = 256;
            m_InputBytePadding = 0;
            m_MetaBytePadding = 0;
            m_BufferingSize = 10000;
            m_InputStreamIndex = 0;
            m_TextBuffer = "";
            CodeUnkown = 0;
            CodeDistance = 1;
            CodeLength = 2;
            CodeLiteral = 3;
            Uncompressed = '0';
        }

        public void Inflate(string FileName)
        {
            ReadCompressedFile(FileName);

            ExtractHeaders();

            BuildCodeBooks();

            DecodeStream();

            ProduceOutputFile(FileName);
        }
    }
}
