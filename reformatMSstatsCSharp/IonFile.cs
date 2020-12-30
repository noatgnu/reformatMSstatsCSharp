using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;

namespace reformatMSstatsCSharp
{
    public class IonFile
    {
        public IonFile(string filePath)
        {
            
        }

        public static IEnumerable<IonRow> IterateIonFile(string filePath)
        {
            TextReader reader = new StreamReader(filePath);
            var csvReader = new CsvReader(reader, CultureInfo.CurrentCulture);
            csvReader.Read();
            csvReader.ReadHeader();
            var header = csvReader.Context.HeaderRecord;
            var sampleCount = header.Length - 9;
            while (csvReader.Read())
            {
                var row = new IonRow();
                for (var index = 0; index < 9; index++)
                {
                    switch (header[index])
                    {
                        case "Protein":
                            row.Protein = csvReader.GetField(index);
                            break;
                        case "Peptide":
                            row.Peptide = csvReader.GetField(index);
                            break;
                        case "Precursor MZ":
                            row.PrecursorMZ = csvReader.GetField(index);
                            break;
                        case "Precursor Charge":
                            row.PrecursorCharge = csvReader.GetField(index);
                            break;
                        case "RT":
                            row.RT = csvReader.GetField(index);
                            break;
                        case "Fragment MZ":
                            row.FragmentMZ = csvReader.GetField(index);
                            break;
                        case "Fragment Charge":
                            row.FragmentCharge = csvReader.GetField(index);
                            break;
                        case "Ion Type":
                            row.IonType = csvReader.GetField(index);
                            break;
                        case "Residue":
                            row.Residue = csvReader.GetField(index);
                            break;
                        default:
                            Console.WriteLine($"Unexpected column name: {header[index]}");
                            break;
                    }
                }

                var values = new List<string>();

                for (var index = 9; index < header.Length; index++)
                {
                    if ((csvReader.GetField(index).Length > 0) && (csvReader.GetField(index) != "0"))
                    {
                        values.Add(csvReader.GetField(index));
                    }
                    else
                    {
                        values.Add("");
                    }
                }
                row.SamplesArea = values;
                yield return row;
                
                
                //Console.WriteLine(csvReader.GetField(0));
            }
            reader.Close();
            yield break;
        }
    }

    public sealed class IonRow
    {
        public string Protein { get; set; }
        public string Peptide { get; set; }
        public string PrecursorMZ { get; set; }
        public string PrecursorCharge { get; set; }
        public string RT { get; set; }
        public string FragmentMZ { get; set; }
        public string FragmentCharge { get; set; }
        public string IonType { get; set; }
        public string Residue { get; set; }
        //public List<string> Samples { get; set; }
        public List<string> SamplesArea { get; set; }
    }
}