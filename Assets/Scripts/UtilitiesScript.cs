using ReadWriteCsv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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

    public static Dictionary<String, int> readConfigurationFile() {
        Dictionary<String, int> conf = new Dictionary<string, int>();
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
                    conf.Add(singleConf[0], Convert.ToInt32(singleConf[1]));
                }
            }
        }
        /*
        conf.TryGetValue("POS_BELOW_MAX_REACH", out POS_BELOW_MAX_REACH);
        conf.TryGetValue("POS_ABOVE_MAX_REACH", out POS_ABOVE_MAX_REACH);
        conf.TryGetValue("NUMBER_OF_TRIAL_PER_POSITION", out NUMBER_OF_TRIAL_PER_POSITION);
        conf.TryGetValue("REMAINING_TIME_TO_TOUCH_TARGET", out REMAINING_TIME_TO_TOUCH_TARGET);
        conf.TryGetValue("NEXT_TRIAL_START_TIME", out NEXT_TRIAL_START_TIME);
        conf.TryGetValue("LOWER_RANGE", out LOWER_RANGE);
        conf.TryGetValue("HIGHER_RANGE", out HIGHER_RANGE);
        */
        return conf;
    }
}