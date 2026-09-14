using DataSeries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Windows;

namespace P_FUN_PlotThoseLines {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();


            WpfPlot1.Refresh();
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

                using (var db = new TemperatureContext()) {
                    // verify if the db exist
                    db.Database.EnsureCreated();
                    // add values to the db
                    foreach (var item in temperatures.Values) {
                        db.Temp.Add(item);
                        await db.SaveChangesAsync();
                    }

                }

                MessageBox.Show($"count : {temperatures.Count}");
            }
        }

        public void showData(TemperatureSet temp) {

        }

        private TemperatureSet ParseTemperatrue(string[] cols) {
            TemperatureSet temp = new TemperatureSet(cols[0], DateTime.Parse(cols[1]), Double.Parse(cols[2]));

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

