namespace P_FUN_PlotThoseLines {
    public class TemperatureSet {
        private TemperatureSet() { }

        public TemperatureSet(string location, DateTime time, double temperature) {
            Location = location;
            Time = time;
            Temperature = temperature;
        }

        public int Id { get; set; }
        public string Location { get; set; }
        public DateTime Time { get; set; }
        public double Temperature { get; set; }
    }
}
