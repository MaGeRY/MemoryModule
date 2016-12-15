using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    public class CommandLine
    {
        private static bool Initialized = false;        

        public static Dictionary<string, string> Switches { get; private set; }
        public static string ExecutableFilename { get; private set; }
        public static string WorkingDirectory { get; private set; }        
        public static string[] Arguments { get; private set; }

        public CommandLine()
        {
            Initialize();
        }

        #region [private] Initialize(bool force = false, string commandString = null)
        private static void Initialize(bool force = false, string commandString = null)
        {
            if (!Initialized || force)
            {
                WorkingDirectory = Environment.CurrentDirectory;
                WorkingDirectory += Path.DirectorySeparatorChar;

                Process process = Process.GetCurrentProcess();
                ExecutableFilename = process.MainModule.FileName;

                if (commandString != null)
                {
                    Arguments = commandString.Split();
                }
                else
                {                    
                    Arguments = Environment.GetCommandLineArgs();                    
                }

                Switches = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);                                                

                if (Arguments != null && Arguments.Length > 0)
                {
                    // Remove executable first argument //
                    Arguments = Arguments.RemoveAt(0);

                    string switchKey = string.Empty;
                    foreach (string value in Arguments)
                    {
                        if (value.Length != 0)
                        {
                            if (value[0] == '/' || value[0] == '-' || value[0] == '+')
                            {
                                if (switchKey != "" && !Switches.ContainsKey(switchKey))
                                {
                                    Switches.Add(switchKey, "");
                                }

                                if (value.IndexOf("=") > 0)
                                {
                                    string[] keyValue = value.Substring(1).Split('=');
                                    if (keyValue.Length > 1)
                                    {
                                        Switches.Add(keyValue[0].ToLower(), keyValue[1]);
                                    }
                                    continue;
                                }

                                switchKey = value;
                            }
                            else if (switchKey != "")
                            {
                                if (!Switches.ContainsKey(switchKey))
                                {
                                    Switches.Add(switchKey, value);
                                }
                                switchKey = "";
                            }
                        }
                    }

                    if (switchKey != "" && !Switches.ContainsKey(switchKey))
                    {
                        Switches.Add(switchKey, "");
                    }
                }

                Initialized = true;
            }
        }
        #endregion

        #region [public] Force(string commandString) 
        public static void Force(string commandString)
        {
            CommandLine.Initialize(true, commandString);
        }
        #endregion

        #region [public] HasSwitch(string name)
        public static bool HasSwitch(string name)
        {
            Initialize();            
            return Switches.ContainsKey(name);
        }
        #endregion

        #region [public] GetSwitch(string strName, string strDefault = "")
        public static string GetSwitch(string strName, string strDefault = "")
        {
            Initialize();
            string result = string.Empty;
            if (Switches.TryGetValue(strName, out result))
            {
                return result;
            }
            return strDefault;
        }
        #endregion

        #region [public] GetSwitchInt(string strName, int iDefault = 0)
        public static int GetSwitchInt(string strName, int iDefault = 0)
        {
            Initialize();
            int iResult = iDefault;
            string result = string.Empty;
            if (Switches.TryGetValue(strName, out result))
            {
                if (int.TryParse(result, out iResult))
                {
                    return iResult;
                }
            }            
            return iDefault;
        }
        #endregion

        #region [public] GetSwitchInt64(string strName, long iDefault = 0)
        public static long GetSwitchInt64(string strName, long iDefault = 0)
        {
            Initialize();
            long iResult = iDefault;
            string result = string.Empty;
            if (Switches.TryGetValue(strName, out result))
            {
                if (long.TryParse(result, out iResult))
                {
                    return iResult;
                }
            }
            return iDefault;
        }
        #endregion

        #region [public] GetSwitchFloat(string strName, float iDefault = 0)
        public static float GetSwitchFloat(string strName, float iDefault = 0)
        {
            Initialize();
            float iResult = iDefault;
            string result = string.Empty;
            if (Switches.TryGetValue(strName, out result))
            {
                if (float.TryParse(result, out iResult))
                {
                    return iResult;
                }
            }
            return iDefault;
        }
        #endregion
    }
}
