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

using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;
using HttpsUtility.Https;

// ReSharper disable UnusedMember.Global

namespace HttpsUtility.Symbols
{
    /* --------------------------------------------  GENERIC SIMPL+ TYPE HELPER ALIASES  -------------------------------------------- */
    using STRING = System.String;             // string = STRING
    using SSTRING = SimplSharpString;         // SimplSharpString = STRING (used to interface with SIMPL+)
    using INTEGER = System.UInt16;            // ushort = INTEGER (unsigned)
    using SIGNED_INTEGER = System.Int16;      // short = SIGNED_INTEGER
    using SIGNED_LONG_INTEGER = System.Int32; // int = SIGNED_LONG_INTEGER
    using LONG_INTEGER = System.UInt32;       // uint = LONG_INTEGER (unsigned)
    /* ------------------------------------------------------------------------------------------------------------------------------ */

    public sealed partial class SimplHttpsClient
    {
        private readonly Lazy2<string> _moduleIdentifier;
        private readonly HttpsClient _httpsClient = new HttpsClient();

        public SimplHttpsClient()
        {
            _moduleIdentifier = new Lazy2<string>(() =>
            {
                var asm = Assembly.GetExecutingAssembly().GetName();
                return string.Format("{0} {1}", asm.Name, asm.Version.ToString(2));
            });
        }
        
        private static IEnumerable<KeyValuePair<string, string>> ParseHeaders(STRING input)
        {
            if (string.IsNullOrEmpty(input))
                return null;
            
            var headerTokens = input.Split('|');
            return (from header in headerTokens
                let n = header.IndexOf(':')
                where n != -1
                select new KeyValuePair<string, string>(
                    header.Substring(0, n).Trim(),
                    header.Substring(n + 1).Trim())
                ).ToList();
        }
        
        public INTEGER SendGet(STRING url, STRING headers)
        {
            var response = _httpsClient.Get(url, ParseHeaders(headers));
            OnSimplHttpsClientResponse(response.Status, response.ResponseUrl, response.Content);
            return (INTEGER)response.Status;
        }
        
        public INTEGER SendPost(STRING url, STRING headers, STRING content)
        {
            var response = _httpsClient.Post(url, ParseHeaders(headers), content.NullIfEmpty());
            OnSimplHttpsClientResponse(response.Status, response.ResponseUrl, response.Content);
            return (INTEGER)response.Status;
        }
        
        public INTEGER SendPut(STRING url, STRING headers, STRING content)
        {
            var response = _httpsClient.Put(url, ParseHeaders(headers), content.NullIfEmpty());
            OnSimplHttpsClientResponse(response.Status, response.ResponseUrl, response.Content);
            return (INTEGER)response.Status;
        }
        
        public INTEGER SendDelete(STRING url, STRING headers, STRING content)
        {
            var response = _httpsClient.Delete(url, ParseHeaders(headers), content.NullIfEmpty());
            OnSimplHttpsClientResponse(response.Status, response.ResponseUrl, response.Content);
            return (INTEGER)response.Status;
        }

        public override string ToString()
        {
            return _moduleIdentifier.Value;
        }
    }
}