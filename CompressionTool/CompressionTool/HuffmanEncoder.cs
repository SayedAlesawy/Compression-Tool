using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CompressionTool
{
    class HuffmanEncoder
    {
        private Dictionary<byte, int> m_CharactersCount;
        private List<HuffmanNode> m_HuffmanNodes;
        private Dictionary<byte, string> m_EncodingDictionary;
        private Dictionary<byte, string> m_CanonicalEncodingDictionary;
        private List<byte> m_EncodedStream;
        private List<byte> m_InputStream;
        private int m_HeaderSize;
        private byte m_BytePadding;

        private void GetHuffmanNodes()
        {
            foreach (KeyValuePair<byte, int> entry in m_CharactersCount)
            {
                m_HuffmanNodes.Add(new HuffmanNode(entry.Key, entry.Value));
            }

            m_HuffmanNodes.Sort();
        }

        private void ReduceHuffmanNodes(List<HuffmanNode> nodeList)
        {
            while (nodeList.Count > 1)
            {
                HuffmanNode FirstNode = nodeList[0];
                nodeList.RemoveAt(0);

                HuffmanNode SecondNode = nodeList[0];
                nodeList.RemoveAt(0);

                nodeList.Add(new HuffmanNode(FirstNode, SecondNode));

                nodeList.Sort();
            }
        }

        private void BuildHuffmanTree(string Code, HuffmanNode Node)
        {
            if (Node == null) return;

            if (Node.LeftChild == null && Node.RightChild == null)
            {
                Node.Code = Code;
                m_EncodingDictionary.Add(Node.Character, Node.Code);
                return;
            }

            BuildHuffmanTree(Code + "0", Node.LeftChild);
            BuildHuffmanTree(Code + "1", Node.RightChild);
        }

        private void GetCanonicalCodes()
        {
            IOrderedEnumerable<KeyValuePair<byte, string>> sortedCollection = 
                m_EncodingDictionary.OrderBy(x => x.Value.Length).ThenBy(x => x.Key);

            Dictionary<byte, string> Temp = new Dictionary<byte, string>();

            Temp = sortedCollection.ToDictionary(pair => pair.Key, pair => pair.Value);

            string code = ""; bool f = true;
            foreach(KeyValuePair<byte, string> entry in Temp)
            {
                if (f)
                {
                    for (int i = 0; i < entry.Value.Length; i++) code += '0';

                    m_CanonicalEncodingDictionary.Add(entry.Key, code);

                    f = false;
                }
                else
                {
                    int PreSize = code.Length;

                    long next = Convert.ToInt64(code, 2); next++;
                    
                    code = Convert.ToString(next, 2);

                    int LeftPadding = PreSize - code.Length; string Padding = "";

                    for (int p = 0; p < LeftPadding; p++) Padding += '0';

                    code = Padding + code;

                    int cnt = entry.Value.Length - code.Length;

                    for (int i = 0; i < cnt; i++) code += '0';

                    m_CanonicalEncodingDictionary.Add(entry.Key, code);
                }
            }
        }

        private void GetEncodedOutput()
        {
            string EncodedText = "";

            for (int i = 0; i < m_InputStream.Count; i++)
            {
                byte b = m_InputStream[i];
                EncodedText += m_CanonicalEncodingDictionary[m_InputStream[i]];
                EncodedText = ToBinary(EncodedText);
            }

            if (EncodedText.Length > 0)
            {
                while (EncodedText.Length < 8)
                {
                    EncodedText += '0';
                    m_BytePadding++;
                }
                ToBinary(EncodedText);
            }

            m_EncodedStream.Insert(m_HeaderSize, m_BytePadding);
        }

        private string ToBinary(string EncodedText)
        {
            int StartIndex = 0, ByteCount = 0;

            while (StartIndex <= EncodedText.Length - 8)
            {
                byte StringByte = Convert.ToByte(EncodedText.Substring(StartIndex, 8), 2);
                m_EncodedStream.Add(StringByte);
                StartIndex += 8;
                ByteCount++;
            }

            return EncodedText.Substring(StartIndex, EncodedText.Length - 8 * ByteCount);
        }

        private void OutputEncodedFile(string FileName)
        {
            GetEncodedOutput();

            OutputWriter OutputWriter = new OutputWriter(FileName);
            OutputWriter.WriteToFile(m_EncodedStream);
        }

       
        private void BuildHeader()
        {
            for (byte i = 0; i < m_HeaderSize; i++) 
            {
                if (m_CanonicalEncodingDictionary.ContainsKey(i))
                {
                    byte CodeLength = (byte) m_CanonicalEncodingDictionary[i].Length;
                    m_EncodedStream.Add(CodeLength);
                }
                else
                {
                    m_EncodedStream.Add(0);
                }
            }
        }

        public HuffmanEncoder()
        {
            m_CharactersCount = new Dictionary<byte, int>();
            m_HuffmanNodes = new List<HuffmanNode>();
            m_EncodingDictionary = new Dictionary<byte, string>();
            m_CanonicalEncodingDictionary = new Dictionary<byte, string>();
            m_InputStream = new List<byte>();
            m_EncodedStream = new List<byte>();
            m_BytePadding = 0;
            m_HeaderSize = 180;
        }

        public void Encode(Dictionary<byte, int> CharactersCount, List<byte> InputStream, string FileName)
        {
            m_InputStream = InputStream;

            m_CharactersCount = CharactersCount;

            GetHuffmanNodes();

            ReduceHuffmanNodes(m_HuffmanNodes);

            BuildHuffmanTree("", m_HuffmanNodes[0]);

            GetCanonicalCodes();

            BuildHeader();

            OutputEncodedFile(FileName);
        }
    }
}