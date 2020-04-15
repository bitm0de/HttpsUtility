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
using Crestron.SimplSharp;

namespace HttpsUtility
{
    /// <summary>
    /// Wrapper for lazy initialization support (before .NET 4.0).
    /// </summary>
    /// <typeparam name="T">Object type to be lazy initialized.</typeparam>
    public sealed class Lazy<T>
    {
        private Box _boxValue;
        private volatile bool _initialized;

        [NonSerialized] private readonly Func<T> _valueInitFunc;
        [NonSerialized] private readonly CCriticalSection _objLock = new CCriticalSection();

        public bool Initialized
        {
            get
            {
                try
                {
                    _objLock.Enter();
                    return _initialized;
                }
                finally
                {
                    _objLock.Leave();
                }
            }
        }

        public T Value
        {
            get
            {
                if (!_initialized)
                {
                    try
                    {
                        _objLock.Enter();
                        if (!_initialized)
                        {
                            _boxValue = new Box(_valueInitFunc());
                            _initialized = true;
                        }
                    }
                    finally
                    {
                        _objLock.Leave();
                    }
                }

                return _boxValue.Value;
            }
        }

        public Lazy()
            : this(() => (T)Activator.CreateInstance(typeof(T))) { }

        public Lazy(Func<T> valueInitFunc)
        {
            if (valueInitFunc == null)
                throw new ArgumentNullException("valueInitFunc");

            _valueInitFunc = valueInitFunc;
        }

        public override string ToString()
        {
            return _initialized ? Value.ToString() : "null";
        }

        [Serializable]
        private class Box
        {
            internal readonly T Value;

            internal Box(T value)
            {
                Value = value;
            }
        }
    }

    /// <summary>
    /// Non-generic class helper
    /// </summary>
    public sealed class Lazy
    {
        /// <summary>
        /// Creates a default instance of the specified type.
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <returns>Lazy instance of the specified type.</returns>
        /// <remarks>The new() type constraint is placed on this little factory method
        /// as a way to still allow for types that aren't restricted by this limitation
        /// to be used by the class</remarks>
        public static Lazy<T> CreateNew<T>()
            where T : new()
        {
            return new Lazy<T>(Activator.CreateInstance<T>);
        }
    }
}