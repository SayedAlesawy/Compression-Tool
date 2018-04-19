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
            var watch = System.Diagnostics.Stopwatch.StartNew();
            // the code that you want to measure comes here

            string FileName = "DataSet_1";

            //string FileName = "test";

            InputReader InputReader = new InputReader(FileName);
            //OutputWriter OutputWriter = new OutputWriter(FileName);

            string Text = InputReader.GetFileContent();
            //OutputWriter.WriteToFile(Text);

            Probability Probability = new Probability(Text);
            Dictionary<char, int> CharactersCount = Probability.GetCharactersCount();

            HuffmanEncoder HuffmanEncoder = new HuffmanEncoder();
            HuffmanEncoder.Encode(CharactersCount, Text, FileName);

            Console.WriteLine("Encoded {0} symbol", Text.Length);

            HuffmanDecoder HuffmanDecoder = new HuffmanDecoder();
            HuffmanDecoder.Decode(FileName);

            Console.WriteLine("Decoded {0} symbol", Text.Length);

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            double lol = (double)elapsedMs / 1000.0;
            lol /= 60;
            Console.WriteLine("{0} mins", lol);
        }
    }
}
