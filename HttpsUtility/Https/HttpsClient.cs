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
using System.Text;
using Crestron.SimplSharp.Net.Http;
using Crestron.SimplSharp.Net.Https;
using HttpsUtility.Diagnostics;
using HttpsUtility.Threading;

using RequestType = Crestron.SimplSharp.Net.Https.RequestType;

// ReSharper disable UnusedMember.Global

namespace HttpsUtility.Https
{
    public sealed class HttpsClient : IDisposable
    {
        private readonly SyncSection _requestLock = new SyncSection();
        private readonly Lazy<Crestron.SimplSharp.Net.Https.HttpsClient> _httpsClient
            = new Lazy<Crestron.SimplSharp.Net.Https.HttpsClient>(
                    () => new Crestron.SimplSharp.Net.Https.HttpsClient { PeerVerification = false }
                );

        private static HttpsClientRequest CreateDefaultClientRequest(string url, RequestType requestType)
        {
            var httpRequest = new HttpsClientRequest
            {
                Encoding = Encoding.UTF8,
                RequestType = requestType,
                Url = new UrlParser(url)
            };
            return httpRequest;
        }

        private HttpsResult SendRequest(string url, RequestType requestType, IEnumerable<KeyValuePair<string, string>> additionalHeaders, string value)
        {
            using (_requestLock.AquireLock())
            {
                HttpsClientRequest httpRequest = CreateDefaultClientRequest(url, requestType);
                
                if (additionalHeaders != null)
                {
                    foreach (var item in additionalHeaders)
                        httpRequest.Header.AddHeader(new HttpsHeader(item.Key, item.Value));
                }

                if (!string.IsNullOrEmpty(value))
                    httpRequest.ContentString = value;

                try
                {
                    HttpsClientResponse httpResponse = _httpsClient.Value.Dispatch(httpRequest);
                    return new HttpsResult(httpResponse.Code, httpResponse.ResponseUrl, httpResponse.ContentString);
                }
                catch (HttpsException ex)
                {
                    Debug.LogException(GetType(), ex);
                }

                return null;
            }
        }

        public HttpsResult Get(string url)
        {
            return Get(url, null);
        }

        public HttpsResult Get(string url, IEnumerable<KeyValuePair<string, string>> additionalHeaders)
        {
            return SendRequest(url, RequestType.Get, additionalHeaders, null);
        }

        public HttpsResult Post(string url, string value)
        {
            return Post(url, null, value);
        }

        public HttpsResult Post(string url, IEnumerable<KeyValuePair<string, string>> additionalHeaders, string value)
        {
            return SendRequest(url, RequestType.Post, additionalHeaders, value);
        }

        public HttpsResult Put(string url, string value)
        {
            return Put(url, null, value);
        }

        public HttpsResult Put(string url, IEnumerable<KeyValuePair<string, string>> additionalHeaders, string value)
        {
            return SendRequest(url, RequestType.Put, additionalHeaders, value);
        }

        public HttpsResult Delete(string url)
        {
            return Delete(url, null);
        }

        public HttpsResult Delete(string url, string value)
        {
            return Delete(url, null, value);
        }

        public HttpsResult Delete(string url, IEnumerable<KeyValuePair<string, string>> additionalHeaders, string value)
        {
            return SendRequest(url, RequestType.Delete, additionalHeaders, value);
        }

        public void Dispose()
        {
            _httpsClient.Dispose();
        }
    }
}