using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Barak.Benchmark
{
    public class ConsolePrinter : IResult
    {
        //List<Tuple<string, TestMode, int, IEnumerable<BenchResult>>> m_benchResults = new List<Tuple<string, TestMode, int, IEnumerable<BenchResult>>>();
        protected TableBuilder m_tableBuilder = new TableBuilder();
        private bool m_autoFlush;

        public ConsolePrinter(bool autoFlush = true)
        {
            m_autoFlush = autoFlush;
            m_tableBuilder.AddRow(new[] { "Description", "ThreadCount","xTimes", "Result" });
            m_tableBuilder.AddRow(new[] { "-----------", "-----------", "------", "------" });    
        }

        public void Format(BenchSession session, TestMode testMode, int threadCount,int runtimes,TimeSpan duration, IEnumerable<BenchResult> benchResults)
        {
            string description = GetSessionFullDescription(session);
            
            foreach (var results in benchResults)
            {
                m_tableBuilder.AddRow(new[] {description, threadCount.ToString(),testMode == TestMode.ActionRunXTime?  runtimes.ToString():string.Empty, GetBenchResult(results, testMode)});
            }
            m_tableBuilder.Separator = " | ";
            
            if (m_autoFlush)
            {
                Flush();
            }
        }

        private string GetBenchResult(BenchResult results,TestMode testMode)
        {
            if (testMode == TestMode.ActionOnly ||
                testMode == TestMode.ActionRunXTime)
            {
                return results.Duration.TotalMilliseconds.ToString("#,##0") + " ms";
            }
            else if (testMode == TestMode.ActionWithDuration)
            {
                return results.Counter.ToString("#,##0");
            }

            return string.Empty;
        }

        private string GetSessionFullDescription(BenchSession session)
        {
            if (session == null)
            {
                return string.Empty;
            }
            else
            {
                var prev = GetSessionFullDescription(session.Parent);
                if (string.IsNullOrWhiteSpace( prev) )
                {
                    return session.Description ?? string.Empty;
                }
                else
                {
                    return prev + "\\" + session.Description ?? string.Empty;   
                }
            }
        }

        public virtual void Flush()
        {
            Console.WriteLine(m_tableBuilder.Output());
        }
    }
}
