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
        // list of the Scatters present in the main graph
        private Dictionary<int, (Scatter graph, TextBlock? legend)> _showedTemperatureSets = new Dictionary<int, (Scatter, TextBlock?)>();
        private const int MAX_GRAPH_SHOWED = 5;


        public MainWindow() {
            InitializeComponent();

            MainGraph.Refresh();
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

                // need to write in this form because of the nullable Boolean
                if (isDuplicate == true) {
                    if (_showedTemperatureSets.ContainsKey(id!.Value) && id is not null) { // case the plot is already present in the graph
                        RemovePlotFromTheMainGraph(_showedTemperatureSets[id!.Value].graph);
                        GraphLegende.Children.Remove(_showedTemperatureSets[id!.Value].legend);
                        _showedTemperatureSets[id!.Value] = (plot, legend);
                    } else {
                        _showedTemperatureSets.Add(DbCommunication.GetLastDataSetId(dbContext), (plot, legend));
                    }
                } else {
                    _showedTemperatureSets.Add(DbCommunication.GetLastDataSetId(dbContext), (plot, legend));
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

        private void RemovePlotFromTheMainGraph(Scatter plot) {
            MainGraph.Plot.Remove(plot);
        }
    }

    public class TemperatureContext : DbContext {
        // db structure
        public DbSet<TemperatureSet> Temp { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            optionsBuilder.UseSqlite(
                @"Data Source=temperature.db");
        }
    }
}

