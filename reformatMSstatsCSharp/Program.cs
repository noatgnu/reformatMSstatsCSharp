using System;
using System.Globalization;
using System.IO;
using CsvHelper;

namespace reformatMSstatsCSharp
{
    internal class Program
    {
        public static void Main(string[] args)
        
        {
            var ex = new Experiment(
                @"C:\Users\Toan\go\src\github.com\noatgnu\reformatMS\bin\20180228_YP_DEG_wce_FDR_renamed.csv", 
                @"C:\Users\Toan\go\src\github.com\noatgnu\reformatMS\bin\20180228_YP_DEG_wce_Ions_renamed.csv",
                0.05);
            ex.ReadFDR();
            var writer = new StreamWriter(@"test.csv");
            var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            //csv.WriteHeader(typeof(OutputRow));
            csv.WriteRecords(ex.ProcessIon());
            writer.Flush();
            writer.Close();
        }
    }
}