namespace Barak.Benchmark
{
    public sealed class BenchSession
    {
        private string m_Description;
        public BenchSession Parent { get; set; }
        public string Identity { get; set; }

        public string Description
        {
            get { return m_Description ?? Identity; }
            set { m_Description = value; }
        }
    }
}