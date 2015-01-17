using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Barak.Benchmark.Tests
{
    [TestClass]
    public class DurationTests
    {
        [TestMethod]
        public void RunDurationIsWorking()
        {
            Bench bench = new Bench();
            bench.ThreadCount = 0;
            int hitCount = 0;
            bench.SetTest((thread, index) => 
            {
                hitCount++;
            }, TimeSpan.FromMilliseconds(100));

            bench.SetResualts(new TestPrinter());
            bench.Start();

            Assert.IsTrue(hitCount > 1000); // We assoume we can make at least 1000 hit in 100 ms event on a slow computer
        }

        [TestMethod]
        public void RunDurationForMultiThreadasWellIsWorking()
        {

            Bench bench = new Bench();
            bench.ThreadCount = 0;
            int hitCount = 0;
            bench.ThreadCount = 2;
            bench.SetTest((thread, index) =>
            {
                Interlocked.Increment(ref hitCount);
            }, TimeSpan.FromMilliseconds(100));

            bench.Start();

            Assert.IsTrue(hitCount > 1000); // We assoume we can make at least 1000 hit in 100 ms event on a slow computer
        }
    }

    [TestClass]
    public class ActionRunTimeTests
    {
        [TestMethod]
        public void RunMultiTimeIsWorking()
        {
            Bench bench = new Bench();
            bench.ThreadCount = 0;
            int hitCount = 0;
            bench.SetTest((thread, index) =>
                              {
                                  hitCount++;
                              },3);

            bench.Start();

            Assert.AreEqual(3,hitCount);
        }

        [TestMethod]
        public void RunMultiTimeWithRepeateIsWorking()
        {
            Bench bench = new Bench();
            bench.ThreadCount = 0;
            bench.RepeatCount = 2;
            bench.ThreadCount = 2;
            int hitCount = 0;
            bench.SetTest((thread, index) =>
                              {
                                  Interlocked.Increment(ref hitCount);
                              }, 3);

            bench.Start();

            Assert.AreEqual(12, hitCount);
        }
    }
}