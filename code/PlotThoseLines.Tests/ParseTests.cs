using DataSeries;

namespace PlotThoseLines.Tests {
    [TestClass]
    public sealed class ParseTests {
        [TestMethod]
        public void ParseTemperature_Valide_RetourneTemperatureSet() {
            TemperatureSet cols = new TemperatureSet { "Lausanne", "2026-01-15", "21.5" };


            DataSeries<TemperatureSet> temperatures = DataSeries<TemperatureSet>.From(cols);

            Assert.AreEqual("Lausanne", temperatures.Location);
        }

    }
}
