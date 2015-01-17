using System;
using System.Collections.Generic;

namespace Barak.Benchmark
{
    public class CompositeResults : IResult
    {
        private IResult[] m_results;

        public CompositeResults(params IResult[] results)
        {
            m_results = results;
        }

        public void Format(BenchSession session, TestMode testMode, int threadCount,int runTimes,TimeSpan duration, IEnumerable<BenchResult> benchResults)
        {
            foreach (var results in m_results)
            {
                results.Format(session, testMode, threadCount,runTimes,duration, benchResults);
            }
        }


        public void Flush()
        {
            foreach (var results in m_results)
            {
                results.Flush();
            }
        }
    }
}