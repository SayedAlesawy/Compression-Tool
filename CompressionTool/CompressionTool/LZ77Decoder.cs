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
        private StringBuilder m_DecodedOutput;
        private List<byte> m_OutputStream;
        private Dictionary<char, byte> m_Alphabet;
        private Dictionary<byte, char> m_InverseAlphabet;
        private byte m_BytePadding;
        private int m_InputStreamIndex;
        private int m_SearchBufferMaxSize;
        private string m_TextBuffer;

        private void ReadCompressedFile(string FileName)
        {
            InputReader InputReader = new InputReader();

            m_InputStream = InputReader.ReadDecompressionMetaData(FileName);

            m_BytePadding = m_InputStream[0];

            m_InputStream.RemoveAt(0);
        }

        private void LoadSymbolDictionary()
        {
            InputReader InputReader = new InputReader();

            m_Alphabet = InputReader.ReadSymbolDictionary();
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

        private void LZ77Decode()
        {
            int CurrentCode = Constants.CodeUnkown;
            int BackwardDistance = 0, MatchLength = 0;

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

                else if(CurrentCode == Constants.CodeDistance)
                {
                    BackwardDistance = Convert.ToInt32(GetDistanceBits(), 2) + Constants.MinBackwardDistance;

                    CurrentCode = Constants.CodeLength;
                }

                else if(CurrentCode == Constants.CodeLength)
                {
                    MatchLength = Convert.ToInt32(GetLengthByte(), 2) + Constants.MinMatchLength;

                    List<byte> ToBeDecoded = new List<byte>();
                    int Start = m_SearchBuffer.Count - BackwardDistance, End = Start + MatchLength;

                    for (int i = Start; i < End; i++)
                    {
                        ToBeDecoded.Add(m_SearchBuffer[i]);

                        m_OutputStream.Add(m_SearchBuffer[i]);
                    }

                    SlideWindow(ToBeDecoded);

                    CurrentCode = Constants.CodeUnkown;

                    BackwardDistance = 0; MatchLength = 0;
                }

                else if(CurrentCode == Constants.CodeLiteral)
                {
                    byte Literal = Convert.ToByte(GetLiteralByte(), 2);

                    m_SearchBuffer.Add(Literal);

                    RetireOldMatches();

                    m_OutputStream.Add(Literal);

                    CurrentCode = Constants.CodeUnkown;
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

            OutputWriter OutputWriter = new OutputWriter();

            OutputWriter.WriteFinalDecompressedFile(m_DecodedOutput.ToString(), FileName);
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
            m_SearchBufferMaxSize = SearchBufferMaxSize;
        }

        public int Decode(string FileName)
        {
            BuildInverseSymbolDictionary();

            ReadCompressedFile(FileName);

            LZ77Decode();

            ProduceOutputFile(FileName);

            return m_OutputStream.Count;
        }
    }
}
