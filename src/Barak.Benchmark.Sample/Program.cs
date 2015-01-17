using System;
using System.Collections.Generic;

namespace Barak.Benchmark.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            SqlDatabaseWriter sqlDatabaseWriter = new SqlDatabaseWriter("Server=.;Database=Benchmark;Integrated Security=True;");
            var consuleResults = new ConsolePrinter(false);
            
            var results = new CompositeResults(consuleResults);
            
            // Unremark to write resualt to database
            //var results = new CompositeResults(consuleResults, sqlDatabaseWriter);

            Bench bench = new Bench();
            bench.ThreadCount = 1;
            bench.RepeatCount = 3;
            bench.RepeatWarmup = true;
            bench.RepeatCleanup = true;
            bench.SetResualts(results);

            string[] intToString = new string[1000];
            for (int i = 0; i < 1000; i++)
            {
                intToString[i] = i.ToString();
            }

            var dictionaryTestSession = new BenchSession()
            {
                Identity = "16BBBB70-78EB-4608-B9B5-96AC720916DD",
                Description = "Dictionary",
                Parent = null,
            };

            var tryGetByValue = new BenchSession()
            {
                Identity = "56888FB1-C9AA-4CDE-A26A-F1B6C032D626",
                Description = "TryGetValue By Type",
                Parent = dictionaryTestSession,
            };

            Dictionary<int, int> intDic = new Dictionary<int, int>();
            Dictionary<string, int> stringDic = new Dictionary<string, int>();

            // Int Value Test
            bench.Session = new BenchSession()
            {
                Description = "Int",
                Identity = "E4600A0E-2352-4AEC-BDD0-46B097AF6756",
                Parent = tryGetByValue,
            };

            bench.SetWarmup(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    intDic.Add(i,i);
                }
            });

            bench.SetCleanup(() =>
            {
                intDic.Clear();
            });

            bench.SetTest((thread, index) =>
            {
                int value;
                var dicIndex = index%1000;
                var temp = intToString[dicIndex];
                intDic.TryGetValue(dicIndex, out value);
            },TimeSpan.FromSeconds(2));

            bench.Start();

            // String Value Test
            bench.Session = new BenchSession()
            {
                Description = "String",
                Identity = "8AF9E524-6BF7-4C18-93D2-FD900D1A17AA",
                Parent = tryGetByValue,
            };

            bench.SetWarmup(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    stringDic.Add(intToString[i], i);
                }
            });

            bench.SetCleanup(() =>
            {
                stringDic.Clear();
            });

            bench.SetTest((thread, index) =>
            {
                int value;
                stringDic.TryGetValue(intToString[index % 1000], out value);
            }, TimeSpan.FromSeconds(2));

            bench.Start();

            consuleResults.Flush();

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
