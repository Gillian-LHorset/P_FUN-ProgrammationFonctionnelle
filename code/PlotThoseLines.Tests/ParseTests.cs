using DataSeries;
using P_FUN_PlotThoseLines;
namespace PlotThoseLines.Tests {
    [TestClass]
    public sealed class ParseTests {
        [TestMethod]
        public void ParseTemperature_Valide_RetourneTemperatureSet() {

            List<TemperatureSet> cols = new List<TemperatureSet>{
                new TemperatureSet("Lausanne", new DateTime(2026-01-15), 21.5)
            };

            DataSeries<TemperatureSet> temperatures = DataSeries<TemperatureSet>.From(cols);

            Assert.AreEqual("Lausanne", temperatures.Values.First().Location);
        }

    }
}
