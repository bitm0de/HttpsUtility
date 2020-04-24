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
using Crestron.SimplSharp.Net.Https;
using HttpsUtility.Diagnostics;

using ContentSource = Crestron.SimplSharp.Net.Https.ContentSource;
using RequestType = Crestron.SimplSharp.Net.Https.RequestType;

namespace HttpsUtility.Https
{
    public sealed class HttpsClientPool : IDisposable
    {
        private const int HttpsClientPoolSize = 10;

        private readonly ObjectPool<Lazy<HttpsClient>> _httpsClientPool
            = new ObjectPool<Lazy<HttpsClient>>(HttpsClientPoolSize, HttpsClientPoolSize, 
                () => new Lazy<HttpsClient>(() => new HttpsClient {
                    PeerVerification = false, HostVerification = false,
                    TimeoutEnabled = true, Timeout = 5, KeepAlive = false
                })) { CleanupPoolOnDispose = true };

        private HttpsResult SendRequest(string url, RequestType requestType, IEnumerable<KeyValuePair<string, string>> additionalHeaders, string content)
        {
            var obj = _httpsClientPool.GetFromPool();
            var client = obj.Value;

            try
            {
                Debug.WriteInfo("Making API GET request to endpoint: {0}", url);
                
                if (client.ProcessBusy)
                    client.Abort();

                var httpsRequest = new HttpsClientRequest {
                    RequestType = requestType,
                    Encoding = Encoding.UTF8,
                    KeepAlive = false,
                };

                if (requestType != RequestType.Get && !string.IsNullOrEmpty(content))
                {
                    httpsRequest.ContentSource = ContentSource.ContentString;
                    httpsRequest.ContentString = content;
                }
                
                if (additionalHeaders != null)
                {
                    foreach (var item in additionalHeaders)
                        httpsRequest.Header.AddHeader(new HttpsHeader(item.Key, item.Value));
                }

                httpsRequest.Url.Parse(url);

                HttpsClientResponse httpResponse = client.Dispatch(httpsRequest);
                return new HttpsResult(httpResponse.Code, httpResponse.ResponseUrl, httpResponse.ContentString);
            }
            catch (Exception ex)
            {
                Debug.WriteException(ex);
            }
            finally
            {
                _httpsClientPool.AddToPool(obj);
            }

            return null;
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
            _httpsClientPool.Dispose();
        }
    }
}
