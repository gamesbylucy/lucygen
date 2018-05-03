using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class LucygenEventLog {

    public static List<string> log = new List<string>();
    

    public static void printLog()
    {
        File.WriteAllText("C:\\Users\\jorda\\Documents\\GitHub Desktop Repos\\gamesbylucy\\lucygen\\lucygen_prototype\\RuntimeLogs\\mainlog.txt", string.Empty);
        StreamWriter file = new StreamWriter("C:\\Users\\jorda\\Documents\\GitHub Desktop Repos\\gamesbylucy\\lucygen\\lucygen_prototype\\RuntimeLogs\\mainlog.txt");
        file.WriteLine("START EVENT LOG");
        int lineNumber = 0;
        foreach (string logEvent in log)
        {
            file.WriteLine(lineNumber + " " +logEvent);
            lineNumber++;
        }
        file.Flush();
        file.Close();
    }

    public static void write(string logEvent)
    {
        log.Add(logEvent);
    }
}
