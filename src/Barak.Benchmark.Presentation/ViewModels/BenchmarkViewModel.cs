using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;


namespace Barak.Benchmark.Presentation.ViewModels
{
    public class Result
    {
        public string Column { get; set; }
        public double Value { get; set; }
    }
    public class Series
    {
        public string Title { get; set; }
        public List<Result> Results { get; set; }
    }

    public class BenchmarkViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public class BenchSession
        {
            public string Id { get; set; }
            public string ParentId { get; set; }
            public string Description { get; set; }
            public int ThreadCount { get; set; }
            public TestMode TestMode { get; set; }
            public int Runtimes { get; set; }
            public int Duration { get; set; }
        }

        public class RunSession
        {
            public Guid Id { get; set; }
            public string SessionId { get; set; }
            public DateTime InstanceTime { get; set; }
        }

        private IEnumerable<Series> m_series;
        private string m_connectionString;
        private IBenchmarkPresentationConfiguration m_benchmarkPresentationConfiguration;
        private string m_lastSeccesfulRefresh;
        private RelayCommand m_deleteCommand;
        private RelayCommand m_renameCommand;

        public string LastSeccesfulRefresh {get { return m_lastSeccesfulRefresh; }}

        public IEnumerable<Series> Series
        {
            get { return m_series; }
            set
            {
                if (m_series != value)
                {
                    m_series = value;
                    OnPropertyChanged("Series");
                }
            }
        }

        public string ConnectionString
        {
            get { return m_connectionString; }
            set
            {
                if (m_connectionString != value)
                {
                    m_connectionString = value;
                    OnPropertyChanged("ConnectionString");
                    OnConnectionStringchanged();
                }
            }
        }

        private void OnConnectionStringchanged()
        {            
            RefreshCommand.RaiseCanExecuteChanged();
        }

        public Barak.Benchmark.Presentation.ViewModels.RelayCommand RefreshCommand { get; private set; }
        public System.Windows.Input.ICommand GenerateCommand { get; private set; }
        public ObservableCollection<BenchmarkNodeViewModel> HeadNodesViewModel { get; protected set; }

