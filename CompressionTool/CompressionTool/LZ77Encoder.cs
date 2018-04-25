using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionTool
{
    class Match
    {
        public int BackwardDistance;
        public int MatchLength;

        public Match (int Distance, int Length)
        {
            BackwardDistance = Distance;
            MatchLength = Length;
        }
    }

    class LZ77Encoder
    {
        private List<byte> m_InputStream;
        private List<byte> m_SearchBuffer;
        private List<byte> m_LookAheadBuffer;
        private List<byte> m_CompressedStream;
        private string m_CompressedOutput;
        private int m_LookAheadBufferMaxSize;
        private int m_SearchBufferMaxSize;
        private int m_InputStreamIndex;
        private byte m_BytePadding;
        
        private void PopulateLookAheadBuffer()
        {
            for(int i = 0; i < m_LookAheadBufferMaxSize && m_InputStreamIndex < m_InputStream.Count; i++)
            {
                m_LookAheadBuffer.Add(m_InputStream[m_InputStreamIndex]);

                m_InputStreamIndex++;
            }
        }

        private void AddByteToLookAheadBuffer()
        {
            if (m_InputStreamIndex >= m_InputStream.Count) return;

            byte InputByte = m_InputStream[m_InputStreamIndex];

            m_InputStreamIndex++;

            m_LookAheadBuffer.Add(InputByte);
        }

        private void SlideWindow(int SlidingDistance)
        {
            for(int i=0; i< SlidingDistance; i++)
            {
                byte ToBeRemovedFromLAB = m_LookAheadBuffer[0];

                m_LookAheadBuffer.RemoveAt(0);
                
                m_SearchBuffer.Add(ToBeRemovedFromLAB);

                AddByteToLookAheadBuffer();
            }

            RetireOldMatches();
        }

        private void RetireOldMatches()
        {
            if (m_SearchBuffer.Count <= m_SearchBufferMaxSize) return;

            while(m_SearchBuffer.Count > m_SearchBufferMaxSize)
            {
                m_SearchBuffer.RemoveAt(0);
            }
        }

        private Match GetLongestMatch()
        {
            int LookAheadBufferIndex = 0, MaxMatchLength = 0, MinBackwardDistance = Constants.Infinity;

            for(int SearchBufferIndex = m_SearchBuffer.Count - 1; SearchBufferIndex >= 0; SearchBufferIndex--)
            {
                int j = SearchBufferIndex, i = LookAheadBufferIndex;
                int MatchLength = 0, BackwardDistance = m_SearchBuffer.Count - SearchBufferIndex;

                while (i<m_LookAheadBuffer.Count && j<m_SearchBuffer.Count && m_LookAheadBuffer[i] == m_SearchBuffer[j])
                {
                    j++; i++;
                    MatchLength++;
                }

                if (MatchLength > MaxMatchLength)
                {
                    MaxMatchLength = MatchLength;
                    MinBackwardDistance = BackwardDistance;
                }
                else if(MatchLength == MaxMatchLength)
                {
                    if(BackwardDistance < MinBackwardDistance)
                    {
                        MinBackwardDistance = BackwardDistance;
                    }
                }
            }

            return new Match(MinBackwardDistance, MaxMatchLength);
        }

        private void WriteMatchLength(int Length)
        {
            string MatchLength = Convert.ToString(Length, 2);

            MatchLength = MatchLength.PadLeft(Constants.MatchLengthCodewordLength, '0');

            m_CompressedOutput += MatchLength;
        }

        private void WriteBackwardDistance(int BackwardDistance)
        {
            string Distance = Convert.ToString(BackwardDistance, 2);

            Distance = Distance.PadLeft(Constants.BackwardDistanceCodewordLength, '0');

            m_CompressedOutput += Distance;
        }

        private void WriteLiteral(byte Literal)
        {
            string Symbol = Convert.ToString(Literal, 2);

            Symbol = Symbol.PadLeft(Constants.LiteralCodewordLength, '0');

            m_CompressedOutput += Symbol;
        }

        private void WriteUncompressedLiteral(byte Literal)
        {
            m_CompressedOutput += Constants.Uncompressed;

            WriteLiteral(Literal);
        }

        private void WriteCompressedSequence(Match Match)
        {
            m_CompressedOutput += Constants.Compressed;

            WriteBackwardDistance(Match.BackwardDistance - Constants.MinBackwardDistance);

            WriteMatchLength(Match.MatchLength - Constants.MinMatchLength);
        }

        private string ToBinary()
        {
            int StartIndex = 0, ByteCount = 0;

            while (StartIndex <= m_CompressedOutput.Length - Constants.Byte)
            {
                byte StringByte = Convert.ToByte(m_CompressedOutput.Substring(StartIndex, Constants.Byte), 2);

                m_CompressedStream.Add(StringByte);

                StartIndex += Constants.Byte;

                ByteCount++;
            }

            return m_CompressedOutput.Substring(StartIndex, m_CompressedOutput.Length - Constants.Byte * ByteCount);
        }

        private void LZ77Encode()
        {
            while (m_LookAheadBuffer.Count > 0)
            {
                Match Match = GetLongestMatch();

                if (Match.MatchLength > Constants.MinMatchLength)
                {
                    WriteCompressedSequence(Match);

                    SlideWindow(Match.MatchLength);
                }

                else
                {
                    WriteUncompressedLiteral(m_LookAheadBuffer[0]);
                   
                    SlideWindow(1);
                }

                m_CompressedOutput = ToBinary();
            }

            if (m_CompressedOutput.Length > 0)
            {
                m_BytePadding = (byte)(Constants.Byte - m_CompressedOutput.Length);

                m_CompressedOutput = m_CompressedOutput.PadRight(Constants.Byte, '0');
                
                ToBinary();
            }

            m_CompressedStream.Insert(0, m_BytePadding);
        }

        private void ProduceFile(string FileName)
        {
            OutputWriter writer = new OutputWriter();

            writer.WriteToCompressionMetaData(m_CompressedStream, FileName);
        }

        public LZ77Encoder(int LookAheadBufferSize, int SearchBufferSize)
        {
            m_InputStream = new List<byte>();
            m_SearchBuffer = new List<byte>();
            m_LookAheadBuffer = new List<byte>();
            m_CompressedStream = new List<byte>();
            m_CompressedOutput = "";
            m_LookAheadBufferMaxSize = LookAheadBufferSize;
            m_SearchBufferMaxSize = SearchBufferSize;
            m_InputStreamIndex = 0;
            m_BytePadding = 0;
        }

        public void Encode(List<byte> InputStream, string FileName)
        {
            m_InputStream = InputStream;

            PopulateLookAheadBuffer();

            LZ77Encode();

            ProduceFile(FileName);
        }
    }
}
