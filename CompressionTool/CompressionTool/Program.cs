using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionTool
{
    class Program
    {
        static double CompressionTotalTime = 0;
        static double DecompressionTotalTime = 0;
    
        static void Compress(string FileName, int FileNumber)
        {
            var Watch = System.Diagnostics.Stopwatch.StartNew();

            Deflator Deflator = new Deflator();

            Deflator.Deflate(FileName);

            Watch.Stop();
            double elapsedMs = Watch.ElapsedMilliseconds;

            double Secs = (double)elapsedMs / 1000.0;

            CompressionTotalTime += Secs;

            Console.WriteLine("Compressed file number {0} in {1} mins", FileNumber, Secs/60.0);
        }

        static void Decompress(string FileName, int FileNumber)
        {
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
            Console.WriteLine("Total Compression time = {0} mins", CompressionTotalTime / 60.0);

            Console.WriteLine("Total Decompression time = {0} secs", DecompressionTotalTime);

            Console.WriteLine("Process finished!");
        }

        static void Main(string[] args)
        {
            int FileCount = 0;

            for (int file = 10; file <= 10; file++)
            {
                string FileName = "DataSet_" + file.ToString();

                Compress(FileName, file);
                
                Decompress(FileName, file);

                FileCount++;

                Console.WriteLine("\n---------------------------------------------------");
            }

            ReportStatistics();
        }
    }
}
