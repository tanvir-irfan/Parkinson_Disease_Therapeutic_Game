using ReadWriteCsv;
using System;
using System.IO;
using System.Text;

public class UtilitiesScript {
    public static void writeTest(string p, string fileName) {
        // Write sample data to CSV file
        char[] delimiterChars = { '\n' };
        //FileStream fs = File.Create(fileName);
        using ( CsvFileWriter writer = new CsvFileWriter(fileName) ) {
            string[] allRows = p.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
            foreach ( string rowData in allRows ) {
                delimiterChars[0] = ',';
                string[] allCol = rowData.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
                CsvRow row = new CsvRow();
                foreach ( string colData in allCol ) {
                    row.Add(colData);
                }

                writer.WriteRow(row);
            }

        }
    }

    public static bool isValid(string str) {
        if ( str == null || str.Length == 0 || str == "" ) {
            return false;
        } else {
            return true;
        }
    }

    public static void writeToFile(StringBuilder outputStringBuilder) {
        String outputPath = "Data\\test.csv";
        //String outputPath = "test.csv";                       

        if ( !File.Exists(outputPath) ) {
            File.WriteAllText(outputPath, outputStringBuilder.ToString());
        } else {
            File.AppendAllText(outputPath, outputStringBuilder.ToString());
        }
    }
}