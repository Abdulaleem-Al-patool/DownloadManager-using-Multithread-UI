
using MultiThreadDownloadManager.DownloadManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultithreadDownloadManager.DownloadManager;



public class DownloadManagerService
{

    public async Task StartDownloadAsync(string link, string destination, IProgress<double> progress, IProgress<string> statusMessage)
    {
        statusMessage?.Report("Fetching file info...");
        long? totalBytes = await DownloadHandler.GetFileSizeAsync(link);

        if (totalBytes == null || totalBytes == 0)
        {
            throw new Exception("Could not get file size. Please ensure this is a direct file link.");
        }

        int threadNumber = Environment.ProcessorCount;
        double[] progressArray = new double[threadNumber];
        Task[] downloadParts = new Task[threadNumber];

        var separatedThreadBytes = new ThreadByteCalculator().Calculate(totalBytes, threadNumber);

        statusMessage?.Report($"Downloading with {threadNumber} threads...");

        for (int i = 0; i < threadNumber; i++)
        {
            long? start = (i == 0) ? 0 : separatedThreadBytes[i - 1] + 1;
            long? end = separatedThreadBytes[i];
            int currentIndex = i;


            var internalProgress = new Progress<double>(p =>
            {
                progressArray[currentIndex] = p;
                double totalProgress = progressArray.Sum() / threadNumber;
                progress?.Report(totalProgress);
            });

            downloadParts[i] = new DownloadHandler().DownloadFileAsync(link, destination, i, start, end, internalProgress, 131072);
        }

        await Task.WhenAll(downloadParts);

        statusMessage?.Report("Merging files... Please wait.");
        await MergeFilesAsync(destination, threadNumber);

        statusMessage?.Report("Download completed successfully!");
    }

    private async Task MergeFilesAsync(string destination, int threadNumber)
    {
        using (FileStream finalFile = new FileStream(destination, FileMode.Create, FileAccess.Write))
        {
            for (int i = 0; i < threadNumber; i++)
            {
                string tempFile = $"{destination}.part{i}";
                if (File.Exists(tempFile))
                {
                    using (FileStream partStream = new FileStream(tempFile, FileMode.Open, FileAccess.Read))
                    {
                        await partStream.CopyToAsync(finalFile);
                    }
                    File.Delete(tempFile);
                }
            }
        }
    }



}