using System;

namespace DecompressionTool
{
    class Program
    {
        static double DecompressionTotalTime = 0;

        static void Decompress(string FileName, int FileNumber)
        {
            Console.WriteLine("Started decompressing file number {0}", FileNumber);

            var Watch = System.Diagnostics.Stopwatch.StartNew();

            Inflator Inflator = new Inflator();

            Inflator.Inflate(FileName);

            Watch.Stop();
            double elapsedMs = Watch.ElapsedMilliseconds;

            double Secs = (double)elapsedMs / 1000.0;

            DecompressionTotalTime += Secs;

            Console.WriteLine("Decompressed file number {0} in {1} secs", FileNumber, Secs);
        }

        static void ReportStatistics()
        {
            Console.WriteLine("Total Decompression time = {0} mins", DecompressionTotalTime / 60.0);

            Console.WriteLine("Process finished!");
        }

        static void Main(string[] args)
        {
            int FileCount = 0;

            for (int file = 1; file <= 20; file++)
            {
                string FileName = "DataSet_" + file.ToString();

                Decompress(FileName, file);

                FileCount++;

                Console.WriteLine("\n---------------------------------------------------");
            }

            ReportStatistics();
        }
    }
}
