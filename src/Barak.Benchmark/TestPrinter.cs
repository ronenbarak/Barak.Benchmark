using System.Diagnostics;

namespace Barak.Benchmark
{
    public class TestPrinter : ConsolePrinter
    {
        public override void Flush()
        {
            Trace.WriteLine(m_tableBuilder.Output());
        }
    }
}