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

        public static async Task<(string title, int? id, Boolean? isDuplicate)> RegisterDataInDB(TemperatureContext dbContext, DataSeries<TemperatureSet> temperatures) {

            // verify si la db existe
            await dbContext.Database.EnsureCreatedAsync();

            // crée une hashmap avec un tuple (t.Location, t.Time) comme clé et TemperatureSet comme valeur
            Dictionary<(string Location, DateTime Time), TemperatureSet> existingDataDictionnary = (await dbContext.Temp.ToListAsync())
                .ToDictionary(t => (t.Location, t.Time));

            // copie de la list temperatures
            List<TemperatureSet> incomingList = temperatures.Values.ToList();

            // doublons
            List<TemperatureSet> updates = incomingList.Where(t => existingDataDictionnary.ContainsKey((t.Location, t.Time))).ToList();
            // valeurs inédites
            List<TemperatureSet> inserts = incomingList.Where(t => !existingDataDictionnary.ContainsKey((t.Location, t.Time))).ToList();

            string title = "";
            int id;
            Boolean isDuplicate = false;

            if (updates.Any()) {
                // cas où il y a des doublons dans la db

                // récupère le DataSetId de la première donnée en doublon
                int existingDataSetId = existingDataDictionnary[(updates.First().Location, updates.First().Time)].DataSetId;

                // écrase uniquement la température des doublons
                updates.ForEach(t => existingDataDictionnary[(t.Location, t.Time)].Temperature = t.Temperature);

                // les valeurs inédites reçoivent le meme DataSetId que les données update
                // car on part du principe qu'il s'agit du meme set de données
                inserts.ForEach(t => t.DataSetId = existingDataSetId);

                MessageBox.Show("Des doublons ont été détectés !\nLes données importées ont écrasé les données précédentes.", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                title = GetDatasetName(dbContext, existingDataSetId);
                id = existingDataSetId;
                isDuplicate = true;
            } else {
                // cas où il n'y a pas de doublons dans la db

                title = Microsoft.VisualBasic.Interaction.InputBox(
                    "Veuillez entrer le nom pour le dataset :",
                    "Nom du Dataset",
                    ""
                );

                if (string.IsNullOrWhiteSpace(title)) {
                    MessageBox.Show("Nommer le dataset est obligatoire.", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return (null, null, null);
                }

                if (title.Length > 100) {
                    MessageBox.Show("Le nom choisi pour le dataset est trop long.", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return (null, null, null);
                }

                // nouvel id des données importées
                int newId = GetLastDataSetId(dbContext) + 1;

                // ajout de la nouvelle serie 
                await dbContext.Database.ExecuteSqlInterpolatedAsync(
                    $"INSERT INTO t_dataset (dataset_id, title) VALUES ({newId}, {title})"
                );

                // Toutes les données reçoivent ce newId
                inserts = inserts.Select(ChangeId(newId)).ToList(); // TODO : vérifier si ca marche
                inserts.ForEach(t => t.DataSetId = newId);
                id = newId;
            }

            // ajout des nouvelles données qui peuvent être présentes dans les deux cas du if précédent
            if (inserts.Any()) {
                dbContext.Temp.AddRange(inserts);
            }

            await dbContext.SaveChangesAsync();
            return (title, id, isDuplicate);
        }

        public static async Task<DataSeries<TemperatureSet>> LoadSingleDataSet(TemperatureContext dbContext, int datasetId) {

            // verify si la db existe
            await dbContext.Database.EnsureCreatedAsync();

            // take all the rows from the db
            var rows = dbContext.Temp.FromSql(
                    $"SELECT * FROM Temp WHERE DataSetId = {datasetId}"
                );

            if (!rows.Any()) {
                return null;
            }

            DataSeries<TemperatureSet> temperatures = DataSeries<TemperatureSet>.From(rows);

            return temperatures;
        }

        public static string GetDatasetName(TemperatureContext tempContext, int datasetId) {
            string? title = tempContext.Database
            // SqlQuery return the value from a field named "Value"
            .SqlQuery<string>($"SELECT title AS Value FROM t_dataset WHERE dataset_id = {datasetId} LIMIT 1")
            .FirstOrDefault();

            return title ?? "";
        }

        public static async Task<List<(int, string)>> GetAllDataset(TemperatureContext dbContext) {
            // verify si la db existe
            await dbContext.Database.EnsureCreatedAsync();

            var datasets = new List<(int, string)>();

            var connection = dbContext.Database.GetDbConnection();
            await connection.OpenAsync();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT dataset_id, title FROM t_dataset";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync()) {
                datasets.Add((reader.GetInt32(0), reader.GetString(1)));
            }

            return datasets;
        }

        // ChangeId correspond au nom de la fonction
        // la première fleche coresspond à la définition de la fonction ChangeId
        // id correspond au premier paramètre de la fonction
        // La deuxième fleche correspond à la déclaration de la deuxième fonction (temp)(celle en paramètre)
        // La troisième fleche défini la fonction temp
        public static Func<int, Func<TemperatureSet, TemperatureSet>> ChangeId =>
            id => temp => new TemperatureSet(temp.Location, temp.Time, temp.Temperature) {
                DataSetId = id
            };
    }
}
