using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace System
{
    public class ConsoleWindow
    {
        internal class ConsoleWriter : StreamWriter
        {
            private static string[] separators = new string[] { "\n", "\r\n" };

            public ConsoleWriter(Stream stream, Encoding encoding) : base (stream, encoding, 0x400)
            {
                //
            }

            public override void Write(string value)
            {
                if (value.IndexOf("\n") > -1)
                {                    
                    foreach (string text in value.Split(separators, StringSplitOptions.None))
                    {
                        base.Write(text.PadRight(Console.BufferWidth));
                    }
                    InputRedraw();
                }
                else
                {
                    base.Write(value);
                }
            }

            public override void WriteLine(string value)
            {               
                base.WriteLine(value);
                InputRedraw();
            }

            public override void WriteLine()
            {
                if (Console.CursorLeft > 0)
                {
                    base.WriteLine();
                    InputRedraw();
                }
            }
        }

        /*
        public class CursorPosition
        {
            public int Top;
            public int Left;

            public CursorPosition()
            {                
                this.Left = 0;
                this.Top = 0;
            }
            public void SetCursorPosition()
            {
                Console.CursorLeft = this.Left;
                Console.CursorTop = this.Top;
            }
            public void GetCursorPosition()
            {
                this.Left = Console.CursorLeft;
                this.Top = Console.CursorTop;
            }
        }
        */

        public class StatusText
        {
            private string leftText;
            private string rightText;
            public ConsoleColor LeftColor;
            public ConsoleColor RightColor;

            public bool IsEmpty => string.IsNullOrEmpty(leftText) && string.IsNullOrEmpty(rightText);

            public string LeftText
            {
                get
                {
                    return leftText.Substring(0, Math.Min(leftText.Length, LineWidth - rightText.Length));
                }
                set
                {
                    leftText = value;
                }
            }

            public string RightText
            {
                get
                {
                    int emptySpace = LineWidth - (leftText.Length + rightText.Length);
                    if (emptySpace < 0) emptySpace = 0;
                    return rightText.PadLeft(rightText.Length + emptySpace);
                }
                set
                {
                    rightText = value;
                }
            }

            public StatusText(string leftText = "", ConsoleColor leftColor = ConsoleColor.White, string rightText = "", ConsoleColor rightColor = ConsoleColor.White)
            {
                this.leftText = leftText.Substring(0, Math.Min(leftText.Length, LineWidth - 1));
                this.rightText = rightText.Substring(0, Math.Min(leftText.Length, LineWidth - 1));
                this.LeftColor = leftColor;                
                this.RightColor = rightColor;
            }            
        }

        // ConsoleWindow : Output //
        private const int STD_INPUT_HANDLE  = -10;
        private const int STD_OUTPUT_HANDLE = -11;

        #region Native Import from Kernel32.dll
        private class Native
        {
            private const string KERNEL32 = "KERNEL32.DLL";

            [DllImport(KERNEL32, CharSet = CharSet.Ansi, ExactSpelling = false, SetLastError = true)]
            public static extern bool AllocConsole();

            [DllImport(KERNEL32, CharSet = CharSet.Ansi, ExactSpelling = false, SetLastError = true)]
            public static extern bool AttachConsole(uint dwProcessId);

            [DllImport(KERNEL32, CharSet = CharSet.Ansi, ExactSpelling = false, SetLastError = true)]
            public static extern bool FreeConsole();

            [DllImport(KERNEL32)]
            public static extern IntPtr GetConsoleWindow();

            [DllImport(KERNEL32)]
            public static extern bool SetConsoleOutputCP(uint wCodePageID);

            [DllImport(KERNEL32)]
            public static extern bool SetConsoleTitle(string lpConsoleTitle);

            [DllImport(KERNEL32, CharSet = CharSet.Auto)]
            public static extern IntPtr GetModuleHandle(string lpModuleName);

            [DllImport(KERNEL32, EntryPoint = "GetStdHandle", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern IntPtr GetStdHandle(int nStdHandle);
        }
        #endregion

        private static object lockObject = new object();

        private static TextWriter OriginalOutput;
        private static Encoding OriginalEncoding;
        private static ConsoleColor DefaultColor;
        
        private static float nextUpdate;

        private static string LogText;
        private static string LogFile;
        private static bool TimeStamp;
        private static bool DateStamp;
        
        public static int LineWidth => Console.BufferWidth;
        public static bool IsValid => Console.BufferWidth > 0;

        public static string[] ignoreMessages;
       
        // ConsoleWindow : Status //        
        public static StatusText[] consoleStatus;

        // ConsoleWindow : Input //        
        public static string InputString = string.Empty;
        public static event Action<string> OnInputText;
        private static List<string> inputHistory;
        private static int inputHistoryIndex;
        public static int inputCursorLeft;

        // Console Log //

        #region [public] LogFilename
        public static string LogFilename
        {
            get
            {
                if (!string.IsNullOrEmpty(LogFile))
                {
                    if (!DateStamp && LogFile.Contains("{0}"))
                    {
                        return string.Format(LogFile, DateTime.Now.ToString("dd-MM-yyyy"));
                    }
                }
                return LogFile;
            }
        }
        #endregion

        #region [private] GetTimeStamp
        private static string GetTimeStamp
        {
            get
            {
                string result = string.Empty;
                if (DateStamp) result += DateTime.Now.ToString("dd-MM-yyyy");
                if (TimeStamp) result += " "+DateTime.Now.ToString("HH:mm:ss");
                return result.Trim();
            }
        }
        #endregion        

        #region [private] WriteToLogFile()
        private static void WriteToLogFile(bool newLine = false)
        {
            if (!string.IsNullOrEmpty(LogFilename) && LogText != null)
            {
                lock (lockObject)
                {
                    if (newLine)
                    {
                        LogText += Environment.NewLine;
                    }

                    if (TimeStamp && LogText != Environment.NewLine)
                    {
                        LogText = "[" + GetTimeStamp + "] " + LogText;
                    }

                    System.IO.File.AppendAllText(LogFilename, LogText);
                    LogText = string.Empty;
                }
            }
        }
        #endregion

        // Console Output //

        #region[public] Initialize()
        public static void Initialize(string logfile = null, bool logReset = true, bool timeStamp = true, bool dateStamp = false)
        {
            if (!Native.AttachConsole(uint.MaxValue))
            {
                Native.AllocConsole();
            }

            OriginalOutput = Console.Out;
            OriginalEncoding = Console.OutputEncoding;

            Native.SetConsoleOutputCP((uint)Encoding.UTF8.CodePage);
            Console.OutputEncoding = Encoding.UTF8;            

            Stream outputStream;
            try
            {
                SafeFileHandle fileHandle = new SafeFileHandle(Native.GetStdHandle(STD_OUTPUT_HANDLE), true);
                outputStream = new FileStream(fileHandle, FileAccess.Write);
            }
            catch (Exception)
            {
                outputStream = Console.OpenStandardOutput();
            }

            Console.SetOut(new ConsoleWriter(outputStream, Console.OutputEncoding) { AutoFlush = true });
            
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("Initialized.");
            Console.ResetColor();
            Console.Clear();

            DefaultColor = ConsoleColor.Gray;

            if (!string.IsNullOrEmpty(logfile))
            {
                TimeStamp = timeStamp;
                DateStamp = dateStamp;
                LogFile = Path.GetFullPath(logfile);
                string directoryName = Path.GetDirectoryName(LogFile);                

                if (!DateStamp)
                {
                    string LogName = Path.GetFileNameWithoutExtension(LogFile) + ".{0}" + Path.GetExtension(LogFile);
                    LogFile = Path.Combine(directoryName, LogName);
                }

                if (!System.IO.Directory.Exists(directoryName))
                {
                    System.IO.Directory.CreateDirectory(directoryName);
                }

                if (logReset && System.IO.File.Exists(LogFile))
                {
                    System.IO.File.Delete(LogFile);
                }
            }                       

            consoleStatus = new StatusText[0];
            inputHistory = new List<string>();
            inputHistoryIndex = -1;
        }
        #endregion

        #region[public] Shutdown()
        public static void Shutdown()
        {
            if (OriginalOutput != null)
            {
                Console.SetOut(OriginalOutput);
            }

            if (OriginalEncoding != null)
            {
                Native.SetConsoleOutputCP((uint)OriginalEncoding.CodePage);
                Console.OutputEncoding = OriginalEncoding;
            }

            Native.FreeConsole();

            LogText = null;
            LogFile = null;            
        }
        #endregion

        #region [public] SetTitle(string name)
        public static void SetTitle(string name)
        {
            Native.SetConsoleTitle(name);
        }
        #endregion

        #region [public] SetStatusText(int line, string leftText = "", string rightText = "")
        public static void SetStatusText(int line, string leftText = "", string rightText = "")
        {
            if (line > consoleStatus.Length - 1)
            {
                if (!string.IsNullOrEmpty(leftText) || !string.IsNullOrEmpty(rightText))
                {
                    Array.Resize(ref consoleStatus, line + 1);
                    for (int i = 0; i < consoleStatus.Length; i++)
                    {
                        if (consoleStatus[i] == null)
                        {
                            consoleStatus[i] = new StatusText();
                        }
                    }
                }
            }
            else
            {
                if (string.IsNullOrEmpty(leftText) && string.IsNullOrEmpty(rightText))
                {
                    consoleStatus = consoleStatus.RemoveAt(line);
                    return;
                }
            }            
            consoleStatus[line].RightText = rightText;
            consoleStatus[line].LeftText = leftText;
        }
        #endregion

        #region [public] SetStatusLeft(int line, string text = "", ConsoleColor color = ConsoleColor.White)
        public static void SetStatusLeft(int line, string text = "", ConsoleColor color = ConsoleColor.White)
        {
            if (line > consoleStatus.Length - 1)
            {
                if (!string.IsNullOrEmpty(text))
                {
                    Array.Resize(ref consoleStatus, line + 1);
                    for (int i = 0; i < consoleStatus.Length; i++)
                    {
                        if (consoleStatus[i] == null)
                        {
                            consoleStatus[i] = new StatusText();
                        }
                    }
                }
            }
            else if (string.IsNullOrEmpty(text) && string.IsNullOrEmpty(consoleStatus[line].RightText))
            {
                consoleStatus = consoleStatus.RemoveAt(line);
                return;
            }
            consoleStatus[line].LeftColor = color;
            consoleStatus[line].LeftText = text;
        }
        #endregion

        #region [public] SetStatusRight(int line, string text = "", ConsoleColor color = ConsoleColor.White)
        public static void SetStatusRight(int line, string text = "", ConsoleColor color = ConsoleColor.White)
        {
            if (line > consoleStatus.Length - 1)
            {
                if (!string.IsNullOrEmpty(text))
                {
                    Array.Resize(ref consoleStatus, line + 1);
                    for (int i = 0; i < consoleStatus.Length; i++)
                    {
                        if (consoleStatus[i] == null)
                        {
                            consoleStatus[i] = new StatusText();
                        }
                    }
                }
            }
            else if (string.IsNullOrEmpty(text) && string.IsNullOrEmpty(consoleStatus[line].LeftText))
            {
                consoleStatus = consoleStatus.RemoveAt(line);
                return;
            }
            consoleStatus[line].RightColor = color;
            consoleStatus[line].RightText = text;
        }
        #endregion

        #region [public] Write(objects)
        public static void Write(params object[] objects)
        {
            if (objects.Length > 0)
            {
                Console.ForegroundColor = DefaultColor;

                if (Console.CursorLeft == 0)
                {
                    ClearLine();
                }

                foreach (object obj in objects)
                {
                    if (obj.GetType() == typeof(ConsoleColor))
                    {
                        Console.ForegroundColor = (ConsoleColor)obj;
                    }
                    else
                    {
                        string text = obj.ToString();
                        Console.Write(text);
                        LogText += text;
                    }
                }                

                Console.ForegroundColor = DefaultColor;

                if (LogText.Length > 0)
                {
                    WriteToLogFile(false);
                }
            }
        }
        #endregion

        #region [public] WriteLine(objects)
        public static void WriteLine(params object[] objects)
        {
            if (objects.Length > 0)
            {
                Console.ForegroundColor = DefaultColor;

                if (Console.CursorLeft == 0)
                {
                    ClearLine();
                }

                foreach (object obj in objects)
                {
                    if (obj.GetType() == typeof(ConsoleColor))
                    {
                        Console.ForegroundColor = (ConsoleColor)obj;
                    }
                    else
                    {
                        string text = obj.ToString();
                        Console.Write(text);
                        LogText += text;
                    }
                }

                Console.ForegroundColor = DefaultColor;

                if (LogText.Length > 0)
                {
                    Console.WriteLine();
                    WriteToLogFile(true);
                }                
            }
            else
            {
                Console.Write(Environment.NewLine);
            }
        }
        #endregion

        #region [public] ClearLine(int lines = 1)
        public static void ClearLine(int lines = 1)
        {
            Console.CursorLeft = 0;
            Console.Write(new string(' ', lines * LineWidth));
            Console.CursorTop -= lines;
        }
        #endregion

        #region [public] Pause(params object[] objects)
        public static void Pause(params object[] objects)
        {
            // Output text of pause //
            if (objects.Length > 0)
            {
                foreach (object obj in objects)
                {
                    if (obj.GetType() == typeof(ConsoleColor))
                    {
                        Console.ForegroundColor = (ConsoleColor)obj;
                    }
                    else
                    {
                        Console.Write(obj.ToString());
                    }
                }
                Console.WriteLine();
            }

            // Waiting on pressing ENTER to continue //
            while (!Console.KeyAvailable || Console.ReadKey(false).Key != ConsoleKey.Enter)
            {
                Threading.Thread.Sleep(5);
            }
        }
        #endregion

        // Console Update //

        #region [public] Update()
        public static void Update()
        {
            if (ConsoleWindow.IsValid)
            {
                // Output Console Input //                
                if (OnInputText != null)
                {
                    ConsoleWindow.InputUpdate();
                }

                if (nextUpdate < UnityEngine.Time.time)
                {
                    ConsoleWindow.InputRedraw();
                }
            }            
        }
        #endregion

        // Console Input //

        #region [private] InputRedraw()
        private static void InputRedraw(bool inputOnly = false)
        {
            try
            {               
                if (!inputOnly)
                {
                    nextUpdate = UnityEngine.Time.time + 0.66f;

                    int LineCount = OnInputText != null ? 2 : 1;

                    Console.CursorLeft = 0;
                    Console.Write(new string(' ', LineWidth * LineCount));

                    // Output Status Text //
                    for (int i = 0; i < consoleStatus.Length; i++)
                    {
                        if (!consoleStatus[i].IsEmpty)
                        {
                            Console.ForegroundColor = consoleStatus[i].LeftColor;
                            Console.Write(consoleStatus[i].LeftText);
                            Console.ForegroundColor = consoleStatus[i].RightColor;
                            Console.Write(consoleStatus[i].RightText);
                            LineCount++;
                        }
                    }

                    Console.CursorTop = Console.CursorTop - LineCount;                    
                }

                // Output console input //                
                if (OnInputText != null)
                {
                    Console.CursorLeft = 0;

                    // Set foreground color for input text //
                    Console.ForegroundColor = ConsoleColor.Green;

                    if (InputString.Length != 0)
                    {
                        int maxInputWidth = (LineWidth - 2);
                        if (InputString.Length < maxInputWidth)
                        {
                            Console.Write(InputString.PadRight(maxInputWidth));
                            Console.CursorLeft = inputCursorLeft;
                        }
                        else
                        {
                            Console.Write(InputString.Substring(InputString.Length - maxInputWidth));
                            Console.CursorLeft = maxInputWidth;
                        }
                    }
                }

                Console.ForegroundColor = ConsoleColor.Gray;
            }
            catch (Exception)
            {
                /* Ignore exceptions */
            }
        }
        #endregion

        #region [private] InputUpdate()
        private static void InputUpdate()
        {
            if (Console.KeyAvailable)
            {                
                ConsoleKeyInfo info = Console.ReadKey();

                switch (info.Key)
                {
                    case ConsoleKey.Enter:
                        OnEnter();
                    return;

                    case ConsoleKey.Escape:
                        OnEscape();
                    return;

                    case ConsoleKey.UpArrow:
                        OnHistoryUp();
                    return;

                    case ConsoleKey.DownArrow:
                        OnHistoryDown();
                    return;

                    case ConsoleKey.LeftArrow:
                        OnCursorLeft();
                    return;

                    case ConsoleKey.RightArrow:
                        OnCursorRight();
                    return;

                    case ConsoleKey.Home:
                        OnCursorStart();
                    return;

                    case ConsoleKey.End:
                        OnCursorEnd();
                    return;

                    case ConsoleKey.Backspace:
                        OnBackspace();
                    return;

                    case ConsoleKey.Delete:
                        OnDelete();
                    return;

                    default:
                        if (info.KeyChar != '\0')
                        {
                            if (inputCursorLeft < InputString.Length)
                            {
                                InputString = InputString.Substring(0, inputCursorLeft) + info.KeyChar + InputString.Substring(inputCursorLeft);
                            }
                            else
                            {
                                InputString += info.KeyChar;
                            }

                            inputCursorLeft++;
                            InputRedraw(true);
                        }
                    return;
                }
            }
        }
        #endregion

        #region [private] OnEnter() 
        private static void OnEnter()
        {
            ConsoleWindow.ClearLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("> " + InputString);

            inputHistory.Add(InputString);
            inputHistoryIndex = inputHistory.Count;

            string inputString = InputString;
            InputString = string.Empty;
            inputCursorLeft = 0;

            InputRedraw();

            OnInputText?.Invoke(inputString);
        }
        #endregion

        #region [private] OnEscape()  
        private static void OnEscape()
        {
            InputString = string.Empty;
            InputRedraw(true);
        }
        #endregion

        #region [private] OnBackspace() 
        private static void OnBackspace()
        {
            if (InputString.Length > 0 && inputCursorLeft > 0)
            {
                if (inputCursorLeft < InputString.Length)
                {
                    InputString = InputString.Remove(inputCursorLeft - 1, 1);
                }
                else
                {
                    InputString = InputString.Substring(0, InputString.Length - 1);
                }
                inputCursorLeft--;
                InputRedraw(true);
            }
        }
        #endregion

        #region [private] OnDelete() 
        private static void OnDelete()
        {
            if (InputString.Length > inputCursorLeft)
            {
                InputString = InputString.Remove(inputCursorLeft, 1);
                InputRedraw(true);
            }
        }
        #endregion

        #region [private] OnHistoryUp()
        private static void OnHistoryUp()
        {
            if (inputHistory.Count > 0)
            {
                if (inputHistoryIndex > 0)
                {
                    inputHistoryIndex--;
                }
                InputString = inputHistory[inputHistoryIndex];
                inputCursorLeft = InputString.Length;
                InputRedraw(true);
            }
        }
        #endregion

        #region [private] OnHistoryDown() 
        private static void OnHistoryDown()
        {
            if (inputHistory.Count > 0)
            {
                if (inputHistoryIndex < inputHistory.Count - 1)
                {
                    inputHistoryIndex++;
                }
                InputString = inputHistory[inputHistoryIndex];
                inputCursorLeft = InputString.Length;
                InputRedraw(true);
            }
        }
        #endregion

        #region [private] OnCursorLeft()
        private static void OnCursorLeft()
        {
            if (inputCursorLeft > 0) inputCursorLeft--;
            Console.CursorLeft = inputCursorLeft;
        }
        #endregion

        #region [private] OnCursorRight()
        private static void OnCursorRight()
        {
            if (inputCursorLeft < InputString.Length)
            {
                inputCursorLeft++;
            }
            Console.CursorLeft = inputCursorLeft;
        }
        #endregion

        #region [private] OnCursorStart()
        private static void OnCursorStart()
        {
            Console.CursorLeft = 0;
            inputCursorLeft = 0;
        }
        #endregion

        #region [private] OnCursorEnd()
        private static void OnCursorEnd()
        {
            inputCursorLeft = InputString.Length;
            Console.CursorLeft = inputCursorLeft;
        }
        #endregion
    }
}
