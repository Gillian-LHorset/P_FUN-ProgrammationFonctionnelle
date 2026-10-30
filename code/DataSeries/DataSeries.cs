namespace DataSeries {
    public class DataSeries<T> {
        private readonly IEnumerable<T> _data;

        private DataSeries(IEnumerable<T> data) => _data = data;

        public static DataSeries<T> From(IEnumerable<T> source) => new DataSeries<T>(source);

        public static DataSeries<T> FromCsv(string filename, Func<string[], T> parser) {
            try {
                List<T> data = File.ReadLines(filename)
                    .Skip(1)
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(line => line.Split(';'))
                    .Where(cols => !cols.Any(string.IsNullOrEmpty))
                    .Select(parser)
                    .ToList();

                return From(data);
            } catch (Exception e) {
                Console.WriteLine("exeption :" + e.ToString());
                return null;
            }
        }

        public int Count => _data.Count();
        //public IEnumerable<T> Values => _data.Select(dp => dp.Value);
        public IEnumerable<T> Values => _data;

        public override string ToString() {
            return $"DataSerie<{typeof(T).Name}>: {Count} points: {Environment.NewLine}{String.Join(Environment.NewLine, _data.Select(s => s).ToArray())}";
        }
    }
}
