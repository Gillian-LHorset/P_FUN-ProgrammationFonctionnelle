# Questions de compréhension – Gillian L'Horset

## Question 1 – Accès aux données

**Passage** : `code/P_FUN-PlotThoseLines/DbCommunication.cs`, lignes 22-41 et 78

```csharp
var existingDataDictionnary = (await dbContext.Temp.ToListAsync())
    .ToDictionary(t => (t.Location, t.Time));

List<TemperatureSet> updates = incomingList.Where(t => existingDataDictionnary.ContainsKey((t.Location, t.Time))).ToList();
...
updates.ForEach(t => existingDataDictionnary[(t.Location, t.Time)].Temperature = t.Temperature);
...
await dbContext.SaveChangesAsync();
```

- Qu'est-ce que c'est que `dbContext` ?
- Vous avez utilisé Entity Framework. C'est quoi ? pourquoi l'avoir choisi ?
- Que signifie async/await ?
- Qu'est-ce qu'une migration ?

---

## Question 2 – Requêtes SQL écrites à la main

**Passage** : `code/P_FUN-PlotThoseLines/DbCommunication.cs`, lignes 9, 65-67 et 83

```csharp
int id = tempContext.Temp.FromSql($"SELECT * FROM Temp ORDER BY DataSetId DESC LIMIT 1")
    .Select(t => t.DataSetId).FirstOrDefault();
...
await dbContext.Database.ExecuteSqlInterpolatedAsync(
    $"INSERT INTO t_dataset (dataset_id, title) VALUES ({newId}, {title})");
...
.SqlQuery<string>($"SELECT title FROM t_dataset WHERE dataset_id = {datasetId} LIMIT 1")
```

- La variable `title` vient directement de ce que l'utilisateur tape dans l'`InputBox`. Si l'utilisateur saisit `Genève'); DROP TABLE Temp; --`, que se passe-t-il ?
