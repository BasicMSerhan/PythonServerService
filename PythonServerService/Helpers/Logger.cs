using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RASMachineController
{
    static class Logger
    {

        private static bool isInitialized = false;

        private static List<string> LoggerFileTypes = new List<string>();

        private static string CurrentServicePath = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));

        /// <summary>
        /// Initializes the logger by cleaning up log files older than 10 days
        /// </summary>
        public static void Initialize(string[] LogTypes)
        {
            if (!isInitialized)
            {
                isInitialized = true;

                LoggerFileTypes.AddRange(LogTypes);

                AddLoggerType("Error");
                AddLoggerType("Debug");

                CleanUpLogFiles(10);
                Debug("LOGGER", "Logger Has Been Initialized");
            }
            else
            {
                Debug("LOGGER", "Logger Already Has Been Initialized, Initialize Not Successful");
            }
        }

        /// <summary>
        /// Cleans up (deletes) old log files, That are older from the current day by the integer days
        /// </summary>
        /// <param name="days"></param>
        public static void CleanUpLogFiles(int days)
        {
            foreach (var type in LoggerFileTypes)
            {
                CleanUpSingleLogFile(days, CurrentServicePath + "/Logs/" + type + "/");
            }
            Debug("LOGGER", "CleanUpLogFiles - Done All Log Files Cleanups");
        }

        /// <summary>
        /// Loops through all the files in a folder, and deletes the files whose name has a date and the date is older than the number of days provided
        /// </summary>
        /// <param name="days"></param>
        /// <param name="file"></param>
        private static void CleanUpSingleLogFile(int days, string file)
        {
            if (Directory.Exists(file))
            {
                foreach (var currentFile in Directory.EnumerateFiles(file))
                {
                    var currentFileWithoutDirectory = currentFile.Replace(file, "");
                    string dateString = currentFileWithoutDirectory.Replace("Log ", "").Replace(".txt", "");
                    try
                    {
                        DateTime date = DateTime.ParseExact(dateString, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                        var totalDays = (DateTime.Now - date).TotalDays;
                        if (totalDays > days)
                        {
                            try
                            {
                                File.Delete(currentFile);
                            }
                            catch (Exception ex)
                            {
                                Error("LOGGER", "CleanUpSingleLogFile - Failed Deleting File " + currentFile + " Error: " + ex);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Error("LOGGER", "CleanUpSingleLogFile - Failed Parsing Date For File " + currentFile + " Date: " + dateString + " Error: " + ex);
                    }
                }
            }
            else Directory.CreateDirectory(file);
        }

        /// <summary>
        /// Appends a text to a specific type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="tag"></param>
        /// <param name="msg"></param>
        public static void Append(string type, string tag, string msg, bool writeToConsole = true)
        {
            var folder = CurrentServicePath + "/Logs/" + type + "/";
            if (writeToConsole)
                Console.WriteLine(tag + ": " + msg);

            if (!isInitialized)
            {
                Console.WriteLine("LOGGER: Logger Not Initialized Yet");
                return;
            }

            if (!IsLoggerTypeValid(type))
            {
                Error("LOGGER", "Attempting To Log Data For Type: " + type + " While Logger Type Is Not Valid!");
                return;
            }

            var date = DateTime.Now;

            string fileName = folder + "Log " + date.ToString("dd-MM-yyyy") + ".txt";
            string data = "[" + tag + "] " + date.ToString("HH:mm:ss.fff") + ": " + msg;

            try
            {
                if (!File.Exists(fileName))
                    File.Create(fileName).Dispose();

                File.AppendAllLines(fileName, new List<string>() { data });

            }
            catch (Exception ex)
            {
                Error("LOGGER", "Failed Creating Output Stream For File: " + fileName + " Error: " + ex.Message);
            }


        }

        /// <summary>
        /// Sends a debug log which is saved to the current day's log file in the /Debug/ Folder
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="msg"></param>
        public static void Debug(string tag, string msg)
        {
            Append("Debug", tag, msg);
        }

        /// <summary>
        /// Sends an error log which is saved to the current day's log file in the /Error/ Folder
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="msg"></param>
        public static void Error(string tag, string msg)
        {
            Append("Error", tag, msg);
        }

        /// <summary>
        /// Sends a log which is saved to the current day's log file in the /@type/ Folder
        /// </summary>
        /// <param name="type"></param>
        /// <param name="tag"></param>
        /// <param name="msg"></param>
        public static void Log(string type, string tag, string msg)
        {
            Append(type, tag, msg);
        }

        /// <summary>
        /// Adds A Logger Type Such As Debug, Error, Etc...
        /// </summary>
        /// <param name="type"></param>
        public static void AddLoggerType(string type)
        {
            if (LoggerFileTypes.Contains(type))
                return;
            LoggerFileTypes.Add(type);
        }

        /// <summary>
        /// Checks If A Logger Type Is Valid
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsLoggerTypeValid(string type)
        {
            return LoggerFileTypes.Contains(type);
        }

    }
}
