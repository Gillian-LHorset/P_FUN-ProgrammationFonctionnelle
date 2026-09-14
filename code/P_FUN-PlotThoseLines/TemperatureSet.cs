namespace P_FUN_PlotThoseLines {
    public class TemperatureSet {
        private TemperatureSet() { }

        public TemperatureSet(string location, DateTime time, double temperature) {
            this.location = location;
            this.time = time;
            this.temperature = temperature;
        }

        public int Id { get; set; }
        public string location { get; set; }
        public DateTime time { get; set; }
        public double temperature { get; set; }
    }
}