        public BenchmarkViewModel(IBenchmarkPresentationConfiguration benchmarkPresentationConfiguration)
        {
            HeadNodesViewModel = new ObservableCollection<BenchmarkNodeViewModel>();
            m_benchmarkPresentationConfiguration = benchmarkPresentationConfiguration;

            RefreshCommand = new Barak.Benchmark.Presentation.ViewModels.RelayCommand(o => OnRefresh(),
                                                                                      o => !string.IsNullOrWhiteSpace(ConnectionString));

            ConnectionString = m_benchmarkPresentationConfiguration.ConnectionString;

            m_lastSeccesfulRefresh = ConnectionString;

            GenerateCommand = new Barak.Benchmark.Presentation.ViewModels.RelayCommand(o =>  OnGenerateCommand());

            m_deleteCommand = new RelayCommand(o =>
                                                   {
                                                       OnDeleteCommand(o as BenchmarkNodeViewModel);
                                                   });
            m_renameCommand = new RelayCommand(o =>
                                                   {
                                                       OnRenameCommand(o as BenchmarkNodeViewModel);
                                                   }, o => !(o is RunSessionBenchmarkNodeViewModel));
            RefreshCommand.Execute(null);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnRenameCommand(BenchmarkNodeViewModel benchmarkNodeViewModel)
        {
            if (!(benchmarkNodeViewModel is RunSessionBenchmarkNodeViewModel))
            {
                string resualt = Microsoft.VisualBasic.Interaction.InputBox(string.Empty, "Change Name", benchmarkNodeViewModel.Description);
                if (!string.IsNullOrWhiteSpace(resualt) )
                {
                    try
                    {
                        using (SqlConnection connection = new SqlConnection(ConnectionString))
                        {
                            connection.Open();

                            using (var command = connection.CreateCommand())
                            {
                                command.CommandText =
                                    string.Format("Update BenchSession Set Description ='{0}' where Id ='{1}'", resualt, benchmarkNodeViewModel.Id);

                                command.ExecuteNonQuery();
                            }
                        }
                        benchmarkNodeViewModel.Description = resualt;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private void OnDeleteCommand(BenchmarkNodeViewModel benchmarkNodeViewModel)
        {
            try
            {
                List<Guid> runSession = new List<Guid>();
                List<string> benchSession = new List<string>();
                FillRemoveGuids(benchmarkNodeViewModel, benchSession, runSession);

                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    using (var tran = connection.BeginTransaction())
                    {
                        if (runSession.Any())
                        {
                            using (var command = connection.CreateCommand())
                            {
                                command.Transaction = tran;
                                command.CommandText = string.Format("Delete RunSessionResults where [RunSessionId] in ({0})",
                                                           string.Join(",", runSession.Select(p => string.Format("'{0}'",p))));

                                command.ExecuteNonQuery();
                                command.CommandText = string.Format("Delete RunSession where Id in ({0})",
                                                               string.Join(",", runSession.Select(p => string.Format("'{0}'", p))));

                                command.ExecuteNonQuery();
                            }

                        }

                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "Delete BenchSession where Id = @p0";
                            command.Transaction = tran;
                            command.Parameters.Add("p0",SqlDbType.NVarChar);
                            foreach (var benchSessionId in benchSession)
                            {
                                command.Parameters[0].Value = benchSessionId;
                                command.ExecuteNonQuery();
                            }
                        }

                        tran.Commit();
                    }
                }

                OnRefresh();
            }
            catch (Exception)
            {
                
            }
        }

        private void FillRemoveGuids(BenchmarkNodeViewModel benchmarkNodeViewModel, List<string> benchSession, List<Guid> runSession)
        {
            if (benchmarkNodeViewModel is RunSessionBenchmarkNodeViewModel)
            {
                runSession.Add(new Guid((benchmarkNodeViewModel as RunSessionBenchmarkNodeViewModel).Id));
            }
            else
            {
                foreach (var child in benchmarkNodeViewModel.Children)
                {
                    FillRemoveGuids(child,benchSession, runSession);
                }
                benchSession.Add(benchmarkNodeViewModel.Id);
            }
        }

        private void OnGenerateCommand()
        {
            var selectedItems = GetAllSelected(HeadNodesViewModel);
            List<Series> series = new List<Series>();
            foreach (IGrouping<BenchmarkNodeViewModel, RunSessionBenchmarkNodeViewModel> currentSelected in selectedItems.GroupBy(p=>p.Parent))
            {
                int index = 0;
                series.Add(new Series()
                           {
                               Title = currentSelected.Key != null ? currentSelected.Key.Description: currentSelected.First().Description,
                               Results = currentSelected.Select(p=> new Result()
                                                                                 {
                                                                                     Column = (++index).ToString(),
                                                                                     Value = p.Resualts.Average(x=>x.Result),
                                                                                 }).ToList(),
                           });    
            }
            

            Series = series;
        }

        private List<RunSessionBenchmarkNodeViewModel> GetAllSelected(IEnumerable<BenchmarkNodeViewModel> viewModels )
        {
            List<RunSessionBenchmarkNodeViewModel> selected = new List<RunSessionBenchmarkNodeViewModel>();
            foreach (BenchmarkNodeViewModel benchmarkNodeViewModel in viewModels)
            {
                if (benchmarkNodeViewModel.Selected == true && benchmarkNodeViewModel is RunSessionBenchmarkNodeViewModel)
                {
                    selected.Add(benchmarkNodeViewModel as RunSessionBenchmarkNodeViewModel);
                }

                if (benchmarkNodeViewModel.Selected != false)
                {
                    selected.AddRange(GetAllSelected(benchmarkNodeViewModel.Children));
                }
            }

            return selected;
        }

        private void OnRefresh()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    IEnumerable<BenchSession> benchSessions = connection.Query<BenchSession>("SELECT * from BenchSession");
                    var benchSessionById = benchSessions.ToDictionary(x => x.Id);
                    Dictionary<string, BenchmarkNodeViewModel> nodes = new Dictionary<string, BenchmarkNodeViewModel>();
                    foreach (var benchSession in benchSessions)
                    {
                        var childNode = CreateNode(nodes, benchSession);

                        if (benchSession.ParentId != null)
                        {
                            var parentNode = CreateNode(nodes, benchSessionById[benchSession.ParentId]);
                            childNode.Parent = parentNode;
                            parentNode.Children.Add(childNode);
                        }
                    }

                    var runSessions = connection.Query<RunSession>("SELECT * from RunSession order by InstanceTime");
                    var runSessionsResults = connection.Query<Resualt>("SELECT * from RunSessionResults").ToLookup(p=>p.RunSessionId);

                    foreach (var session in runSessions)
                    {
                        var currentNode = nodes[session.SessionId];
                        var runSession = new RunSessionBenchmarkNodeViewModel(m_deleteCommand, m_renameCommand)
                                             {
                                                 Id = session.Id.ToString(),
                                                 Description = session.InstanceTime.ToString(),
                                                 Parent = currentNode,
                                             };
                        if (runSessionsResults.Contains(session.Id))
                        {
                            foreach (var runSessionsResult in runSessionsResults[session.Id])
                            {
                                runSession.Resualts.Add(runSessionsResult);
                            }
                            currentNode.Children.Add(runSession);   
                        }
                    }

                    RefreshNodes(HeadNodesViewModel, nodes.Values.Where(p => p.Parent == null));
                }
                
                m_lastSeccesfulRefresh = ConnectionString;
            }
            catch (Exception)
            {
                // This should do some thing
            }
        }

