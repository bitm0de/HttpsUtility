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
using HttpsUtility.Diagnostics;
using HttpsUtility.Https;

namespace HttpsUtility.Symbols
{
    // ReSharper disable once UnusedType.Global
    public sealed partial class SimplHttpsClient
    {
        private readonly string _moduleIdentifier;
        private readonly HttpsClientPool _httpsClientPool = new HttpsClientPool();
        
        public SimplHttpsClient()
        {
            var asm = Assembly.GetExecutingAssembly().GetName();
            _moduleIdentifier = string.Format("{0} {1}", asm.Name, asm.Version.ToString(2));
        }

        private static IEnumerable<KeyValuePair<string, string>> ParseHeaders(string input)
        {
            if (string.IsNullOrEmpty(input))
                return new KeyValuePair<string, string>[] { };
            
            var headerTokens = input.Split('|');
            return (from header in headerTokens
                let n = header.IndexOf(':')
                where n != -1
                select new KeyValuePair<string, string>(
                    header.Substring(0, n).Trim(),
                    header.Substring(n + 1).Trim())
                ).ToList();
        }

        private ushort MakeRequest(Func<HttpsResult> action)
        {
            try
            {
                var response = action.Invoke();
                if (response == null)
                {
                    Debug.WriteError("HTTPS response object was null.");
                    return 0;
                }

                // Some HTTP(S) responses will not have a message body.
                if (response.Content == null)
                {
                    OnSimplHttpsClientResponse(response.Status, response.ResponseUrl, string.Empty, 0);
                }
                else
                {
                    foreach (var contentChunk in response.Content.SplitIntoChunks(255))
                    {
                        OnSimplHttpsClientResponse(response.Status, response.ResponseUrl, contentChunk, response.Content.Length);
                        CrestronEnvironment.Sleep(10); // allow a little bit for things to process
                    }
                }

                return (ushort)response.Status;
            }
            catch (Exception ex)
            {
                Debug.WriteException(ex);
                return 0;
            }
        }
        
        public ushort SendGet(string url, string headers)
        {
            return MakeRequest(() => _httpsClientPool.Get(url, ParseHeaders(headers)));
        }
        
        public ushort SendPost(string url, string headers, string content)
        {
            return MakeRequest(() => _httpsClientPool.Post(url, ParseHeaders(headers), content.NullIfEmpty()));
        }
        
        public ushort SendPut(string url, string headers, string content)
        {
            return MakeRequest(() => _httpsClientPool.Put(url, ParseHeaders(headers), content.NullIfEmpty()));
        }
        
        public ushort SendDelete(string url, string headers, string content)
        {
            return MakeRequest(() => _httpsClientPool.Delete(url, ParseHeaders(headers), content.NullIfEmpty()));
        }

        public override string ToString()
        {
            return _moduleIdentifier;
        }
    }
}