using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiThreadDownloadManager.DownloadManager;

    public class DownloadHandler
    {

    private static readonly HttpClient sharedClient = new HttpClient();


    public static async Task<long?> GetFileSizeAsync(string fileUrl)
    {
        using var request = new HttpRequestMessage(HttpMethod.Head, fileUrl);
        using var response = await sharedClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        return response.Content.Headers.ContentLength;
    }


    public async Task DownloadFileAsync(string fileUrl, string destinationPath, int threadNumber, long? start, long? end, IProgress<double> progress, int chunkSize = 65536)
    {


        var request = new HttpRequestMessage(HttpMethod.Get, fileUrl);
        request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(start, end);

        try
        {
            HttpResponseMessage response = await sharedClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);


            if (response.StatusCode == System.Net.HttpStatusCode.OK && threadNumber > 0)
            {
                return;
            }

            response.EnsureSuccessStatusCode();

            string tempFilePath = $"{destinationPath}.part{threadNumber}";

            using (Stream contentStream = await response.Content.ReadAsStreamAsync())
            using (FileStream fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None, chunkSize, true))
            {
                var buffer = new byte[chunkSize];
                int bytesRead;
                long totalRead = 0;


                long? expectedLength = response.Content.Headers.ContentLength;

                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    totalRead += bytesRead;

                    if (expectedLength.HasValue)
                    {
                        //ProgressCounter.Progress[threadNumber] = (double)totalRead / expectedLength.Value * 100;
                        double percentage = (double)totalRead / expectedLength.Value * 100;
                        progress?.Report(percentage);
                    }
                }
            }

            response.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Part {threadNumber + 1}: {ex.Message}");
        }
    }



}

