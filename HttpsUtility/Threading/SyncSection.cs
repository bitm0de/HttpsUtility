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
    /// <inheritdoc />
    /// <summary>
    /// Thread synchronization critical section lock for mutually exclusive access to a section of code
    /// </summary>
    public sealed class SyncSection : ILockSynchronization
    {
        private readonly CEvent _delayEvent = new CEvent(false, false);
        private readonly CCriticalSection _criticalSection = new CCriticalSection();

        #region ISynchronization Members

        /// <summary>
        /// Attempts to aquire a lock to access the section of code (non-blocking).
        /// (Note: You must check the returned LockToken in this case to ensure that
        /// the lock was actually aquired.)
        /// </summary>
        /// <returns>LockToken object</returns>
        public bool TryAquireLock(out LockToken lockToken)
        {
            if (_criticalSection.TryEnter())
            {
                lockToken = new LockToken(this);
                return true;
            }
            lockToken = default(LockToken);
            return false;
        }

        /// <summary>
        /// Aquire a lock and return the LockToken when the lock is aquired.
        /// </summary>
        /// <returns>LockToken object</returns>
        public LockToken AquireLock()
        {
            _criticalSection.Enter();
            return new LockToken(this);
        }

        /// <summary>
        /// Aquire a lock and return the LockToken when the lock is aquired.
        /// </summary>
        /// <param name="waitTime">Amount of time to wait to aquire the lock in milliseconds</param>
        /// <returns>LockToken object</returns>
        public LockToken AquireLock(int waitTime)
        {
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < waitTime)
            {
                if (_criticalSection.TryEnter())
                    return new LockToken(this);
                
                _delayEvent.Wait(100);
            }
            return new LockToken(null);
        }

        /// <summary>
        /// Manually release the lock.
        /// (Note: This is done automatically when the LockToken is disposed)
        /// </summary>
        public void ReleaseLock()
        {
            _criticalSection.Leave();
        }

        #endregion
    }
}