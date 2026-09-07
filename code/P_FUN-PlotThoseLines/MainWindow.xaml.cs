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

    }
}

// Source - https://stackoverflow.com/a/12593753
// Posted by Brian Hinchey, modified by community. See post 'Timeline' for change history
// Retrieved 2026-09-07, License - CC BY-SA 3.0

public static class Constants {
    public const string DateTimeUiFormat = "dd/MM/yyyy";

    //etc...
}
