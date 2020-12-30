using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;

namespace reformatMSstatsCSharp
{
    public class FDRFile
    {
        public Dictionary<string, FDRValue> FDRValueMap { get; set; }
        public List<string> Samples { get; set; }
        public double FDRCutOff { get; set; }
        public string[] Header { get; set; }
        public FDRFile(string filePath, double fdrCutOff)
        {
            this.FDRCutOff = fdrCutOff;
            FDRValueMap = new Dictionary<string, FDRValue>();
            TextReader reader = new StreamReader(filePath);
            var csvReader = new CsvReader(reader, CultureInfo.CurrentCulture);
            csvReader.Read();
            csvReader.ReadHeader();
            this.Header = csvReader.Context.HeaderRecord;

            while (csvReader.Read())
            {
                var row = new FDRRow();
                for (var index = 0; index < 7; index++)
                {
                    switch (this.Header[index])
                    {
                        case "Protein":
                            row.Protein = csvReader.GetField(index);
                            break;
                        case "Peptide":
                            row.Peptide = csvReader.GetField(index);
                            break;
                        case "Label":
                            row.Label = csvReader.GetField(index);
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
                        case "Decoy":
                            row.Decoy = csvReader.GetField(index) != "FALSE";
                            break;
                        default:
                            Console.WriteLine($"Unexpected column name: {this.Header[index]}");
                            break;
                    }
                }

                var values = new List<double>();
                var valuesPass = new List<bool>();
                var rowPass = false;
                for (var index = 7; index < this.Header.Length; index++)
                {
                    var value = Convert.ToDouble(csvReader.GetField(index));
                    values.Add(value);
                    if (value < this.FDRCutOff)
                    {
                        valuesPass.Add(true);
                        rowPass = true;
                    }
                    else
                    {
                        valuesPass.Add(false);
                    }

                }
                var samples = new FDRValue(values, row.Decoy, valuesPass, rowPass);
                this.AddToMap(row.Protein, row.Peptide, row.PrecursorCharge, row.RT, samples);
                
                //Console.WriteLine(csvReader.GetField(0));
            }
            reader.Close();
        }

        public void AddToMap(string protein, string peptide, string precursorCharge, string rt, FDRValue fdrValue)
        {
            this.FDRValueMap.Add($"{protein}-{peptide}-{rt}-{precursorCharge}", fdrValue);
        }
    }

    public sealed class FDRRow
    {
        public string Protein { get; set; }
        public string Peptide { get; set; }
        public string Label { get; set; }
        public string PrecursorMZ { get; set; }
        public string PrecursorCharge { get; set; }
        public string RT { get; set; }
        public bool Decoy { get; set; }
    }

    public sealed class FDRValue
    {
        public FDRValue(List<double> values, bool decoy, List<bool> valuesPass, bool rowPass=false)
        {
            this.Values = values;
            this.Decoy = decoy;
            this.RowPass = rowPass;
            this.ValuesPass = valuesPass;
        }

        public bool Decoy { get; set; }
        public List<double> Values { get; set; }
        public bool RowPass;
        public List<bool> ValuesPass { get; set; }
    }
}