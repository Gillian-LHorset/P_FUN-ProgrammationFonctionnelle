using Microsoft.EntityFrameworkCore;

namespace P_FUN_PlotThoseLines {
    static public class DbCommunication {
        public static int GetLastDataSetId(TemperatureContext tempContext) {
            int id = tempContext.Temp.FromSql($"SELECT * FROM Temp ORDER BY DataSetId DESC LIMIT 1")
            .Select(t => t.DataSetId)
            .FirstOrDefault();

            return id;
        }
    }
}
