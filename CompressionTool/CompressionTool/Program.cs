using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionTool
{
    class Program
    {
        static void Main(string[] args)
        {
            double TotalTime = 0.0;

            for (int file = 1; file <= 1; file++)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();

                string FileName = "DataSet_" + file.ToString();

                
                TextToByteConverter TextToByteConverter = new TextToByteConverter(FileName);
                List<byte> InputStream = TextToByteConverter.Convert();

                int SearchBufferMaxSize = 32;

                LZ77Encoder LZ77Encoder = new LZ77Encoder(258, SearchBufferMaxSize * 1024);
                LZ77Encoder.Encode(InputStream, FileName);

                int LZ77 = 1, Huffman = 0;
                
                StreamExtractor StreamExtractor = new StreamExtractor(FileName, LZ77);
                List<byte> Literals = StreamExtractor.ExtractLiterals();
                List<byte> MatchLengths = StreamExtractor.ExtractMatchLengths();
                List<byte> BackwardDistance = StreamExtractor.ExtractBackwardDistances();

                Probability LiteralsProbability = new Probability(Literals);
                Dictionary<byte, int> LiteralsCount = LiteralsProbability.GetCharactersCount();

                Probability MatchLenghtsProbability = new Probability(MatchLengths);
                Dictionary<byte, int> MatchLengthsCount = MatchLenghtsProbability.GetCharactersCount();

                Probability BackwardDistanceProbability = new Probability(BackwardDistance);
                Dictionary<byte, int> BackwardDistanceCount = BackwardDistanceProbability.GetCharactersCount();

                HuffmanEncoder LiteralsHuffmanTree = new HuffmanEncoder();
                Dictionary<byte, string> LiteralsCodeBook =  LiteralsHuffmanTree.GetCodeBook(LiteralsCount);
                List<byte> LiteralsHeader = LiteralsHuffmanTree.GetHeader(180);
                
                HuffmanEncoder MatchLengthsHuffmanTree = new HuffmanEncoder();
                Dictionary<byte, string> MatchLengthsCodeBook = MatchLengthsHuffmanTree.GetCodeBook(MatchLengthsCount);
                List<byte> MatchLenghtsHeader = MatchLengthsHuffmanTree.GetHeader(256);

                HuffmanEncoder BackwardDistanceHuffmanTree = new HuffmanEncoder();
                Dictionary<byte, string> BackwardDistanceCodeBook = BackwardDistanceHuffmanTree.GetCodeBook(BackwardDistanceCount);
                List<byte> BackwardDistanceHeader = BackwardDistanceHuffmanTree.GetHeader(256);

                
                LZ77ToHuffmanAdapter Deflater = new LZ77ToHuffmanAdapter(LiteralsCodeBook, LiteralsHeader, 
                    MatchLengthsCodeBook, MatchLenghtsHeader, BackwardDistanceCodeBook, BackwardDistanceHeader);
                Deflater.Deflate(FileName);
                
                

                //-------------------------------------------------------------------------------
                
                Inflator Inflator = new Inflator();
                Inflator.Inflate(FileName);

                LZ77Decoder LZ77Decoder = new LZ77Decoder(32 * 1024);
                LZ77Decoder.Decode(FileName);
                

                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                double Secs = (double)elapsedMs / 1000.0;
                Console.WriteLine("Compressed file number {0} in {1} secs", file, Secs);
                TotalTime += Secs;

                /*
                Probability Probability = new Probability(InputStream);
                Dictionary<byte, int> CharactersCount = Probability.GetCharactersCount();

                HuffmanEncoder HuffmanEncoder = new HuffmanEncoder();
                HuffmanEncoder.Encode(CharactersCount, InputStream, FileName);

                Console.WriteLine("Encoded {0} bytes", InputStream.Count);

                HuffmanDecoder HuffmanDecoder = new HuffmanDecoder();
                HuffmanDecoder.Decode(FileName);

                Console.WriteLine("\nDecoded {0} bytes", InputStream.Count);

                Console.Write("File number {0} finished in ", file);
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                double Secs = (double)elapsedMs / 1000.0;
                Console.WriteLine("{0} secs", Secs);

                TotalTime += Secs;

                Console.WriteLine("=====================================================");
                Console.WriteLine("=====================================================");
                */

            }

            Console.WriteLine("Compressed all 3 files in {0} mins", TotalTime/60.0);
        }
    }
}
