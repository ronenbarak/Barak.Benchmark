using System.Configuration;

namespace Barak.Benchmark.Presentation
{

    public interface ISettings
    {
        void Save();
    }

    public interface IBenchmarkPresentationConfiguration : ISettings
    {
        string ConnectionString { get; set; }
    }

    public class BenchmarkPresentationConfiguration : ApplicationSettingsBase, IBenchmarkPresentationConfiguration
    {
        [UserScopedSettingAttribute]
        [DefaultSettingValue(@"Data Source=localhost;Initial Catalog=Benchmark;Integrated Security=True")]
        public string ConnectionString
        {
            get { return ((string)(this["ConnectionString"])); }
            set { this["ConnectionString"] = value; }
        }
    }
}