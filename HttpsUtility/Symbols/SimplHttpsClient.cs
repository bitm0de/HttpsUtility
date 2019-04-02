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
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;
using HttpsUtility.Https;
using HttpsUtility.Threading;

// ReSharper disable UnusedMember.Global

namespace HttpsUtility.Symbols
{
    /* --------------------------------------------  GENERIC SIMPL+ TYPE HELPER ALIASES  -------------------------------------------- */
    using STRING = String;             // string = STRING
    using SSTRING = SimplSharpString;  // SimplSharpString = STRING (used to interface with SIMPL+)
    using INTEGER = UInt16;            // ushort = INTEGER (unsigned)
    using SIGNED_INTEGER = Int16;      // short = SIGNED_INTEGER
    using SIGNED_LONG_INTEGER = Int32; // int = SIGNED_LONG_INTEGER
    using LONG_INTEGER = UInt32;       // uint = LONG_INTEGER (unsigned)
    /* ------------------------------------------------------------------------------------------------------------------------------ */

    public sealed partial class SimplHttpsClient
    {
        private readonly Lazy<string> _moduleIdentifier;
        private readonly HttpsClient _httpsClient = new HttpsClient();
        private readonly SyncSection _httpsOperationLock = new SyncSection();
        
        public SimplHttpsClient()
        {
            _moduleIdentifier = new Lazy<string>(() =>
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

        private INTEGER MakeRequest(Func<HttpsResult> action)
        {
            using (_httpsOperationLock.AquireLock())
            {
                var response = action.Invoke();
                foreach (var contentChunk in response.Content.SplitIntoChunks(250))
                {
                    OnSimplHttpsClientResponse(response.Status, response.ResponseUrl, contentChunk, response.Content.Length);
                    CrestronEnvironment.Sleep(10); // allow for things to process
                }
                return (INTEGER)response.Status;
            }
        }
        
        public INTEGER SendGet(STRING url, STRING headers)
        {
            return MakeRequest(() => _httpsClient.Get(url, ParseHeaders(headers)));
        }
        
        public INTEGER SendPost(STRING url, STRING headers, STRING content)
        {
            return MakeRequest(() => _httpsClient.Post(url, ParseHeaders(headers), content.NullIfEmpty()));
        }
        
        public INTEGER SendPut(STRING url, STRING headers, STRING content)
        {
            return MakeRequest(() => _httpsClient.Put(url, ParseHeaders(headers), content.NullIfEmpty()));
        }
        
        public INTEGER SendDelete(STRING url, STRING headers, STRING content)
        {
            return MakeRequest(() => _httpsClient.Delete(url, ParseHeaders(headers), content.NullIfEmpty()));
        }

        public override string ToString()
        {
            return _moduleIdentifier.Value;
        }
    }
}