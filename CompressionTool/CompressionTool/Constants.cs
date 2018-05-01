using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionTool
{
    class Constants
    {
        static public int MinMatchLength = 4;
        static public int MinBackwardDistance = 1;
        static public int LiteralCodewordLength = 8;
        static public int BackwardDistanceCodewordLength = 18;
        static public int SearchBufferSize = (1 << BackwardDistanceCodewordLength);
        static public int MatchLengthCodewordLength = 8;
        static public int LookAheadBufferSize = (1 << MatchLengthCodewordLength) + 2;
        static public int BufferingSize = 10000;
        static public int CodeUnkown = 0;
        static public int CodeDistance = 1;
        static public int CodeLength = 2;
        static public int CodeLiteral = 3;
        static public char Uncompressed = '0';
        static public char Compressed = '1';
        static public int LiteralsHeaderSize = 180;
        static public int MatchLengthsHeaderSize = 256;
        static public int BackwardDistanceHeaderSize = 256;
        static public int BytePaddingSize = 1;
        static public int Bit = 1;
        static public int Byte = 8;
        static public int Infinity = 1000000000;
        static public int LZ77 = 1;
        static public int Huffman = 0;
    }
}
