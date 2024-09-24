using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using System.Linq;

namespace CodeLineCounter.Utils
{
    public class CsvHandler
    {
        public static void Serialize<T>(IEnumerable<T> data, string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(data);
            }
        }

        public static IEnumerable<T> Deserialize<T>(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                return csv.GetRecords<T>().ToList();
            }
        }
    }

}
