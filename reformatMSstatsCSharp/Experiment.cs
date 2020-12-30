using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace reformatMSstatsCSharp
{
    public class Experiment
    {
        private Regex rx = new Regex(@"(\w+)_(\d+)(v\d+)*$");
        private string _fdrFile;
        private FDRFile fdrFile;
        private string _ionFile;
/*
        private IonFile ionFile;
*/
        private double _fdrCutOff;
        public List<Sample> Samples;
        public Experiment(string fdrFile, string ionFile, double fdrCutOff)
        {
            this._fdrFile = fdrFile;
            this._ionFile = ionFile;
            this._fdrCutOff = fdrCutOff;
        }

        public void ReadFDR()
        {
            this.fdrFile = new FDRFile(this._fdrFile, this._fdrCutOff);
            this.Samples = new List<Sample>();
            for (var index = 7; index < this.fdrFile.Header.Length; index++)
            {
                var matches = this.rx.Match(this.fdrFile.Header[index]);
                if (matches.Success)
                {
                    this.Samples.Add(new Sample(this.fdrFile.Header[index], matches.Groups[1].Value, matches.Groups[2].Value));
                }
                else
                {
                    Console.WriteLine($"Cannot parse sample: {this.fdrFile.Header[index]}");
                }

            }
        }

        public IEnumerable<OutputRow> ProcessIon()
        {
            foreach (var ionRow in IonFile.IterateIonFile(this._ionFile))
            {
                var key = $"{ionRow.Protein}-{ionRow.Peptide}-{ionRow.RT}-{ionRow.PrecursorCharge}";
                if (this.fdrFile.FDRValueMap.ContainsKey(key))
                {
                    if (this.fdrFile.FDRValueMap[key].RowPass)
                    {
                        var values = new string[this.Samples.Count];
                        var blankCount = 0;
                        for (var index = 0; index < this.Samples.Count; index++)
                        {
                            if (this.fdrFile.FDRValueMap[key].ValuesPass[index])
                            {
                                values[index] = ionRow.SamplesArea[index];
                                if (values[index] == "")
                                {
                                    blankCount++;
                                }
                            }
                            else
                            {
                                blankCount++;
                                values[index] = "";
                            }
                        }

                        if (blankCount < this.Samples.Count)
                        {
                            for (var index = 0; index < this.Samples.Count; index++)
                            {
                                var outputRow = new OutputRow();
                                outputRow.ProteinName = ionRow.Protein;
                                outputRow.PeptideSequence = ionRow.Peptide;
                                outputRow.PrecursorCharge = ionRow.PrecursorCharge;
                                outputRow.FragmentIon = $"{ionRow.IonType}{ionRow.Residue}";
                                outputRow.ProductCharge = ionRow.FragmentCharge;
                                outputRow.IsotopeLabelType = "L";
                                outputRow.Condition = this.Samples[index].SampleName;
                                outputRow.BioReplicate = this.Samples[index].ColumnName;
                                outputRow.Run = index+1;
                                outputRow.Intensity = values[index];
                                yield return outputRow;
                            }
                        }
                        
                    }
                }
                
            }
        }
    }

    public struct Sample
    {
        public string ColumnName;
        public string SampleName;
        public string ReplicateID;

        public Sample(string columnName, string sampleName, string replicateID)
        {
            this.ColumnName = columnName;
            this.SampleName = sampleName;
            this.ReplicateID = replicateID;
        }
    }

    public class OutputRow
    {
        public string ProteinName { get; set; }
        public string PeptideSequence { get; set; }
        public string PrecursorCharge { get; set; }
        public string FragmentIon { get; set; }
        public string ProductCharge { get; set; }
        public string IsotopeLabelType { get; set; }
        public string Condition { get; set; }
        public string BioReplicate { get; set; }
        public int Run { get; set; }
        public string Intensity { get; set; }
    }
}