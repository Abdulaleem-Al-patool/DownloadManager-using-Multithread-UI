using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiThreadDownloadManager.DownloadManager;

public class ThreadByteCalculator
{
    public long?[] Calculate(long? byteSize, long totalThreadNumber)
    {

        long? baseValue = byteSize / totalThreadNumber;
        long? remainder = byteSize % totalThreadNumber;
        long?[] parts = new long?[totalThreadNumber];


        long? cumulativeSum = 0;


        for (int i = 0; i < totalThreadNumber; i++)
        {
            cumulativeSum += baseValue;
            if (remainder > 0)
            {
                cumulativeSum += 1;
                remainder -= 1;

            }
            parts[i] = cumulativeSum;

        }
        return parts;

    }
}