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
using System.Diagnostics;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronLogger;
using HttpsUtility.Diagnostics.Formatters;
using HttpsUtility.Threading;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace HttpsUtility.Diagnostics
{
    /// <summary>
    /// Class for all diagnostic debugging utilities where most debug methods are conditionally
    /// enabled by the DEBUG compiler directive. (Any fatal or more severe indicator methods are
    /// not disabled for RELEASE builds such as exception messages.)
    /// </summary>
    internal static class Debug
    {
        private static readonly SyncSection _debugFormatterLock = new SyncSection();
        private static IDiagnosticsFormatter _formatter = new AssemblyFormatter();
        private static DebugLevelFlags _debugLevel = DebugLevelFlags.Warning | DebugLevelFlags.Error | DebugLevelFlags.Exception;

        /// <summary>Debug flag to set in the project settings for enabling debug output</summary>
        public const string DEBUG_FLAG_DIRECTIVE = "DEBUG";

        /// <summary>Debug level flags for console output</summary>
        public static DebugLevelFlags DebugLevel { get { return _debugLevel; } set { _debugLevel = value; } }

        /// <summary>Flag for enabling debug trace messages</summary>
        public static bool EnableTrace { get; set; }

        static Debug()
        {
            CrestronLogger.Initialize(1, false, LoggerModeEnum.DEFAULT);
        }

        /// <summary>
        /// Set custom formatter for all debug messages.
        /// </summary>
        /// <param name="formatter">Diagnostics formatter.</param>
        [Conditional(DEBUG_FLAG_DIRECTIVE)]
        public static void SetFormatter(IDiagnosticsFormatter formatter)
        {
            if (formatter == null)
                throw new ArgumentNullException("formatter");

            using (_debugFormatterLock.AquireLock())
                _formatter = formatter;
        }

        /// <summary>
        /// Write debug build indicator.
        /// </summary>
        [Conditional(DEBUG_FLAG_DIRECTIVE)]
        public static void WriteDebugIndicator(Type classType)
        {
            WriteInfo(classType, string.Empty, "Running debug build.");
        }

        /// <summary>
        /// Throws the specified exception with the exception message written to console as an error.
        /// </summary>
        /// <param name="classType">Class type</param>
        /// <param name="exception">Exception</param>
        [Conditional(DEBUG_FLAG_DIRECTIVE)]
        public static void ThrowWithMessage(Type classType, Exception exception)
        {
            LogException(classType, exception);
            throw exception;
        }

        /// <summary>
        /// Write formatted message to console using the specified formatter.
        /// </summary>
        /// <param name="classType">Class type</param>
        /// <param name="fmt">Message to be written to the console</param>
        /// <param name="args">Format arguments for the message</param>
        [Conditional(DEBUG_FLAG_DIRECTIVE)]
        private static void InternalWrite(Type classType, string fmt, params object[] args)
        {
            using (_debugFormatterLock.AquireLock())
                CrestronConsole.Print(_formatter.Format(classType.Name + " - " + string.Format(fmt, args)));
        }

        /// <summary>
        /// Writes an exception to the log.
        /// </summary>
        /// <param name="classType">Class type</param>
        /// <param name="ex">Exception type</param>
        [Conditional(DEBUG_FLAG_DIRECTIVE)]
        public static void LogException(Type classType, Exception ex)
        {
            ErrorLog(ex, "Exception: {0}: {1}", ex.Message, ex.StackTrace);
            WriteException(classType, "Exception: {0}: {1}", ex.Message, ex.StackTrace);
        }

        /// <summary>
        /// Writes a message to the log
        /// </summary>
        /// <param name="fmt">Message to be written to the log</param>
        /// <param name="args">Format arguments for the message</param>
        [Conditional(DEBUG_FLAG_DIRECTIVE)]
        public static void Log(string fmt, params object[] args)
        {
            using (_debugFormatterLock.AquireLock())
                CrestronLogger.WriteToLog(_formatter.Format(fmt + "\n", args), 1);
        }

        [Conditional(DEBUG_FLAG_DIRECTIVE)]
        public static void Log(string message)
        {
            Log("{0}", message);
        }

        /// <summary>
        /// Writes a message to the error log.
        /// </summary>
        /// <param name="messageType">Error log message type</param>
        /// <param name="fmt">Message to be written to the log</param>
        /// <param name="args">Format arguments for the message</param>
        [Conditional(DEBUG_FLAG_DIRECTIVE)]
        public static void ErrorLog(ErrorLogMessageType messageType, string fmt, params object[] args)
        {
            using (_debugFormatterLock.AquireLock())
            {
                var msg = _formatter.Format(fmt + "\n", args);

                switch (messageType)
                {
                    case ErrorLogMessageType.Ok:
                        Crestron.SimplSharp.ErrorLog.Ok("{0}", msg);
                        break;
                    case ErrorLogMessageType.Info:
                        Crestron.SimplSharp.ErrorLog.Info("{0}", msg);
                        break;
                    case ErrorLogMessageType.Warn:
                        Crestron.SimplSharp.ErrorLog.Warn("{0}", msg);
                        break;
                    case ErrorLogMessageType.Error:
                        Crestron.SimplSharp.ErrorLog.Error("{0}", msg);
                        break;
                    case ErrorLogMessageType.Notice:
                        Crestron.SimplSharp.ErrorLog.Notice("{0}", msg);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("messageType");
                }
            }
        }
        
        [Conditional(DEBUG_FLAG_DIRECTIVE)]
        public static void ErrorLog(ErrorLogMessageType messageType, string message)
        {
            ErrorLog(messageType, "{0}", message);
        }
        
        /// <summary>
        /// Writes an exception to the error log.
        /// </summary>
        /// <param name="exception">Exception type</param>
        /// <param name="fmt">Format message</param>
        /// <param name="args">Message args</param>
        [Conditional(DEBUG_FLAG_DIRECTIVE)]
        public static void ErrorLog(Exception exception, string fmt, params object[] args)
        {
            using (_debugFormatterLock.AquireLock())
            {
                var msg = _formatter.Format(fmt + "\n", args);
                Crestron.SimplSharp.ErrorLog.Exception(msg, exception);
            }
        }
        
        [Conditional(DEBUG_FLAG_DIRECTIVE)]
        public static void ErrorLog(Exception exception, string message)
        {
            ErrorLog(exception, "{0}", message);
        }

        /// <summary>
        /// Writes a trace message to the console if the trace option is enabled.
        /// </summary>
        /// <param name="classType">Class type</param>
        /// <param name="fmt">Format string used to write to the console</param>
        /// <param name="args">Format arguments for the message</param>
        [Conditional(DEBUG_FLAG_DIRECTIVE)]
        public static void TraceWrite(Type classType, string fmt, params object[] args)
        {
            if (EnableTrace)
            {
                InternalWrite(classType, string.Format("\xFA\xE0{0}{1}\xFB",
                    (byte)InitialParametersClass.ApplicationNumber, fmt), args);
            }
        }
        
        [Conditional(DEBUG_FLAG_DIRECTIVE)]
        public static void TraceWrite(Type classType, string message)
        {
            TraceWrite(classType, "{0}", message);
        }

        /// <summary>
        /// Writes a raw debug message to the console with a newline (only for DEBUG builds).
        /// </summary>
        /// <param name="classType">Class type</param>
        /// <param name="fmt">Format message to output to the console</param>
        /// <param name="args">Format message string arguments (if applicable)</param>
        [Conditional(DEBUG_FLAG_DIRECTIVE)]
        public static void WriteLine(Type classType, string fmt, params object[] args)
        {
            InternalWrite(classType, "{0}\n", string.Format(fmt, args));
        }

        [Conditional(DEBUG_FLAG_DIRECTIVE)]
        public static void WriteLine(Type classType, string message)
        {
            WriteLine(classType, "{0}", message);
        }

        /// <summary>
        /// Writes an exception debug message to the console with a newline.
        /// </summary>
        /// <param name="classType">Class type</param>
        /// <param name="fmt">Format message to output to the console</param>
        /// <param name="args">Format message string arguments (if applicable)</param>
        [Conditional(DEBUG_FLAG_DIRECTIVE)]
        public static void WriteException(Type classType, string fmt, params object[] args)
        {
            if (DebugLevel.HasFlag(DebugLevelFlags.Exception))
                InternalWrite(classType, "NOTICE: [Exception] {0}\n", string.Format(fmt, args));
        }

        [Conditional(DEBUG_FLAG_DIRECTIVE)]
        public static void WriteException(Type classType, string message)
        {
            WriteException(classType, "{0}", message);
        }

        [Conditional(DEBUG_FLAG_DIRECTIVE)]
        public static void WriteException(Type classType, Exception obj)
        {
            WriteException(classType, "{0}", obj);
        }

        /// <summary>
        /// Writes an error debug message to the console with a newline.
        /// </summary>
        /// <param name="classType">Class type</param>
        /// <param name="fmt">Format message to output to the console</param>
        /// <param name="args">Format message string arguments (if applicable)</param>
        [Conditional(DEBUG_FLAG_DIRECTIVE)]
        public static void WriteError(Type classType, string fmt, params object[] args)
        {
            if (DebugLevel.HasFlag(DebugLevelFlags.Error))
                InternalWrite(classType, "Error: {0}\n", string.Format(fmt, args));
        }

        [Conditional(DEBUG_FLAG_DIRECTIVE)]
        public static void WriteError(Type classType, string message)
        {
            WriteError(classType, "{0}", message);
        }

        /// <summary>
        /// Writes a warning debug message to the console with a newline.
        /// </summary>
        /// <param name="classType">Class type</param>
        /// <param name="fmt">Format message to output to the console</param>
        /// <param name="args">Format message string arguments (if applicable)</param>
        [Conditional(DEBUG_FLAG_DIRECTIVE)]
        public static void WriteWarning(Type classType, string fmt, params object[] args)
        {
            if (DebugLevel.HasFlag(DebugLevelFlags.Warning))
                InternalWrite(classType, "Warning: {0}\n", string.Format(fmt, args));
        }

        [Conditional(DEBUG_FLAG_DIRECTIVE)]
        public static void WriteWarning(Type classType, string message)
        {
            WriteWarning(classType, "{0}", message);
        }

        /// <summary>
        /// Writes an info debug message to the console with a newline.
        /// </summary>
        /// <param name="classType">Class type</param>
        /// <param name="fmt">Format message to output to the console</param>
        /// <param name="args">Format message string arguments (if applicable)</param>
        [Conditional(DEBUG_FLAG_DIRECTIVE)]
        public static void WriteInfo(Type classType, string fmt, params object[] args)
        {
            if (DebugLevel.HasFlag(DebugLevelFlags.Info))
                InternalWrite(classType, "Info: {0}\n", string.Format(fmt, args));
        }

        [Conditional(DEBUG_FLAG_DIRECTIVE)]
        public static void WriteInfo(Type classType, string message)
        {
            WriteInfo(classType, "{0}", message);
        }

        /// <summary>
        /// Conditionally write using the specified Debug Write method delegate type.
        /// </summary>
        /// <param name="condition">Condition that must evaluate as true before the write method is invoked</param>
        /// <param name="method">Debug write method delegate type</param>
        /// <param name="fmt">Format message</param>
        /// <param name="args">Message arguments</param>
        [Conditional(DEBUG_FLAG_DIRECTIVE)]
        public static void ConditionalWrite(bool condition, Action<string, object[]> method, string fmt, params object[] args)
        {
            if (condition && method != null)
                method.Invoke(fmt, args);
        }

        /// <summary>
        /// Conditionally write using the specified Debug Write method delegate type.
        /// </summary>
        /// <param name="condition">Condition that must evaluate as true before the write method is invoked</param>
        /// <param name="method">Debug write method delegate type</param>
        /// <param name="obj">Object to write</param>
        /// <typeparam name="T">Type of params to write</typeparam>
        [Conditional(DEBUG_FLAG_DIRECTIVE)]
        public static void ConditionalWrite<T>(bool condition, Action<T> method, T obj)
        {
            if (condition && method != null)
                method.Invoke(obj);
        }

        /// <summary>
        /// Assertion condition to test.
        /// </summary>
        /// <param name="classType">Class type</param>
        /// <param name="test">Test condition</param>
        /// <param name="fmt">Assertion message raised in exception</param>
        /// <param name="args">Assertion message arguments</param>
        /// <exception cref="AssertionException">Assertion exception</exception>
        [Conditional(DEBUG_FLAG_DIRECTIVE)]
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Global
        public static void Assert(Type classType, bool test, string fmt, params object[] args)
        {
            if (!test)
            {
                WriteError(classType, fmt, args);
                throw new AssertionException(fmt);
            }
        }

        /// /// <summary>
        /// Assertion condition to test.
        /// </summary>
        /// <param name="classType">Class type</param>
        /// <param name="inner">Inner exception</param>
        /// <param name="test">Test condition</param>
        /// <param name="fmt">Assertion message raised in exception</param>
        /// <param name="args">Assertion message arguments</param>
        /// <exception cref="AssertionException">Assertion exception</exception>
        [Conditional(DEBUG_FLAG_DIRECTIVE)]
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Global
        public static void Assert(Type classType, Exception inner, bool test, string fmt, params object[] args)
        {
            if (!test)
            {
                WriteError(classType, fmt, args);
                throw new AssertionException(fmt, inner);
            }
        }
    }
}