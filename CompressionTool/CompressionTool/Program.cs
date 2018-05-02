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
        static long TotalFileSize = 0;
        static long TotalEncodedFileSize = 0;
        static bool Encode = true;
        static bool Decode = false;
        static bool OperationMode;

        static void Compress(string FileName, int FileNumber)
        {
            Console.WriteLine("Started compressing file number {0}", FileNumber);

            var Watch = System.Diagnostics.Stopwatch.StartNew();

            Deflator Deflator = new Deflator();

            Deflator.Deflate(FileName);

            Watch.Stop();
            double elapsedMs = Watch.ElapsedMilliseconds;

            double Secs = (double)elapsedMs / 1000.0;

            CompressionTotalTime += Secs;

            int OriginalFileSize = Deflator.GetOriginalFileSize();
            int CompressedFileSize = Deflator.GetCompressedFileSize();

            TotalFileSize += OriginalFileSize;
            TotalEncodedFileSize += CompressedFileSize;

            Console.WriteLine("Size of file number {0} before compression = {1} bytes", FileNumber, OriginalFileSize);
            Console.WriteLine("Size of file number {0} after  compression = {1} bytes", FileNumber, CompressedFileSize);
            Console.WriteLine("Compression ratio of file number {0} = {1}", FileNumber, Deflator.GetCompressionRatio());
            Console.WriteLine("Compressed file number {0} in {1} mins", FileNumber, Secs/60.0);
        }

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
            if (OperationMode == Encode)
                Console.WriteLine("Average compression ratio = {0}", (double)TotalFileSize/(double)TotalEncodedFileSize);

            if (OperationMode == Encode)
                Console.WriteLine("Total Compression time = {0} mins", CompressionTotalTime / 60.0);

            if (OperationMode == Decode)
                Console.WriteLine("Total Decompression time = {0} secs", DecompressionTotalTime);

            Console.WriteLine("Process finished!");
        }

        static void Main(string[] args)
        {
            int FileCount = 0;

            //OperationMode = Encode;
            OperationMode = Decode;

            for (int file = 1; file <= 2; file++)
            {
                string FileName = "DataSet_" + file.ToString();

                if(OperationMode == Encode) Compress(FileName, file);
                
                if(OperationMode == Decode) Decompress(FileName, file);

                FileCount++;

                Console.WriteLine("\n---------------------------------------------------");
            }

            ReportStatistics();
        }
    }
}
