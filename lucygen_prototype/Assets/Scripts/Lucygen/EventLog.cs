using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class EventLog {

    public static List<string> mls_log = new List<string>();
    

    public static void WriteFile()
    {
        File.WriteAllText("C:\\Users\\jorda\\Documents\\GitHub Desktop Repos\\gamesbylucy\\lucygen\\lucygen_prototype\\RuntimeLogs\\mainlog.txt", string.Empty);
        StreamWriter file = new StreamWriter("C:\\Users\\jorda\\Documents\\GitHub Desktop Repos\\gamesbylucy\\lucygen\\lucygen_prototype\\RuntimeLogs\\mainlog.txt");
        file.WriteLine("START EVENT LOG");
        int lineNumber = 0;
        foreach (string logEvent in mls_log)
        {
            file.WriteLine(lineNumber + " " +logEvent);
            lineNumber++;
        }
        file.Flush();
        file.Close();
    }

    public static void Add(string logEvent)
    {
        mls_log.Add(logEvent);
    }
}
