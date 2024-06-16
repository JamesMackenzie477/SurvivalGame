using System.IO;
using System;

public class Logging
{
    // logs the string to a log file with a timestamp
    public static void LogToFile(string message)
    {
        // opens log file
        StreamWriter log = new StreamWriter("Debug.log", true);
        // appends message to log file with current time
        log.WriteLine(DateTime.Now.ToString("h:mm:ss tt | ") + message);
        // closes log
        log.Close();
    }
}
