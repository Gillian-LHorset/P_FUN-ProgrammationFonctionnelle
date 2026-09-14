using DataSeries;
using Microsoft.Win32;
using System.Windows;

namespace P_FUN_PlotThoseLines {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();


            double[] dataX = { 1, 2, 3, 4, 5 };
            double[] dataY = { 1, 4, 9, 16, 25 };
            WpfPlot1.Plot.Add.Scatter(dataX, dataY);
            WpfPlot1.Refresh();
        }


        public void dataImport(object sender, RoutedEventArgs e) {
            var openFile = new OpenFileDialog();
            openFile.DefaultExt = "*.csv";
            openFile.Filter = "Csv Files (*.csv)|*.csv";
            if (openFile.ShowDialog() == true) {
                string filePath = openFile.FileName;

                DataSeries<TemperatureSet> temperatures;
                temperatures = DataSeries<TemperatureSet>.FromCsv(filePath, ParseTemperatrue);

                MessageBox.Show($"count : {temperatures.Count}");
            }
        }

        TemperatureSet ParseTemperatrue(string[] cols) {
            TemperatureSet temp = new TemperatureSet(cols[0], DateTime.Parse(cols[1]), Double.Parse(cols[2]));

            return temp;
        }


    }
}

