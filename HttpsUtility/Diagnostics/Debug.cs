/*
 * Copyright (c) Troy Garner 2019 - Present
 *
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
 * OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
 * IN THE SOFTWARE.
 *
*/

using System;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.Reflection;

namespace HttpsUtility.Diagnostics
{
    internal static class Debug
    {
        private static bool _enable;
        private static readonly CTimer _timer = new CTimer(WriteQueueToDisk, null, Timeout.Infinite, Timeout.Infinite);
        private static readonly CCriticalSection _writeLock = new CCriticalSection();
        private static readonly CrestronQueue<string> _queue = new CrestronQueue<string>(1024);

        private static readonly AssemblyName _asmName;

        /// <summary>Initializes the debug class</summary>
        static Debug()
        {
            try
            {
                _asmName = Assembly.GetExecutingAssembly().GetName();
            }
            catch (Exception ex)
            {
                CrestronConsole.PrintLine(ex.Message);
                throw;
            }
        }

        public static DebugLevel Levels { get; set; }

        /// <summary>Flag for enabling debug mode</summary>
        public static bool Enable
        {
            get { return _enable; }
            set
            {
                _enable = value;
                if (_enable)
                    _timer.Reset(0, 5000);
                else
                    _timer.Stop();
            }
        }

        private static void WriteQueueToDisk(object _)
        {
            if (Enable)
            {
                try
                {
                    if (!_writeLock.TryEnter())
                        return;

                    if (!_queue.IsEmpty)
                    {
                        var sb = new StringBuilder(8192);
                        while (!_queue.IsEmpty)
                        {
                            string logItem;
                            if (_queue.Dequeue(out logItem))
                                sb.Append(logItem);
                        }

                        using (var sw = new StreamWriter(string.Format("\\User\\Logs\\{0} {1:yyyy-MM-dd}.log", _asmName.Name, DateTime.Now), true))
                            sw.Write(sb.ToString());
                    }
                }
                finally
                {
                    _writeLock.Leave();
                }
            }
        }

        /// <summary>
        /// Private helper method to return formatted message string prefixed with the assembly identifier.
        /// </summary>
        /// <param name="message">Message to be written</param>
        /// <param name="args">Format arguments for the message</param>
        private static string InternalFormat(string message, params object[] args)
        {
            return string.Format("[{0:yyyy-MM-dd HH:mm:ss}] {1}: App{2:00} - {3}", DateTime.Now, string.Format("{0} {1}", _asmName.Name, _asmName.Version),
                InitialParametersClass.ApplicationNumber, string.Format(message, args));
        }

        /// <summary>
        /// Private helper method to write formatted message to console prefixed with the assembly identifier.
        /// </summary>
        /// <param name="message">Message to be written</param>
        /// <param name="args">Format arguments for the message</param>
        private static void InternalWrite(string message, params object[] args)
        {
            if (Enable)
            {
                try
                {
                    _writeLock.Enter();
                    CrestronInvoke.BeginInvoke(_ => _queue.Enqueue(InternalFormat(message, args)));
                }
                finally
                {
                    _writeLock.Leave();
                }
            }
        }

        /// <summary>
        /// Writes a raw debug message without a newline.
        /// </summary>
        /// <param name="message">Format message to output</param>
        /// <param name="args">Format message string arguments (if applicable)</param>
        public static void Write(string message, params object[] args)
        {
            InternalWrite(message, args);
        }

        public static void Write(object obj)
        {
            Write("{0}", obj);
        }

        /// <summary>
        /// Writes a raw debug message with a newline.
        /// </summary>
        /// <param name="message">Format message to output</param>
        /// <param name="args">Format message string arguments (if applicable)</param>
        public static void WriteLine(string message, params object[] args)
        {
            Write("{0}\n", string.Format(message, args));
        }

        public static void WriteLine(object obj)
        {
            WriteLine("{0}", obj);
        }

        /// <summary>
        /// Writes an exception debug message with a newline.
        /// </summary>
        /// <param name="message">Format message to output</param>
        /// <param name="args">Format message string arguments (if applicable)</param>
        public static void WriteException(string message, params object[] args)
        {
            if (((int)Levels & (int)DebugLevel.Exception) != 0)
                WriteLine("NOTICE: [Exception] {0}", string.Format(message, args));
        }

        public static void WriteException(Exception obj)
        {
            WriteException("{0}", obj);
        }

        /// <summary>
        /// Writes an error debug message with a newline.
        /// </summary>
        /// <param name="message">Format message to output</param>
        /// <param name="args">Format message string arguments (if applicable)</param>
        public static void WriteError(string message, params object[] args)
        {
            if (((int)Levels & (int)DebugLevel.Error) != 0)
                WriteLine("Error: {0}", string.Format(message, args));
        }

        public static void WriteError(object obj)
        {
            WriteError("{0}", obj);
        }

        /// <summary>
        /// Writes a warning debug message with a newline.
        /// </summary>
        /// <param name="message">Format message to output</param>
        /// <param name="args">Format message string arguments (if applicable)</param>
        public static void WriteWarning(string message, params object[] args)
        {
            if (((int)Levels & (int)DebugLevel.Warning) != 0)
                WriteLine("Warning: {0}", string.Format(message, args));
        }

        public static void WriteWarning(object obj)
        {
            WriteWarning("{0}", obj);
        }

        /// <summary>
        /// Writes an info debug message with a newline.
        /// </summary>
        /// <param name="message">Format message to output</param>
        /// <param name="args">Format message string arguments (if applicable)</param>
        public static void WriteInfo(string message, params object[] args)
        {
            if (((int)Levels & (int)DebugLevel.Info) != 0)
                WriteLine("Info: {0}", string.Format(message, args));
        }

        public static void WriteInfo(object obj)
        {
            WriteInfo("{0}", obj);
        }

        public static void Assert(Func<bool> condition)
        {
            Assert(condition, null);
        }
        
        public static void Assert(Func<bool> condition, string message)
        {
            if (!condition.Invoke())
                throw message != null ? new AssertionException(message) : new AssertionException();
        }
    }
}