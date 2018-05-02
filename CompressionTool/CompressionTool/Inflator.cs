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
        private int m_InputStreamIndex;
        private string m_TextBuffer;
        private byte m_MetaBytePadding;
        private byte m_InputBytePadding;
        
        private void ReadCompressedFile(string FileName)
        {
            InputReader InputReader = new InputReader();

            m_InputStream = InputReader.ReadFinalEncodedFile(FileName);
        }

        private void ExtractHeaders()
        {
            for (int i = 0; i < Constants.LiteralsHeaderSize; i++)
            {
                m_LiteralsHeader.Add(m_InputStream[i]);
            }

            for (int i = Constants.LiteralsHeaderSize; i < Constants.LiteralsHeaderSize + Constants.MatchLengthsHeaderSize; i++)
            {
                m_MatchLengthsHeader.Add(m_InputStream[i]);
            }

            for (int i = Constants.LiteralsHeaderSize + Constants.MatchLengthsHeaderSize; i < Constants.LiteralsHeaderSize + Constants.MatchLengthsHeaderSize + Constants.BackwardDistanceHeaderSize; i++)
            {
                m_BackwardDistancesHeader.Add(m_InputStream[i]);
            }

            m_InputBytePadding = m_InputStream[Constants.LiteralsHeaderSize + Constants.MatchLengthsHeaderSize + Constants.BackwardDistanceHeaderSize];

            m_InputStream.RemoveRange(0, Constants.LiteralsHeaderSize + Constants.MatchLengthsHeaderSize + Constants.BackwardDistanceHeaderSize + Constants.BytePaddingSize);
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
            BufferText();

            char FlagBit = m_TextBuffer[0];

            m_TextBuffer = m_TextBuffer.Remove(0, Constants.Bit);

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
                    string CodeWord = Convert.ToString(m_LiteralsCodeBook[Partial], 2);

                    DecodedLiteral = CodeWord.PadLeft(Constants.Byte, '0');

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
                    string CodeWord = Convert.ToString(m_MatchLengthsCodeBook[Partial], 2);

                    DecodedMatchLength = CodeWord.PadLeft(Constants.Byte, '0');

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
                    string CodeWord = Convert.ToString(m_BackwardDistancesCodeBook[Partial], 2);

                    DecodedBackwardDistance = CodeWord.PadLeft(Constants.Byte, '0');

                    m_TextBuffer = m_TextBuffer.Remove(0, i + 1);

                    DecodedBackwardDistance += m_TextBuffer.Substring(0, Constants.BackwardDistanceCodewordLength - Constants.Byte);

                    m_TextBuffer = m_TextBuffer.Remove(0, Constants.BackwardDistanceCodewordLength - Constants.Byte);

                    break;
                }
            }

            return DecodedBackwardDistance;
        }

        private void TrimPadding()
        {
            string PaddedByte = Convert.ToString(m_InputStream[m_InputStream.Count - 1], 2);

            m_InputStreamIndex++;

            PaddedByte = PaddedByte.PadLeft(Constants.Byte, '0');

            string LastByte = PaddedByte.Substring(0, PaddedByte.Length - m_InputBytePadding);

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

        private void DecodeStream()
        {
            int CurrentCode = Constants.CodeUnkown;
            string ToBeEncoded = "";

            BufferText();

            while (m_TextBuffer.Length > 0)
            {
                if (CurrentCode == Constants.CodeUnkown)
                {
                    char FlagBit = GetFlagBit();

                    ToBeEncoded += FlagBit;

                    if (FlagBit == Constants.Uncompressed)
                        CurrentCode = Constants.CodeLiteral;

                    else if(FlagBit == Constants.Compressed)
                        CurrentCode = Constants.CodeDistance;
                }

                else if (CurrentCode == Constants.CodeDistance)
                {
                    ToBeEncoded += DecodeBackwardDistance();

                    CurrentCode = Constants.CodeLength;
                }

                else if (CurrentCode == Constants.CodeLength)
                {
                    ToBeEncoded += DecodeMatchLength();

                    CurrentCode = Constants.CodeUnkown;
                }

                else if (CurrentCode == Constants.CodeLiteral)
                {
                    ToBeEncoded += DecodeLiteral();

                    CurrentCode = Constants.CodeUnkown;
                }

                if (m_TextBuffer.Length == 0) BufferText();
                
                ToBeEncoded = ToBinary(ToBeEncoded);
            }

            if (ToBeEncoded.Length > 0)
            {
                m_MetaBytePadding = (byte)(Constants.Byte - ToBeEncoded.Length);

                ToBeEncoded = ToBeEncoded.PadRight(Constants.Byte, '0');

                ToBinary(ToBeEncoded);
            }

            m_OutputStream.Insert(0, m_MetaBytePadding);
        }

        private string ToBinary(string EncodedText)
        {
            int StartIndex = 0, ByteCount = 0;

            while (StartIndex <= EncodedText.Length - Constants.Byte)
            {
                byte StringByte = Convert.ToByte(EncodedText.Substring(StartIndex, Constants.Byte), 2);

                m_OutputStream.Add(StringByte);

                StartIndex += Constants.Byte;

                ByteCount++;
            }

            return EncodedText.Substring(StartIndex, EncodedText.Length - Constants.Byte * ByteCount);
        }

        private int DecodeFinal(string FileName)
        {
            LZ77Decoder LZ77Decoder = new LZ77Decoder(Constants.SearchBufferSize, m_OutputStream);

            return LZ77Decoder.Decode(FileName);
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
            m_InputBytePadding = 0;
            m_MetaBytePadding = 0;
            m_InputStreamIndex = 0;
            m_TextBuffer = "";
        }

        public int Inflate(string FileName)
        {
            ReadCompressedFile(FileName);

            ExtractHeaders();

            BuildCodeBooks();

            DecodeStream();

            return DecodeFinal(FileName);
        }
    }
}
