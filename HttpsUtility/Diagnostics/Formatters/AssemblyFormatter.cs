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

using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;

namespace HttpsUtility.Diagnostics.Formatters
{
    /// <inheritdoc />
    /// <summary>
    /// Diagnostic assembly formatter class.
    /// </summary>
    public sealed class AssemblyFormatter : IDiagnosticsFormatter
    {
        /// <summary>Current executing assembly debug prefix</summary>
        private static readonly string _assemblyPrefix;
        
        static AssemblyFormatter()
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            _assemblyPrefix = string.Format("{0} {1}", assemblyName.Name, assemblyName.Version);
        }
        
        /// <summary>
        /// Private helper method to return formatted message string prefixed with the assembly identifier.
        /// </summary>
        /// <param name="message">Message to be written to the console</param>
        /// <param name="args">Format arguments for the message</param>
        public string Format(string message, params object[] args)
        {
            var description = string.Format(message, args);
            return string.Format("[{0}]:{1} - {2}", _assemblyPrefix, InitialParametersClass.ApplicationNumber, description);
        }
    }
}