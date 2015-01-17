using System;
using System.Collections.Generic;

namespace Barak.Benchmark.Presentation.ViewModels
{
    public class Resualt
    {
        public int Id { get; set; }
        public Guid RunSessionId { get; set; }
        public int Result { get; set; }
    }

    public class  RunSessionBenchmarkNodeViewModel :BenchmarkNodeViewModel
    {
        public List<Resualt> Resualts { get; set; }

        public RunSessionBenchmarkNodeViewModel(System.Windows.Input.ICommand deleteCommand, System.Windows.Input.ICommand renameCommand)
            : base(deleteCommand, renameCommand)
        {
            Resualts = new List<Resualt>();
        }
    }
}