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
        private Dictionary<char, int> m_CharactersCount;
        private List<HuffmanNode> m_HuffmanNodes;
        private Dictionary<char, string> m_EncodingDictionary;
        private List<byte> m_EncodedStream;
        
        private void GetHuffmanNodes()
        {
            foreach (KeyValuePair<char, int> entry in m_CharactersCount)
            {
                m_HuffmanNodes.Add(new HuffmanNode(entry.Key.ToString(), entry.Value));
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
                m_EncodingDictionary.Add(Node.Character[0], Node.Code);
                return;
            }

            BuildHuffmanTree(Code + "0", Node.LeftChild);
            BuildHuffmanTree(Code + "1", Node.RightChild);
        }

        private void GetEncodedOutput(string Text)
        {
            string EncodedText = "";

            for (int i = 0; i < Text.Length; i++)
            {
                EncodedText += m_EncodingDictionary[Text[i]];
                EncodedText = ToBinary(EncodedText);
            }

            if (EncodedText.Length > 0)
            {
                while (EncodedText.Length < 8)
                {
                    EncodedText += '0';
                }
                ToBinary(EncodedText);
            }
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

        private void OutputEncodedFile(string Text, string FileName)
        {
            GetEncodedOutput(Text);

            OutputWriter OutputWriter = new OutputWriter(FileName);
            OutputWriter.WriteToFile(m_EncodedStream);
        }

        public HuffmanEncoder()
        {
            m_CharactersCount = new Dictionary<char, int>();
            m_HuffmanNodes = new List<HuffmanNode>();
            m_EncodingDictionary = new Dictionary<char, string>();
            m_EncodedStream = new List<byte>();
        }

        public void Encode(Dictionary<char, int> CharactersCount, string Text, string FileName)
        {
            m_CharactersCount = CharactersCount;

            GetHuffmanNodes();

            ReduceHuffmanNodes(m_HuffmanNodes);

            BuildHuffmanTree("", m_HuffmanNodes[0]);

            OutputEncodedFile(Text, FileName);
        }
    }
}