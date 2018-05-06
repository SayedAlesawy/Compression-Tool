using System;

namespace CompressionTool
{
    class Program
    {
        static double CompressionTotalTime = 0;
        static long TotalFileSize = 0;
        static long TotalEncodedFileSize = 0;
    
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

            Console.WriteLine("Size of file number {0} before compression = {1} bytes ({2} bits)", FileNumber, OriginalFileSize, 8 * OriginalFileSize);
            Console.WriteLine("Size of file number {0} after  compression = {1} bytes ({2} bits)", FileNumber, CompressedFileSize, 8 * CompressedFileSize);
            Console.WriteLine("Compression ratio of file number {0} = {1}", FileNumber, Deflator.GetCompressionRatio());
            Console.WriteLine("Compressed file number {0} in {1} secs", FileNumber, Secs);
        }

        static void ReportStatistics()
        {
            Console.WriteLine("Average compression ratio = {0}", (double)TotalFileSize/(double)TotalEncodedFileSize);

            Console.WriteLine("Total Compression time = {0} mins", CompressionTotalTime/60.0);

            Console.WriteLine("Process finished!");
        }

        static void Main(string[] args)
        {
            int FileCount = 0;

            for (int file = 1; file <= 20; file++)
            {
                string FileName = "DataSet_" + file.ToString();

                Compress(FileName, file);
                
                FileCount++;

                Console.WriteLine("\n---------------------------------------------------");
            }

            ReportStatistics();
        }
    }
}
