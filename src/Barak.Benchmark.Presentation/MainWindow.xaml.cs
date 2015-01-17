using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;

using Barak.Benchmark.Presentation.ViewModels;
using Series = Barak.Benchmark.Presentation.ViewModels.Series;

namespace Barak.Benchmark.Presentation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BenchmarkPresentationConfiguration m_benchmarkPresentationConfiguration;
        private BenchmarkViewModel m_viemModel;

        public MainWindow()
        {
            m_benchmarkPresentationConfiguration = new BenchmarkPresentationConfiguration();
            m_viemModel = new BenchmarkViewModel(m_benchmarkPresentationConfiguration);
            m_viemModel.PropertyChanged += MainWindow_PropertyChanged;
            InitializeComponent();
            DataContext = m_viemModel;
            this.Closed += MainWindow_Closed;
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            m_benchmarkPresentationConfiguration.ConnectionString = m_viemModel.LastSeccesfulRefresh;
            m_benchmarkPresentationConfiguration.Save();
        }

        private void MainWindow_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Series")
            {
                chart.Series.Clear();
                if (m_viemModel.Series.Any())
                {
                    var maxLevels = m_viemModel.Series.Max(p => p.Results.Count);
                    if (maxLevels == 1)
                    {
                        foreach (Series currentSeries in m_viemModel.Series)
                        {
                            currentSeries.Results.First().Column = currentSeries.Title;
                            chart.Series.Add(new ColumnSeries()
                            {
                                Title = currentSeries.Title,
                                DependentValuePath = "Value",
                                IndependentValuePath = "Column",
                                ToolTip = currentSeries.Title,
                                IsSelectionEnabled = true,
                                ItemsSource = currentSeries.Results,
                            });
                        }
                    }
                    else
                    {
                        foreach (Series currentSeries in (sender as BenchmarkViewModel).Series)
                        {
                            chart.Series.Add(new LineSeries()
                            {
                                Title = currentSeries.Title,
                                DependentValuePath = "Value",
                                IndependentValuePath = "Column",
                                ToolTip = currentSeries.Title,
                                IsSelectionEnabled = true,
                                ItemsSource = currentSeries.Results,
                            });
                        }
                    }
                }
            }
        }
    }
}
