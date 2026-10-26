using DataSeries;
using P_FUN_PlotThoseLines;
using System.Globalization;
namespace PlotThoseLines.Tests {
    [TestClass]
    public sealed class DataSeriesTests {
        [TestMethod]
        public void ParseTemperature_Valide_RetourneTemperatureSet() {

            List<TemperatureSet> cols = new List<TemperatureSet>{
                new TemperatureSet("Lausanne", new DateTime(2026, 1, 15), 21.5)
            };

            DataSeries<TemperatureSet> temperatures = DataSeries<TemperatureSet>.From(cols);

            Assert.AreEqual("Lausanne", temperatures.Values.First().Location);
            Assert.AreEqual(21.5, temperatures.Values.First().Temperature);
            Assert.AreEqual("2026-01-15", temperatures.Values.First().Time.ToString("yyyy-MM-dd"));
        }

        [TestMethod]
        public void FromCsv_ValidFile_ParsesCorrectly() {
            // Arrange
            string tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, "Location;Time;Temperature\nParis;2026-06-06T12:00:00;22.5");

            // Act
            var result = DataSeries<TemperatureSet>.FromCsv(tempFile, cols => new TemperatureSet(
                cols[0],
                DateTime.Parse(cols[1]),
                double.Parse(cols[2], CultureInfo.InvariantCulture)
            ));

            // Assert
            Assert.IsNotNull(result);

            File.Delete(tempFile);
        }

        [TestMethod]
        public void FromCsv_WithBlankLines_SkipsThem() {
            // Arrange
            string tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, "Location;Time;Temperature\n   \n\nParis;2026-06-06T12:00:00;22.5");

            // Act
            var result = DataSeries<TemperatureSet>.FromCsv(tempFile, cols => new TemperatureSet(
                cols[0],
                DateTime.Parse(cols[1]),
                double.Parse(cols[2], CultureInfo.InvariantCulture)
            ));

            // Assert
            Assert.IsNotNull(result);

            File.Delete(tempFile);
        }

        [TestMethod]
        public void FromCsv_WithEmptyColumns_SkipsIncompleteLines() {
            // Arrange
            string tempFile = Path.GetTempFileName();
            // La première ligne de données a une colonne vide (;;)
            File.WriteAllText(tempFile, "Location;Time;Temperature\nParis;;22.5\nLyon;2026-06-06T12:00:00;20.0");

            // Act
            var result = DataSeries<TemperatureSet>.FromCsv(tempFile, cols => new TemperatureSet(
                cols[0],
                DateTime.Parse(cols[1]),
                double.Parse(cols[2], CultureInfo.InvariantCulture)
            ));

            // Assert
            Assert.IsNotNull(result);

            File.Delete(tempFile);
        }

        [TestMethod]
        public void FromCsv_NonExistentFile_CatchesExceptionAndReturnsNull() {
            // Arrange
            string nonExistentFile = "fichier_inexistant_99999.csv";

            // Act
            var result = DataSeries<TemperatureSet>.FromCsv(nonExistentFile, cols => new TemperatureSet(
                cols[0],
                DateTime.Parse(cols[1]),
                double.Parse(cols[2])
            ));

            // Assert
            Assert.IsNull(result);
        }
    }
}
