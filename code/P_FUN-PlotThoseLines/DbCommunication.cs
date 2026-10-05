using DataSeries;
using Microsoft.EntityFrameworkCore;

namespace P_FUN_PlotThoseLines {
    static public class DbCommunication {
        public static int GetLastDataSetId(TemperatureContext tempContext) {
            int id = tempContext.Temp.FromSql($"SELECT * FROM Temp ORDER BY DataSetId DESC LIMIT 1")
            .Select(t => t.DataSetId)
            .FirstOrDefault();

            return id;
        }

        public static async void RegisterDataInDB(TemperatureContext dbContext, DataSeries<TemperatureSet> temperatures, string title) {

            // verify if the db exist
            dbContext.Database.EnsureCreated();

            int newId = GetLastDataSetId(dbContext) + 1;

            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT INTO t_dataset (dataset_id, title) VALUES ({newId}, {title})"
            );

            // add values to the db
            temperatures.Values.ToList().ForEach(t => {
                t.DataSetId = newId;
                dbContext.Temp.Add(t);
            });
            await dbContext.SaveChangesAsync();
        }

        public static string GetDatasetName(TemperatureContext tempContext, int datasetId) {
            string? title = tempContext.Database
            .SqlQuery<string>($"SELECT title FROM t_dataset WHERE dataset_id = {datasetId} LIMIT 1")
            .FirstOrDefault();

            return title;
        }
    }
}
