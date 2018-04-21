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
        private Dictionary<char, string> m_CanonicalEncodingDictionary;
        private Dictionary<char, int> m_Alphabet;
        private List<byte> m_EncodedStream;
        private byte m_BytePadding;

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

        private void GetCanonicalCodes()
        {
            IOrderedEnumerable<KeyValuePair<char, string>> sortedCollection = 
                m_EncodingDictionary.OrderBy(x => x.Value.Length).ThenBy(x => x.Key);

            Dictionary<char, string> Temp = new Dictionary<char, string>();

            Temp = sortedCollection.ToDictionary(pair => pair.Key, pair => pair.Value);

            string code = ""; bool f = true;
            foreach(KeyValuePair<char, string> entry in Temp)
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

        private void GetEncodedOutput(string Text)
        {
            string EncodedText = "";

            for (int i = 0; i < Text.Length; i++)
            {
                char c = Text[i];
                EncodedText += m_CanonicalEncodingDictionary[Text[i]];
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

            m_EncodedStream.Insert(m_Alphabet.Count, m_BytePadding);
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

       
        private void BuildHeader()
        {
            foreach(KeyValuePair<char, int> entry in m_Alphabet)
            {
                if (m_CanonicalEncodingDictionary.ContainsKey(entry.Key))
                {
                    byte CodeLength = (byte) m_CanonicalEncodingDictionary[entry.Key].Length;
                    m_EncodedStream.Add(CodeLength);
                }
                else
                {
                    m_EncodedStream.Add(0);
                }
            }
        }

        private void LoadSymbolDictionary()
        {
            int id = 0;

            String Text = System.IO.File.ReadAllText(@"..\..\SymbolDictionary.txt", Encoding.UTF8);

            for (int i = 0; i < Text.Length; i++)
            {
                if (m_Alphabet.ContainsKey(Text[i]))
                    continue;

                m_Alphabet.Add(Text[i], id);
                id++;
            }
        }

        public HuffmanEncoder()
        {
            m_CharactersCount = new Dictionary<char, int>();
            m_HuffmanNodes = new List<HuffmanNode>();
            m_EncodingDictionary = new Dictionary<char, string>();
            m_CanonicalEncodingDictionary = new Dictionary<char, string>();
            m_Alphabet = new Dictionary<char, int>();
            m_EncodedStream = new List<byte>();
            m_BytePadding = 0;
        }

        public void Encode(Dictionary<char, int> CharactersCount, string Text, string FileName)
        {
            LoadSymbolDictionary();

            m_CharactersCount = CharactersCount;

            GetHuffmanNodes();

            ReduceHuffmanNodes(m_HuffmanNodes);

            BuildHuffmanTree("", m_HuffmanNodes[0]);

            GetCanonicalCodes();

            BuildHeader();

            OutputEncodedFile(Text, FileName);
        }
    }
}