using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Barak.Benchmark.Tests
{
    [TestClass]
    public class CleanupAndWarmups
    {
        [TestMethod]
        public void WarmupHitOnlyInTheFirstRun()
        {
            int warmup = 0;
            Bench bench = new Bench();
            bench.RepeatCount = 2;
            bench.ThreadCount = 2;

            bench.RepeatWarmup = false;
            bench.DoWarmupInEachThread = false;

            bench.SetTest((thread, index) => { });
            bench.SetWarmup(() =>
                                {
                                    Interlocked.Increment(ref warmup);
                                });

            bench.Start();

            Assert.AreEqual(1, warmup);
        }

        [TestMethod]
        public void WarmupHitOnEveryTest()
        {
            int warmup = 0;
            Bench bench = new Bench();
            bench.RepeatCount = 2;
            bench.ThreadCount = 2;

            bench.RepeatWarmup = true;
            bench.DoWarmupInEachThread = false;

            bench.SetTest((thread, index) => { });
            bench.SetWarmup(() =>
                                {
                                    Interlocked.Increment(ref warmup);
                                });

            bench.Start();

            Assert.AreEqual(2, warmup);
        }

        [TestMethod]
        public void WarmupHitOnEveryThreadForOnlySingleTimeTest()
        {
            int warmup = 0;
            Bench bench = new Bench();
            bench.RepeatCount = 2;
            bench.ThreadCount = 2;

            bench.RepeatWarmup = false;
            bench.DoWarmupInEachThread = true;

            bench.SetTest((thread, index) => { });
            bench.SetWarmup(() =>
                                {
                                    Interlocked.Increment(ref warmup);
                                });

            bench.Start();

            Assert.AreEqual(2, warmup);
        }

        [TestMethod]
        public void WarmupHitOnEveryThreadForEveryThreadTest()
        {
            int warmup = 0;
            Bench bench = new Bench();
            bench.RepeatCount = 2;
            bench.ThreadCount = 2;

            bench.RepeatWarmup = true;
            bench.DoWarmupInEachThread = true;

            bench.SetTest((thread, index) => { });
            bench.SetWarmup(() =>
                                {
                                    Interlocked.Increment(ref warmup);
                                });

            bench.Start();

            Assert.AreEqual(4, warmup);
        }

        [TestMethod]
        public void CleanupHappendsOnlyInTheLastRun()
        {
            int cleanUp = 0;
            Bench bench = new Bench();
            bench.RepeatCount = 2;
            bench.ThreadCount = 0;
            bench.RepeatCleanup = false;
            bench.DoCleanUpInEachThread = false;

            int hitCount = 0;

            bench.RepeatWarmup = false;
            bench.DoWarmupInEachThread = false;

            bench.SetTest((thread, index) => { hitCount++; });
            bench.SetCleanup(() =>
            {
                Assert.AreEqual(2, hitCount);
                cleanUp++;
            });

            bench.Start();

            Assert.AreEqual(1, cleanUp);
        }

        [TestMethod]
        public void CleanUpHitOnlyOnce()
        {
            int cleanup = 0;
            Bench bench = new Bench();
            bench.RepeatCount = 2;
            bench.ThreadCount = 2;

            bench.RepeatCleanup = false;
            bench.DoCleanUpInEachThread = false;

            bench.SetTest((thread, index) => { });
            bench.SetCleanup(() =>
                                 {
                                     Interlocked.Increment(ref cleanup);
                                 });

            bench.Start();

            Assert.AreEqual(1, cleanup);
        }

        [TestMethod]
        public void CleanUpHitOnEveryTest()
        {
            int cleanup = 0;
            Bench bench = new Bench();
            bench.RepeatCount = 2;
            bench.ThreadCount = 2;

            bench.RepeatCleanup = true;
            bench.DoCleanUpInEachThread = false;

            bench.SetTest((thread, index) => { });
            bench.SetCleanup(() =>
                                 {
                                     Interlocked.Increment(ref cleanup);
                                 });

            bench.Start();

            Assert.AreEqual(2, cleanup);
        }

        [TestMethod]
        public void CleanupHitOnEveryThreadForOnlySingleTimeTest()
        {
            int cleanup = 0;
            Bench bench = new Bench();
            bench.RepeatCount = 2;
            bench.ThreadCount = 2;

            bench.RepeatCleanup = false;
            bench.DoCleanUpInEachThread = true;

            bench.SetTest((thread, index) => { });
            bench.SetCleanup(() =>
                                 {
                                     Interlocked.Increment(ref cleanup);
                                 });

            bench.Start();

            Assert.AreEqual(2, cleanup);
        }

        [TestMethod]
        public void CleanupHitOnEveryThreadForEveryThreadTest()
        {
            int cleanup = 0;
            Bench bench = new Bench();
            bench.RepeatCount = 2;
            bench.ThreadCount = 2;

            bench.RepeatCleanup = true;
            bench.DoCleanUpInEachThread = true;

            bench.SetTest((thread, index) => { });
            bench.SetCleanup(() =>
                                 {
                                     Interlocked.Increment(ref cleanup);
                                 });

            bench.Start();

            Assert.AreEqual(4, cleanup);
        }
    }
}