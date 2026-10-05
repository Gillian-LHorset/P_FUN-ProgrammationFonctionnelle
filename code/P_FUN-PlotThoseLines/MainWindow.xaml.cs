using DataSeries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace P_FUN_PlotThoseLines {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        TemperatureContext dbContext = new TemperatureContext();

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

                // enter a name for the dataset
                string title = Microsoft.VisualBasic.Interaction.InputBox(
                    "Veuillez entrer le nom du dataset :",
                    "Nom du Dataset",
                    ""
                );

                if (string.IsNullOrWhiteSpace(title)) {
                    MessageBox.Show("Le nom du dataset ne peut pas être vide.", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DbCommunication.RegisterDataInDB(dbContext, temperatures, title);

                showData(temperatures, title);

                //MessageBox.Show($"count : {temperatures.Count}");
            }
        }

        public void showData(DataSeries<TemperatureSet> temp, string title) {
            double[] xPoints = temp.Values.Select(t => t.Time.ToOADate()).ToArray();
            double[] yPoints = temp.Values.Select(t => t.Temperature).ToArray();

            var plot = MainGraph.Plot.Add.Scatter(xPoints, yPoints);
            MainGraph.Plot.Axes.DateTimeTicksBottom();

            // conversion du système de couleur de scottplot vers le système de WPF
            var wpfColor = System.Windows.Media.Color.FromArgb(
                plot.Color.A,
                plot.Color.R,
                plot.Color.G,
                plot.Color.B
            );

            Brush graphColor = new SolidColorBrush(wpfColor);

            TextBlock legendeText = new TextBlock() {
                Text = title,
                Padding = new Thickness(25),
                Foreground = graphColor
            };
            GraphLegende.Children.Add(legendeText);

            MainGraph.Refresh();
        }

        private TemperatureSet ParseTemperatrue(string[] cols) {
            TemperatureSet temp = new TemperatureSet(cols[0].Trim(), DateTime.Parse(cols[1]), Double.Parse(cols[2]));

            return temp;
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

