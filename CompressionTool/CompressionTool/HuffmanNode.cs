using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionTool
{
    class HuffmanNode : IComparable<HuffmanNode>
    {
        public byte Character;
        public string Code;
        public int Frequency;
        public bool IsLeaf;
        public HuffmanNode Parent;
        public HuffmanNode LeftChild;
        public HuffmanNode RightChild;

        public HuffmanNode(byte Symbol, int FreqeuncyVal)
        {
            Character = Symbol;
            Code = "";
            Frequency = FreqeuncyVal;
            Parent = LeftChild = RightChild = null;
            IsLeaf = true;
        }

        public HuffmanNode(HuffmanNode FirstNode, HuffmanNode SecondNode)
        {
            Code = "";
            IsLeaf = false;
            Parent = null;

            if(FirstNode.Frequency >= SecondNode.Frequency)
            {
                RightChild = FirstNode;
                LeftChild  = SecondNode;
                LeftChild.Parent = RightChild.Parent = this;
                Character = 200;
                Frequency = FirstNode.Frequency + RightChild.Frequency;
            }
            else
            {
                RightChild = SecondNode;
                LeftChild = FirstNode;
                LeftChild.Parent = RightChild.Parent = this;
                Character = 200;
                Frequency = FirstNode.Frequency + RightChild.Frequency;
            }
        }

        public int CompareTo(HuffmanNode otherNode) 
        {
            return this.Frequency.CompareTo(otherNode.Frequency);
        }
    }
}
