using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Barak.Benchmark.Tests
{
    [TestClass]
    public class ActionOnlyTests
    {
        [TestMethod]
        public void SingleRepeatCurrentThread()
        {
            System.Threading.Thread currentThread = System.Threading.Thread.CurrentThread;
            System.Threading.Thread testThread = null;
            Bench bench = new Bench();
            bench.ThreadCount = 0;

            bench.SetTest((thread, index) =>
                              {
                                  testThread = System.Threading.Thread.CurrentThread;
                              });

            bench.Start();

            Assert.IsNotNull(testThread);
            Assert.AreEqual(currentThread, testThread);
        }

        [TestMethod]
        public void SingleRepeatOtherThread()
        {
            int hitcount = 0;
            System.Threading.Thread currentThread = System.Threading.Thread.CurrentThread;
            System.Threading.Thread testThread = null;
            Bench bench = new Bench();
            bench.ThreadCount = 1;

            bench.SetTest((thread, index) =>
                              {
                                  Interlocked.Increment(ref hitcount);
                                  testThread = System.Threading.Thread.CurrentThread;
                              });

            bench.Start();
            Assert.AreEqual(1,hitcount);
            Assert.AreNotEqual(currentThread, testThread);
        }

        [TestMethod]
        public void SingleRepeatMultiThreadsThread()
        {
            int hitcount = 0;

            Bench bench = new Bench();
            bench.ThreadCount = 4;

            bench.SetTest((thread, index) =>
            {
                Interlocked.Increment(ref hitcount);
            });

            bench.Start();
            Assert.AreEqual(4, hitcount);
        }

        [TestMethod]
        public void MultiRepeatMultiThreadsThread()
        {
            int hitcount = 0;

            Bench bench = new Bench();
            bench.RepeatCount = 3;
            bench.ThreadCount = 4;

            bench.SetTest((thread, index) =>
            {
                Interlocked.Increment(ref hitcount);
            });

            bench.Start();
            Assert.AreEqual(12, hitcount);
        }
    }
}
