using System;
using System.Collections.Generic;

namespace Barak.Benchmark
{
    public interface IResult
    {
        void Format(BenchSession session, TestMode testMode, int threadCount,int runTimes,TimeSpan duration, IEnumerable<BenchResult> benchResults);
        void Flush();
    }
}