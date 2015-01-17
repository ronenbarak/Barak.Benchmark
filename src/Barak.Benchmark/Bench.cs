using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Barak.Benchmark
{
    public enum TestMode
    {
        None,
        ActionRunXTime,
        ActionWithDuration,
        ActionOnly,
    }

    public class BenchResult
    {
        public BenchResult(TimeSpan duration,int counter)
        {
            Duration = duration;
            Counter = counter;
        }

        public TimeSpan Duration { get; private set; }
        public int Counter { get; private set; }
    }

    public sealed class Bench
    {
        public delegate void TestAction(int thread,int index);

        class Container
        {
            public int? StartTime;
        }

        private readonly Container m_container = new Container();
        private TestMode m_testMode;
        private Action m_warmup = EmptyAction;
        private Action m_cleanup = EmptyAction;
        private int m_repeatCount;
        private int m_runTimes;
        private TestAction m_test;
        private TimeSpan m_timeSpan;
        public BenchSession Session { get; set; }

        private List<BenchResult> m_benchResults = new List<BenchResult>();
        private IResult m_result;

        /// <summary>
        /// How many times to repeat the test
        /// </summary>
        public int RepeatCount
        {
            get
            {
                if (m_repeatCount < 1)
                {
                    return 1;
                }
                return m_repeatCount;
            }
            set { m_repeatCount = value; }
        }

        public bool RepeatWarmup { get; set; }
        public bool DoWarmupInEachThread { get; set; }
        public bool RepeatCleanup { get; set; }
        public bool DoCleanUpInEachThread { get; set; }
        
        /// <summary>
        /// Start the the test with x Threads
        /// Set 0 to run with the current thread
        /// set 1 to run on a single thread  
        /// </summary>
        public int ThreadCount { get; set; }

        /// <summary>
        /// Start before test starts
        /// </summary>
        public void SetWarmup(Action warmup)
        {
            m_warmup = warmup;
        }
        /// <summary>
        /// Start test for the x times
        /// Rusualt will be how long did it take it to run X times
        /// </summary>
        public void SetTest(TestAction test, int runTimes)
        {
            m_test = test;
            m_runTimes = runTimes;
            m_testMode = TestMode.ActionRunXTime;
        }

        /// <summary>
        /// Start test for the timespan duration
        /// Rusualt will be how many times we hit test for the timespan
        /// </summary>
        public void SetTest(TestAction test,TimeSpan timeSpan)
        {
            m_testMode = TestMode.ActionWithDuration;
            m_test = test;
            m_timeSpan = timeSpan;
        }

        /// <summary>
        /// Start the test for a single time
        /// Resualt will be the duration of the test
        /// </summary>
        public void SetTest(TestAction test)
        {
            m_test = test;
            m_testMode = TestMode.ActionOnly;
        }

        /// <summary>
        /// run after the test finish
        /// Will evaluate time.
        /// </summary>
        public void SetCleanup(Action cleanup)
        {
            m_cleanup = cleanup;
        }

        public void Start()
        {
            switch (m_testMode)
            {
                case TestMode.ActionOnly:
                    {
                        ActionTest(threadIndex =>
                        {
                            m_test.Invoke(threadIndex,0);
                                             return 1;
                        }, RepeatCount, ThreadCount);
                        break;
                    }
                case TestMode.ActionRunXTime:
                    {
                        ActionTestRuntimes(m_test, RepeatCount, ThreadCount, m_runTimes);
                        break;
                    }
                case TestMode.ActionWithDuration:
                    {
                        ActionWithDuration(m_test, RepeatCount, ThreadCount, m_timeSpan);
                        break;
                    }
            }

            if (m_result != null)
            {
                m_result.Format(Session, m_testMode, ThreadCount, m_runTimes, m_timeSpan, m_benchResults);
            }
        }

        private static void EmptyAction()
        {
            
        }

        private void ActionWithDuration(TestAction action, int repeatCount, int threadCount, TimeSpan runDurations)
        {
            ActionTest(threadIndex =>
                           {
                               var runUntil = m_container.StartTime + runDurations.TotalMilliseconds;                            
                               int counter = 0;
                               do
                               {
                                   action.Invoke(threadIndex,counter);
                                   counter++;
                               } while (runUntil > Environment.TickCount); 
                               return counter;
                           }, repeatCount, threadCount);   
        }

        private void ActionTestRuntimes(TestAction action, int repeatCount, int threadCount,int runtimes)
        {
            ActionTest(threadIndex => 
            {
                for(int i = 0;i<runtimes;i++)
                {
                    action.Invoke(threadIndex,i);
                }
                return runtimes;
            },repeatCount, threadCount);   
        }

        private void ActionTest(Func<int,int> action,int repeatCount,int threadCount)
        {
            m_benchResults.Clear();
            for (int repeatIndex = 0; repeatIndex < repeatCount; repeatIndex++)
            {
                m_container.StartTime = null;
                if (!this.DoWarmupInEachThread)
                {
                    if (this.RepeatWarmup)
                    {
                        m_warmup.Invoke();   
                    }
                    else if (repeatIndex == 0)
                    {
                        m_warmup.Invoke();   
                    }
                }
                if (threadCount == 0)
                {
                    var stopwatch = Stopwatch.StartNew();
                    m_container.StartTime = Environment.TickCount;
                    int count = action.Invoke(0);
                    stopwatch.Stop();

                    m_benchResults.Add(new BenchResult(stopwatch.Elapsed, count));
                }
                else
                {
                    List<Thread> threads = new List<Thread>();
                    List<int> summary = new List<int>();
                    CountdownEvent countdownEvent = new CountdownEvent(threadCount);
                    
                    CountdownEvent endedEvent = new CountdownEvent(threadCount);
                    for (int j = 0; j < threadCount; j++)
                    {
                        int threadIndex = j;
                        var thread = new Thread(() =>
                                                    {
                                                        if (this.DoWarmupInEachThread)
                                                        {
                                                            if (this.RepeatWarmup)
                                                            {
                                                                m_warmup.Invoke();
                                                            }
                                                            else if (repeatIndex == 0)
                                                            {
                                                                m_warmup.Invoke();
                                                            }
                                                        }
                                                        SpinWait spin = new SpinWait();
                                                        countdownEvent.Signal();
                                                        while (m_container.StartTime == null)
                                                        {
                                                            spin.SpinOnce();
                                                        }

                                                        int count = action.Invoke(threadIndex);

                                                        // Tell the clock we are finished
                                                        endedEvent.Signal();
                                                        endedEvent.Wait();

                                                        if (this.DoCleanUpInEachThread)
                                                        {
                                                            if (this.RepeatCleanup)
                                                            {
                                                                m_cleanup.Invoke();
                                                            }
                                                            else if (repeatIndex == (repeatCount -1))
                                                            {
                                                                m_cleanup.Invoke();
                                                            }
                                                        }
                                                        lock (summary)
                                                        {
                                                            summary.Add(count);
                                                        }
                                                    });
                        thread.Start();
                        threads.Add(thread);
                    }
                    countdownEvent.Wait();
                    var stopwatch = Stopwatch.StartNew();
                    m_container.StartTime = Environment.TickCount;
                    endedEvent.Wait();
                    stopwatch.Stop();
                    threads.ForEach(p=>p.Join());
                    m_benchResults.Add(new BenchResult(stopwatch.Elapsed, summary.Sum()));
                }

                if (!this.DoCleanUpInEachThread)
                {
                    if (this.RepeatCleanup)
                    {
                        m_cleanup.Invoke();
                    }
                    else if (repeatIndex == (repeatCount - 1))
                    {
                        m_cleanup.Invoke();
                    }
                }
            }
        }

        public void SetResualts(IResult result)
        {
            m_result = result;
        }
    }
}
