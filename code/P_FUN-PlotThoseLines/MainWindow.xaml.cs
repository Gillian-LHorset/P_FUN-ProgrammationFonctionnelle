using DataSeries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using ScottPlot.Plottables;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace P_FUN_PlotThoseLines {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        TemperatureContext dbContext = new TemperatureContext();
        List<(int, string)> loadableDatasets = new List<(int, string)>();
        // list of the Scatters present in the main graph
        private Dictionary<int, (Scatter graph, TextBlock? legend)> _showedTemperatureSets = new Dictionary<int, (Scatter, TextBlock?)>();
        private const int MAX_GRAPH_SHOWED = 5;


        public MainWindow() {
            InitializeComponent();
            LoadAllGraph();
            LoadDefaultGraph(); // load the default graphic at the launch of the application
            MainGraph.Refresh();
        }

        /// <summary>
        /// Toggle the checkability of the item in the section of graphic change and remove or add him in the main graphic section.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void OnGraphSelected(object sender, RoutedEventArgs e) {
            if (sender is MenuItem menuItem && menuItem.Tag is int id) {
                if (_showedTemperatureSets.ContainsKey(id)) {
                    DeleteDatasetInGraph(_showedTemperatureSets[id].graph, _showedTemperatureSets[id].legend!);
                    _showedTemperatureSets.Remove(id);
                } else {
                    (Scatter, TextBlock) scatterInfos = showData(DbCommunication.LoadSingleDataSet(dbContext, id).Result, DbCommunication.GetDatasetName(dbContext, id));
                    _showedTemperatureSets.Add(id, scatterInfos);
                }
            }

        }


        public async void dataImport(object sender, RoutedEventArgs e) {
            // open the file explorer
            var openFile = new OpenFileDialog();
            // can import only csv
            openFile.DefaultExt = "*.csv";
            openFile.Filter = "Csv Files (*.csv)|*.csv";
            // if the user chooses a file
            if (openFile.ShowDialog() == true) {
                string filePath = openFile.FileName;

                DataSeries<TemperatureSet> temperatures = DataSeries<TemperatureSet>.FromCsv(filePath, ParseTemperatrue);

                if (temperatures == null) {
                    MessageBox.Show($"Importation impossible.\nStructure des données incorrecte.");
                    return;
                }

                var DataSerieInfo = await DbCommunication.RegisterDataInDB(dbContext, temperatures);
                int? id = DataSerieInfo.id;
                string title = DataSerieInfo.title;
                Boolean? isDuplicate = DataSerieInfo.isDuplicate;

                if (title == null || id == null || isDuplicate == null) {
                    return;
                }

                var dataInfos = showData(temperatures, title);
                Scatter plot = dataInfos.Item1;
                TextBlock legend = dataInfos.Item2;

                // need to write in this form because of the nullable Boolean isDuplicate
                if (isDuplicate == true) {
                    if (_showedTemperatureSets.ContainsKey(id!.Value)) {
                        DeleteDatasetInGraph(_showedTemperatureSets[id!.Value].graph, _showedTemperatureSets[id!.Value].legend!);
                        _showedTemperatureSets[id!.Value] = (plot, legend);
                    }
                } else {
                    AddGraph(plot, legend, id!.Value);

                }

                //MessageBox.Show($"count : {temperatures.Count}");
            }
        }

        public (Scatter, TextBlock) showData(DataSeries<TemperatureSet> temp, string title) {
            double[] xPoints = temp.Values.Select(t => t.Time.ToOADate()).ToArray();
            double[] yPoints = temp.Values.Select(t => t.Temperature).ToArray();

            Scatter plot = MainGraph.Plot.Add.Scatter(xPoints, yPoints);
            MainGraph.Plot.Axes.DateTimeTicksBottom();

            // conversion du système de couleur de scottplot vers le système de WPF
            System.Windows.Media.Color wpfColor = System.Windows.Media.Color.FromArgb(
                plot.Color.A,
                plot.Color.R,
                plot.Color.G,
                plot.Color.B
            );

            TextBlock legend = ShowLegend(wpfColor, title);

            MainGraph.Refresh();
            return (plot, legend);
        }

        private TextBlock ShowLegend(System.Windows.Media.Color foregroundColor, string title) {
            Brush legendBrush = new SolidColorBrush(foregroundColor);

            TextBlock legendeText = new TextBlock() {
                Text = title,
                Padding = new Thickness(25),
                Foreground = legendBrush
            };
            GraphLegende.Children.Add(legendeText);
            return legendeText;
        }

        private TemperatureSet ParseTemperatrue(string[] cols) {
            TemperatureSet temp = new TemperatureSet(
                cols[0].Trim(),
                DateTime.Parse(cols[1]),
                // CultureInfo.InvariantCulture allow period and comma for double
                Double.Parse(cols[2], CultureInfo.InvariantCulture));

            return temp;
        }

        private void DeleteDatasetInGraph(Scatter plot, TextBlock legend) {
            MainGraph.Plot.Remove(plot);
            GraphLegende.Children.Remove(legend);
            MainGraph.Refresh();
        }

        private async void LoadAllGraph() {
            loadableDatasets = await DbCommunication.GetAllDataset(dbContext);
            if (loadableDatasets.Count < 1) {
                GraphiqueList.Items.Add(new MenuItem {
                    Header = "Aucun graphique disponible."
                });
                return;
            }
            loadableDatasets.ForEach(t => {
                AddMenuItem(t.Item1, t.Item2);
            });
        }

        private void AddMenuItem(int id, string header) {
            MenuItem menuItem = new MenuItem {
                Header = header,
                Tag = id,
                IsCheckable = true,
                IsChecked = false
            };
            menuItem.Click += OnGraphSelected;

            GraphiqueList.Items.Add(menuItem);
        }

        /// <summary>
        /// Load and show the scatter from the db to the main graphique, add his info in the list of all scatter and add the item in the list of "Changement de graphique".
        /// If the number of graphics is bigger than the maximum allow, remove the oldest scatter from the main graphic.
        /// </summary>
        /// <param name="plot"></param>
        /// <param name="legend"></param>
        /// <param name="id"></param>
        private void AddGraph(Scatter plot, TextBlock legend, int id) {
            _showedTemperatureSets.Add(DbCommunication.GetLastDataSetId(dbContext), (plot, legend));
            loadableDatasets.Add((id, legend.Text));
            AddMenuItem(id, legend.Text);

            if (_showedTemperatureSets.Count > MAX_GRAPH_SHOWED) { // TODO : à tester
                Scatter lastGraph = _showedTemperatureSets.Last().Value.graph;
                TextBlock lastLegend = _showedTemperatureSets.Last().Value.legend;
                DeleteDatasetInGraph(plot, legend);
                _showedTemperatureSets.Remove(_showedTemperatureSets.Last().Key);
            }
        }

        private void LoadDefaultGraph() {
            if (!loadableDatasets.Any()) {
                return;
            }
            int id = loadableDatasets.First().Item1;

            DataSeries<TemperatureSet> defaultTemps = DbCommunication.LoadSingleDataSet(dbContext, id).Result;

            (Scatter, TextBlock) scatterInfos = showData(DbCommunication.LoadSingleDataSet(dbContext, id).Result, DbCommunication.GetDatasetName(dbContext, id));
            _showedTemperatureSets.Add(id, scatterInfos);
        }
    }

    public class TemperatureContext : DbContext {
        // db structure
        public DbSet<TemperatureSet> Temp { get; set; }
        public TemperatureContext() { }

        public TemperatureContext(DbContextOptions<TemperatureContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            optionsBuilder.UseSqlite(
                @"Data Source=temperature.db");
        }
    }
}

