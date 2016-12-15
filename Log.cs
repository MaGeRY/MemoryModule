using System;

namespace System
{
    public class Log
    {
        public enum LogType
        {
            Default,
            Warning,
            Assert,
            Error,
            Exception
        }        

        private static object lockObject = new object();

        public static string LogDirectory;

        public static int LineWidth => System.Console.BufferWidth;

        #region [constructor] Log()
        static Log()
        {
            LogDirectory = CommandLine.WorkingDirectory + "/Logs";

            if (CommandLine.HasSwitch("-logfile"))
            {
                LogDirectory = System.IO.Path.GetDirectoryName(CommandLine.GetSwitch("-logfile", LogDirectory));
                LogDirectory = LogDirectory.Replace("\\", "/").TrimEnd('/');

                if (string.IsNullOrEmpty(LogDirectory))
                {
                    LogDirectory = CommandLine.WorkingDirectory.TrimEnd('/');
                }
            }
        }
        #endregion

        #region [private] WriteToFile(string strFilename, string strMessage)
        private static void WriteToFile(string strFilename, string strMessage)
        {            
            lock (lockObject)
            {                
                System.IO.File.AppendAllText(string.Format("{0}/{1}", LogDirectory, strFilename), string.Format("[{0}] {1}\r\n", DateTime.Now.ToString(), strMessage));
            }
        }
        #endregion

        #region [private] ClearLine(int count = 1)
        private static void ClearLine(int count = 1)
        {
            System.Console.CursorLeft = 0;
            System.Console.Write(new string(' ', LineWidth * count));
            System.Console.CursorTop -= count;
            System.Console.CursorLeft = 0;
        }
        #endregion

        #region [private] Print(LogType type, params object[] objects)
        private static void Print(LogType type, params object[] objects)
        {
            if (objects.Length > 0)
            {
                ClearLine();
                string message = string.Empty;
                ConsoleColor ForegroundColor = System.Console.ForegroundColor;

                switch (type)
                {
                    case LogType.Default:
                        System.Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                    case LogType.Warning:
                        System.Console.ForegroundColor = ConsoleColor.Yellow;
                        System.Console.Write("[WARNING] ");
                        message += "[WARNING] ";
                    break;
                    case LogType.Error:
                        System.Console.ForegroundColor = ConsoleColor.Red;
                        System.Console.Write("[ERROR] ");
                        message += "[ERROR] ";
                    break;
                    case LogType.Exception:
                        System.Console.ForegroundColor = ConsoleColor.Red;
                        System.Console.Write("[EXCEPTION] ");
                        message += "[EXCEPTION] ";
                    break;
                }

                foreach (object obj in objects)
                {
                    if (obj != null)
                    {
                        if (obj is ConsoleColor)
                        {
                            System.Console.ForegroundColor = (ConsoleColor)obj;
                        }
                        else if (obj.GetType() != typeof(LogType))
                        {
                            System.Console.Write(obj.ToString());
                            message += obj.ToString();
                        }
                    }
                }

                System.Console.WriteLine();
                System.Console.ForegroundColor = ForegroundColor;

                if (!string.IsNullOrEmpty(LogDirectory))
                {
                    if (!System.IO.Directory.Exists(LogDirectory))
                    {
                        System.IO.Directory.CreateDirectory(LogDirectory);
                    }

                    switch (type)
                    {
                        case LogType.Warning:
                            WriteToFile("Log.Warning.txt", message);
                        break;
                        case LogType.Assert:
                            WriteToFile("Log.Assert.txt", message);
                        break;
                        case LogType.Error:
                            WriteToFile("Log.Error.txt", message);
                        break;
                        case LogType.Exception:
                            WriteToFile("Log.Exception.txt", message);
                        break;
                        default:
                            WriteToFile("Log.Console.txt", message);
                        break;
                    }
                }               
            }
        }
        #endregion

        public static void Write(params object[] objects)
        {
            string message = string.Empty;

            foreach (object obj in objects)
            {
                if (obj != null)
                {
                    if (obj is LogType || obj is ConsoleColor)
                    {
                        continue;
                    }                    
                    message += obj.ToString();
                }
            }

            WriteToFile("Log.Console.txt", message);
        }

        public static void Console(params object[] objects)
        {
            Print(LogType.Default, objects);
        }

        public static void Warning(params object[] objects)
        {
            Print(LogType.Warning, objects);
        }

        public static void Assert(params object[] objects)
        {
            Print(LogType.Assert, objects);
        }

        public static void Error(params object[] objects)
        {
            Print(LogType.Error, objects);
        }

        public static void Exception(params object[] objects)
        {
            Print(LogType.Exception, objects);
        }

        public static void Exception(Exception exception)
        {
            Print(LogType.Exception, exception);
        }
    }
}
