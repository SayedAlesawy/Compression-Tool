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
            string FileName = "DataSet_1";


            InputReader InputReader = new InputReader(FileName);
            OutputWriter OutputWriter = new OutputWriter(FileName);

            string Text = InputReader.GetFileContent();
            OutputWriter.WriteToFile(Text);

            Probability Probability = new Probability(Text);
            Dictionary<char, double> CharacterPropability = Probability.GetCharactersProbability();

            Console.WriteLine("{0}", Text.Length);
        }
    }
}
