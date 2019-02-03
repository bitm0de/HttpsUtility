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

namespace HttpsUtility.Threading
{
    /// <summary>
    /// Thread synchronization auto reset event.
    /// </summary>
    public sealed class SyncAutoResetEvent
    {
        private readonly CEvent _cEvent;

        public SyncAutoResetEvent()
            : this(true)
        { }

        public SyncAutoResetEvent(bool initialState)
        {
            _cEvent = new CEvent(true, initialState);
        }
        
        /// <summary>
        /// Waits indefinitely to be signaled.
        /// </summary>
        /// <returns>True if signaled, otherwise false.</returns>
        /// <remarks>Automatically resets the state of the signal.</remarks>
        public bool Wait()
        {
            return _cEvent.Wait();
        }
        
        /// <summary>
        /// Waits to be signaled with the specified timeout (in milliseconds).
        /// </summary>
        /// <param name="timeout">Timeout (ms)</param>
        /// <returns>True if signaled, otherwise false.</returns>
        /// <remarks>Automatically resets the state of the signal.</remarks>
        public bool Wait(int timeout)
        {
            return _cEvent.Wait(timeout);
        }

        /// <summary>
        /// Signals to other threads.
        /// </summary>
        /// <returns>True if successful, otherwise false.</returns>
        public bool Set()
        {
            return _cEvent.Set();
        }
        
        /// <summary>
        /// Resets the signal for other threads forcing them to wait.
        /// </summary>
        /// <returns>True if successful, otherwise false.</returns>
        public bool Reset()
        {
            return _cEvent.Reset();
        }
    }
}