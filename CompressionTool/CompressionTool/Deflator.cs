using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CompressionTool
{
    class Deflator
    {
        private Dictionary<byte, string> m_LiteralsCodeBook;
        private Dictionary<byte, string> m_MatchLengthsCodeBook;
        private Dictionary<byte, string> m_BackwardDistanceCodeBook;
        private Dictionary<byte, int> m_LiteralsCount;
        private Dictionary<byte, int> m_MatchLengthCount;
        private Dictionary<byte, int> m_BackwardDistanceCount;
        private List<byte> m_BackwardDistanceHeader;
        private List<byte> m_LiteralsHeader;
        private List<byte> m_MatchLengthsHeader;
        private List<byte> m_InputStream;
        private List<byte> m_CompressedOutput;
        private List<byte> m_LiteralsStream;
        private List<byte> m_MatchLengthsStream;
        private List<byte> m_BackwardDistanceStream;
        private List<byte> m_OriginalFile;
        private int m_InputStreamIndex;
        private byte m_InputBytePadding;
        private byte m_OutputBytePadding;
        private string m_TextBuffer;

        private void LoadMetaData(string FileName)
        {
            InputReader InputReader = new InputReader();

            m_InputStream = InputReader.ReadCompressionMetaData(FileName);
            
            m_InputBytePadding = m_InputStream[0];
            
            m_InputStream.RemoveAt(0);
        }

        private void ConvertOriginalFileToBytes(string FileName)
        {
            TextToByteConverter TextToByteConverter = new TextToByteConverter(FileName);

            m_OriginalFile = TextToByteConverter.Convert();
        }

        private void EncodeLZ77(string FileName)
        {
            LZ77Encoder LZ77Encoder = new LZ77Encoder(Constants.LookAheadBufferSize, Constants.SearchBufferSize);
            LZ77Encoder.Encode(m_OriginalFile, FileName);
        }

        private void ExtractStreams(string FileName)
        {
            StreamExtractor StreamExtractor = new StreamExtractor(FileName);

            m_LiteralsStream = StreamExtractor.ExtractLiterals();

            m_MatchLengthsStream = StreamExtractor.ExtractMatchLengths();

            m_BackwardDistanceStream = StreamExtractor.ExtractBackwardDistances();
        }

        private void GetSymbolFrequencies()
        {
            Probability LiteralsProbability = new Probability(m_LiteralsStream);
            m_LiteralsCount = LiteralsProbability.GetCharactersCount();

            Probability MatchLenghtsProbability = new Probability(m_MatchLengthsStream);
            m_MatchLengthCount = MatchLenghtsProbability.GetCharactersCount();

            Probability BackwardDistanceProbability = new Probability(m_BackwardDistanceStream);
            m_BackwardDistanceCount = BackwardDistanceProbability.GetCharactersCount();
        }

        private void GetCodeBooksAndHeaders()
        {
            HuffmanEncoder LiteralsHuffmanTree = new HuffmanEncoder();
            m_LiteralsCodeBook = LiteralsHuffmanTree.GetCodeBook(m_LiteralsCount);
            m_LiteralsHeader = LiteralsHuffmanTree.GetHeader(Constants.LiteralsHeaderSize);

            HuffmanEncoder MatchLengthsHuffmanTree = new HuffmanEncoder();
            m_MatchLengthsCodeBook = MatchLengthsHuffmanTree.GetCodeBook(m_MatchLengthCount);
            m_MatchLengthsHeader = MatchLengthsHuffmanTree.GetHeader(Constants.MatchLengthsHeaderSize);

            HuffmanEncoder BackwardDistanceHuffmanTree = new HuffmanEncoder();
            m_BackwardDistanceCodeBook = BackwardDistanceHuffmanTree.GetCodeBook(m_BackwardDistanceCount);
            m_BackwardDistanceHeader = BackwardDistanceHuffmanTree.GetHeader(Constants.BackwardDistanceHeaderSize);
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

        private void EncodeHuffman()
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
                    string BackwardDistance = GetDistanceBits();

                    byte HigherByte = Convert.ToByte(BackwardDistance.Substring(0, Constants.Byte), 2);

                    ToBeEncoded += m_BackwardDistanceCodeBook[HigherByte];

                    ToBeEncoded += BackwardDistance.Substring(Constants.Byte, Constants.BackwardDistanceCodewordLength - Constants.Byte);

                    CurrentCode = Constants.CodeLength;
                }

                else if (CurrentCode == Constants.CodeLength)
                {
                    byte MatchLength = Convert.ToByte(GetLengthByte(), 2);

                    ToBeEncoded += m_MatchLengthsCodeBook[MatchLength];

                    CurrentCode = Constants.CodeUnkown;
                }

                else if (CurrentCode == Constants.CodeLiteral)
                {
                    byte Literal = Convert.ToByte(GetLiteralByte(), 2);

                    ToBeEncoded += m_LiteralsCodeBook[Literal];

                    CurrentCode = Constants.CodeUnkown;
                }

                if (m_TextBuffer.Length == 0)
                {
                    BufferText();
                }

                ToBeEncoded = ToBinary(ToBeEncoded);
            }

            if (ToBeEncoded.Length > 0)
            {
                m_OutputBytePadding = (byte)(Constants.Byte - ToBeEncoded.Length);

                ToBeEncoded = ToBeEncoded.PadRight(Constants.Byte, '0');

                ToBinary(ToBeEncoded);
            }

            m_CompressedOutput.Insert(Constants.LiteralsHeaderSize + Constants.MatchLengthsHeaderSize + Constants.BackwardDistanceHeaderSize, m_OutputBytePadding);
        }

        private string ToBinary(string EncodedText)
        {
            int StartIndex = 0, ByteCount = 0;

            while (StartIndex <= EncodedText.Length - Constants.Byte)
            {
                byte StringByte = Convert.ToByte(EncodedText.Substring(StartIndex, Constants.Byte), 2);

                m_CompressedOutput.Add(StringByte);

                StartIndex += Constants.Byte;

                ByteCount++;
            }

            return EncodedText.Substring(StartIndex, EncodedText.Length - Constants.Byte * ByteCount);
        }

        private void BuildHeader()
        {
            for (int i = 0; i < m_LiteralsHeader.Count; i++)
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
            OutputWriter writer = new OutputWriter();

            writer.WriteFinalCompressedFile(m_CompressedOutput, FileName);
        }

        public Deflator()
        {
            m_LiteralsCodeBook = new Dictionary<byte, string>();
            m_MatchLengthsCodeBook = new Dictionary<byte, string>();
            m_BackwardDistanceCodeBook = new Dictionary<byte, string>();
            m_LiteralsCount = new Dictionary<byte, int>();
            m_MatchLengthCount = new Dictionary<byte, int>();
            m_BackwardDistanceCount = new Dictionary<byte, int>();
            m_BackwardDistanceHeader = new List<byte>();
            m_LiteralsHeader = new List<byte>();
            m_MatchLengthsHeader = new List<byte>();
            m_InputStream = new List<byte>();
            m_CompressedOutput = new List<byte>();
            m_LiteralsStream = new List<byte>();
            m_MatchLengthsStream = new List<byte>();
            m_BackwardDistanceStream = new List<byte>();
            m_OriginalFile = new List<byte>();
            m_TextBuffer = "";
            m_InputStreamIndex = 0;
            m_OutputBytePadding = 0;
            m_InputBytePadding = 0;
        }

        public int Deflate(string FileName)
        {
            ConvertOriginalFileToBytes(FileName);

            EncodeLZ77(FileName);

            ExtractStreams(FileName);

            GetSymbolFrequencies();

            GetCodeBooksAndHeaders();

            LoadMetaData(FileName);

            BuildHeader();

            EncodeHuffman();

            ProduceFile(FileName);

            return m_CompressedOutput.Count;
        }
    }
}
