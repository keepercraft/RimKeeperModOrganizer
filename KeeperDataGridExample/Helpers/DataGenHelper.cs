namespace KeeperDataGridExample.Helpers;
public static class DataGenHelper
{
    public static IEnumerable<TableModel> DataGridTestData(int count)
    {
        if (count <= 0) yield break;

        var names = new[]
        {
            "John Doe", "Jane Smith", "Alice Johnson", "Bob Brown", "Carol White",
            "Michael Davis", "Emily Wilson", "Daniel Taylor", "Olivia Anderson",
            "James Thomas", "Sophia Martinez", "William Moore", "Isabella Jackson",
            "Benjamin Martin", "Mia Lee", "Lucas Perez", "Charlotte Thompson",
            "Amelia Garcia", "Ethan Clark", "Harper Lewis",
            "Łukasz Kowalski", "François Dupont", "Jürgen Müller",
            "Søren Nielsen", "José Rodríguez", "Zoë Kravitz"
        };

                var countries = new[]
                {
            "USA", "UK", "Canada", "Germany", "France",
            "Poland", "Spain", "Italy", "Netherlands", "Sweden",
            "Norway", "Denmark", "Finland", "Switzerland", "Austria",
            "Czech Republic", "Hungary", "Portugal", "Belgium", "Ireland",
            "Japan", "China", "South Korea", "Brazil", "Mexico"
        };
        var rnd = new Random();

        for (int i = 0; i < count; i++)
        {
            yield return new TableModel
            {
                Name = names[rnd.Next(names.Length)],
                Age = rnd.Next(10, 99),
                Country = countries[rnd.Next(countries.Length)]
            };
        }
    }
}