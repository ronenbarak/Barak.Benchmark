using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Barak.Benchmark
{
    public class SqlDatabaseWriter : IResult
    {
        public static readonly string DefualConnectionString = "Server=.;Database=Benchmark;Integrated Security=True;";

        class ClonedBenchSession
        {
            public BenchSession BenchSession { get; set; }
            public TestMode TestMode { get; set; }
            public int ThreadCount { get; set; }
            public int RunTimes { get; set; }
            public IEnumerable<BenchResult> BenchResults { get; set; }
            public TimeSpan Duration { get; set; }
            public DateTime Time { get; set; }
        }

        private string m_connectionString;
        private List<ClonedBenchSession> m_itemsCollected = new List<ClonedBenchSession>();

        public SqlDatabaseWriter(string connectionString)
        {
            m_connectionString = connectionString;
        }

        public void Format(BenchSession session, TestMode testMode, int threadCount, int runTimes,TimeSpan duration, IEnumerable<BenchResult> benchResults)
        {
            m_itemsCollected.Add(new ClonedBenchSession()
                                     {
                                         BenchResults = benchResults.ToList(),
                                         BenchSession = Clone(session),
                                         ThreadCount = threadCount,
                                         RunTimes = runTimes,
                                         TestMode = testMode,
                                         Duration = duration,
                                         Time =  DateTime.Now,
                                     });
        }

        private BenchSession Clone(BenchSession session)
        {
            if (session == null)
            {
                return null;
            }
            else
            {
                return new BenchSession()
                           {
                               Description = session.Description,
                               Identity = session.Identity,
                               Parent = Clone(session.Parent),
                           };
            }
        }

        public void Flush()
        {
            using (var connection = new SqlConnection(m_connectionString))
            {
                connection.Open();

                foreach (ClonedBenchSession currentBenchmarkItem in m_itemsCollected)
                {
                    var command = connection.CreateCommand();
                    command.CommandText = string.Format("select Id,ParentId from BenchSession");
                    List<Tuple<string, string>> hiercy = new List<Tuple<string, string>>();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            hiercy.Add(new Tuple<string, string>(reader[0].ToString(),(reader[1] is DBNull) ? null:reader[1].ToString()));
                        }
                    }
                    var hiercyNodes = hiercy.ToLookup(p => p.Item1,p=>p.Item2);

                    List<BenchSession> bencHiercy = BuildHiercy(currentBenchmarkItem.BenchSession);

                    List<BenchSession> itemsToCreate = new List<BenchSession>();
                    foreach (var benchSession in bencHiercy)
                    {
                        if (itemsToCreate.Count != 0)
                        {
                            if (hiercyNodes.Contains(benchSession.Identity))
                            {
                                throw new Exception("invalid node hierarchy");
                            }
                            itemsToCreate.Add(benchSession);
                        }
                        else
                        {
                            if (hiercyNodes.Contains(benchSession.Identity))
                            {
                                if (benchSession.Parent != null)
                                {
                                    if (!hiercyNodes[benchSession.Identity].Contains(benchSession.Parent.Identity))
                                    {
                                        throw new Exception("invalid node hierarchy");
                                    }
                                }
                            }
                            else
                            {
                                itemsToCreate.Add(benchSession);
                            }
                        }
                    }

                    using (var transaction = connection.BeginTransaction())
                    {
                        foreach (var itemToCreate in itemsToCreate)
                        {
                            var insertBenchCommand = connection.CreateCommand();
                            insertBenchCommand.Transaction = transaction;
                            insertBenchCommand.CommandText = string.Format(@"INSERT INTO BenchSession 
                                                                            ([Id],[ParentId],[Description])
                                                                             VALUES
                                                                           ('{0}',{1},'{2}')",
                                                                             itemToCreate.Identity,
                                                                             itemToCreate.Parent == null ? "null":string.Format("'{0}'",itemToCreate.Parent.Identity),
                                                                             itemToCreate.Description);

                            insertBenchCommand.ExecuteNonQuery();
                        }

                        var checkIfLastValid = connection.CreateCommand();
                        checkIfLastValid.Transaction = transaction;
                        checkIfLastValid.CommandText =  string.Format(@"SELECT [ThreadCount], [TestMode], [Runtimes] ,[Duration] 
                                                                        FROM [BenchSession] Where Id ='{0}'",currentBenchmarkItem.BenchSession.Identity);


                        using (var reader = checkIfLastValid.ExecuteReader())
                        {
                            reader.Read();
                            if (reader[0] is DBNull)
                            {
                                reader.Close();
                                // Update the current node
                                using (var udpateLastItem = connection.CreateCommand())
                                {
                                    udpateLastItem.Transaction = transaction;
                                    udpateLastItem.CommandText = string.Format(@"UPDATE [dbo].[BenchSession]
                                                                             SET [ThreadCount] = {0}, [TestMode] = {1}, [Runtimes] = {2}, [Duration] = {3}
                                                                             Where Id ='{4}'"
                                                                               , currentBenchmarkItem.ThreadCount,
                                                                               (int) currentBenchmarkItem.TestMode,
                                                                               currentBenchmarkItem.RunTimes,
                                                                               currentBenchmarkItem.Duration.
                                                                                   TotalMilliseconds,
                                                                               currentBenchmarkItem.BenchSession.
                                                                                   Identity);

                                    udpateLastItem.ExecuteNonQuery();
                                }

                            }
                            else
                            {
                                int threadCount = (int) reader[0];
                                TestMode testMode = (TestMode) ((int) reader[1]);
                                int runTimes = (int) reader[2];
                                int duration = (int) reader[3];

                                if (threadCount != currentBenchmarkItem.ThreadCount ||
                                    runTimes != currentBenchmarkItem.RunTimes ||
                                    testMode != currentBenchmarkItem.TestMode ||
                                    duration != currentBenchmarkItem.Duration.TotalMilliseconds)
                                {
                                    throw new Exception(
                                        "The current bench has diffrent settings than previus run, change the identity");
                                }
                                reader.Close();
                            }

                            Guid runId = Guid.NewGuid();
                            // Finaly Now we can start to inset the data:
                            var insertRunSession = connection.CreateCommand();
                            insertRunSession.Transaction = transaction;
                            insertRunSession.CommandText = string.Format(
                                @"INSERT INTO [dbo].[RunSession]
                                                                           ([Id] ,[SessionId] ,[InstanceTime])
                                                                           VALUES
                                                                           ('{0}','{1}','{2}')",
                                runId, currentBenchmarkItem.BenchSession.Identity,
                                currentBenchmarkItem.Time.ToString("yyyy-MM-dd HH:mm:ss"));

                            insertRunSession.ExecuteNonQuery();

                            using (var resualtsCommand = connection.CreateCommand())
                            {
                                resualtsCommand.Transaction = transaction;
                                foreach (var resualt in currentBenchmarkItem.BenchResults)
                                {
                                    var testMode = currentBenchmarkItem.TestMode;
                                    int result = 0;
                                    if (testMode == TestMode.ActionOnly ||
                                        testMode == TestMode.ActionRunXTime)
                                    {
                                        result = (int) resualt.Duration.TotalMilliseconds;
                                    }
                                    else if (testMode == TestMode.ActionWithDuration)
                                    {
                                        result = resualt.Counter;
                                    }

                                    resualtsCommand.CommandText = string.Format(@"INSERT INTO [dbo].[RunSessionResults]
           ([RunSessionId] ,[Result])
            VALUES
           ('{0}',{1})", runId, result);
                                    resualtsCommand.ExecuteNonQuery();
                                }
                            }
                        }
                        // Check if the last item is of the currect format
                        transaction.Commit();
                    }
                }
            }
        }

        private List<BenchSession> BuildHiercy(BenchSession currentBenchmarkItem)
        {
            List<BenchSession> bencHiercy = new List<BenchSession>();
            BenchSession currentBench = currentBenchmarkItem;
            while (currentBench != null)
            {
                bencHiercy.Insert(0, currentBench);
                currentBench = currentBench.Parent;
            }
            return bencHiercy;
        }
    }
}