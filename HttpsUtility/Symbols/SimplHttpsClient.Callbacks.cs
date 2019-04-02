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
        public SimplHttpsClientResponseDelegate SimplHttpsClientResponse { get; set; }

        private void OnSimplHttpsClientResponse(int status, string responseUrl, string content, int length)
        {
            var handler = SimplHttpsClientResponse;
            if (handler != null) handler.Invoke((INTEGER)status, responseUrl.EmptyIfNull(), content.EmptyIfNull(), (INTEGER)length);
        }
    }
}