        private void RefreshNodes(ObservableCollection<BenchmarkNodeViewModel> headNodesViewModel, IEnumerable<BenchmarkNodeViewModel> currentItems)
        {
            var equalityComparer = new EqualityPredicate<BenchmarkNodeViewModel>((model, viewModel) => model.Id == viewModel.Id,model => model.Id.GetHashCode());
            var deletedItems = headNodesViewModel.Except(currentItems, equalityComparer).ToList();
            var addedItems = currentItems.Except(headNodesViewModel, equalityComparer).ToList();

            var updatedItems = currentItems.Except(addedItems).ToDictionary(p => p.Id);
            foreach (var deletedItem in deletedItems)
            {
                headNodesViewModel.Remove(deletedItem);
            }

            foreach (BenchmarkNodeViewModel updateItem in headNodesViewModel)
            {
                updateItem.Description = updateItem.Description;
                if (updateItem is RunSessionBenchmarkNodeViewModel)
                {
                    (updateItem as RunSessionBenchmarkNodeViewModel).Resualts = (updatedItems[updateItem.Id] as RunSessionBenchmarkNodeViewModel).Resualts;
                }
                RefreshNodes(updateItem.Children, updatedItems[updateItem.Id].Children);
            }

            foreach (var addedItem in addedItems)
            {
                headNodesViewModel.Add(addedItem);
            }
        }

        private BenchmarkNodeViewModel CreateNode(Dictionary<string, BenchmarkNodeViewModel> nodes, BenchSession benchSession)
        {
            BenchmarkNodeViewModel nodeViewModel;
            if (!nodes.TryGetValue(benchSession.Id, out nodeViewModel))
            {
                nodeViewModel = new BenchmarkNodeViewModel(m_deleteCommand,m_renameCommand)
                                    {
                                        Id = benchSession.Id,
                                        Description = benchSession.Description,
                                    };

                nodes.Add(benchSession.Id, nodeViewModel);
            }

            return nodeViewModel;
        }
    }
}
