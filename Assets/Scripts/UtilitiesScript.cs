using ReadWriteCsv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class UtilitiesScript {
    public static void writeTest(string p, string fileName, bool isAppendMode) {    
        // Write sample data to CSV file
        char[] delimiterChars = { '\n' };
        //FileStream fs = File.Create(fileName);
        using ( CsvFileWriter writer = new CsvFileWriter(fileName, isAppendMode) ) {
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

    public static Dictionary<String, double> readConfigurationFile() {
        Dictionary<String, double> conf = new Dictionary<string, double>();
        //String conf_Path = "C:\\Users\\SAVE_UTSA\\Desktop\\Tanvir\\LimbTest_Beta\\_build\\configuration_file.txt";
        String conf_Path = "configuration_file.txt";

        StreamReader read = new StreamReader(conf_Path);
        if ( read == null ) {
            //Debug.Log("configuration_file.txt not found or not readable");
            conf = null;
        } else {
            String line = read.ReadLine();
            while ( line != null ) {
                char delim = '=';
                line = read.ReadLine();
                //Debug.Log ( line );
                if ( line != null && line.Contains(delim.ToString()) && !line.Contains("#") ) {
                    String[] singleConf = line.Split(delim);
                    conf.Add(singleConf[0], Convert.ToDouble(singleConf[1]));
                }
            }
        }        
        return conf;
    }

    public static float clampAngle(double angle, float min, float max) {
        if ( angle < -360F )
            angle += 360F;
        if ( angle > 360F )
            angle -= 360F;
        return Mathf.Clamp((float)angle, min, max);
    }
}