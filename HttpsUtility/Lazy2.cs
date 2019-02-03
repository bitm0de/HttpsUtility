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
using HttpsUtility.Threading;

// ReSharper disable MemberCanBePrivate.Global

namespace HttpsUtility
{
    /// <summary>
    /// Wrapper for lazy initialization support (before .NET 4.0).
    /// </summary>
    /// <typeparam name="T">Object type to be lazy initialized.</typeparam>
    public sealed class Lazy2<T> : IDisposable
    {
        private Box _box;
        private volatile bool _initialized;
    
        [NonSerialized]
        private readonly Func<T> _valueInitFunc;
    
        [NonSerialized]
        private readonly ILockSynchronization _syncSection = new SyncSection();

        public bool Initialized
        {
            get { using (_syncSection.AquireLock()) { return _initialized; } }
        }
    
        public T Value
        {
            get
            {
                if (!_initialized)
                {
                    using (_syncSection.AquireLock())
                    {
                        if (!_initialized)
                        {
                            _box = new Box(_valueInitFunc());
                            _initialized = true;
                        }
                    }
                }

                return _box.Value;
            }
        }

        public Lazy2(Func<T> valueInitFunc)
        {
            _valueInitFunc = valueInitFunc;
        }
    
        public override string ToString()
        {
            return _initialized ? Value.ToString() : base.ToString();
        }

        public void Dispose()
        {
            if (Initialized && _box.Value is IDisposable)
            {
                ((IDisposable)_box.Value).Dispose();
            }
        }
    
        [Serializable]
        private class Box
        {
            internal readonly T Value;
            internal Box(T value) { Value = value; }
        }
    }
}