using DataSeries;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Windows;

namespace P_FUN_PlotThoseLines {
    static public class DbCommunication {
        public static int GetLastDataSetId(TemperatureContext tempContext) {
            int id = tempContext.Temp.FromSql($"SELECT * FROM Temp ORDER BY DataSetId DESC LIMIT 1")
            .Select(t => t.DataSetId)
            .FirstOrDefault();

            return id;
        }

        public static async Task RegisterDataInDB(TemperatureContext dbContext, DataSeries<TemperatureSet> temperatures) {

            // verify si la db existe
            await dbContext.Database.EnsureCreatedAsync();

            // crée une hashmap avec le tuple (t.Location, t.Time) comme clé et TODO comme valeur
            var existingDataDictionnary = (await dbContext.Temp.ToListAsync())
                .ToDictionary(t => (t.Location, t.Time));

            // copie de la list temperatures
            List<TemperatureSet> incomingList = temperatures.Values.ToList();

            // doublons
            List<TemperatureSet> updates = incomingList.Where(t => existingDataDictionnary.ContainsKey((t.Location, t.Time))).ToList();
            // valeurs inédites
            List<TemperatureSet> inserts = incomingList.Where(t => !existingDataDictionnary.ContainsKey((t.Location, t.Time))).ToList();


            if (updates.Any()) {
                // cas où il y a des doublons dans la db

                // récupère le DataSetId de la première donnée en doublon
                int existingDataSetId = existingDataDictionnary[(updates[0].Location, updates[0].Time)].DataSetId;

                // écrase uniquement la température des doublons
                updates.ForEach(t => existingDataDictionnary[(t.Location, t.Time)].Temperature = t.Temperature);

                // les valeurs inédites reçoivent le meme DataSetId que les données update
                // car on part du principe qu'il s'agit du meme set de données
                inserts.ForEach(t => t.DataSetId = existingDataSetId);

            } else {
                // cas où il n'y a pas de doublons dans la db

                string title = Microsoft.VisualBasic.Interaction.InputBox(
                    "Veuillez entrer le nom du dataset :",
                    "Nom du Dataset",
                    ""
                );

                if (string.IsNullOrWhiteSpace(title)) {
                    MessageBox.Show("Le nom du dataset est obligatoire.", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // nouvel id des données importées
                int newId = GetLastDataSetId(dbContext) + 1;

                // ajout de la nouvelle serie 
                await dbContext.Database.ExecuteSqlInterpolatedAsync(
                    $"INSERT INTO t_dataset (dataset_id, title) VALUES ({newId}, {title})"
                );

                // Toutes les données reçoivent ce newId
                inserts.ForEach(t => t.DataSetId = newId);
            }

            // ajout des nouvelles données qui peuvent être présentes dans les deux cas du if
            if (inserts.Any()) {
                dbContext.Temp.AddRange(inserts);
            }

            await dbContext.SaveChangesAsync();
        }

        public static string GetDatasetName(TemperatureContext tempContext, int datasetId) {
            string? title = tempContext.Database
            .SqlQuery<string>($"SELECT title FROM t_dataset WHERE dataset_id = {datasetId} LIMIT 1")
            .FirstOrDefault();

            return title ?? "";
        }
    }
}
