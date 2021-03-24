# HttpsUtility
A basic HTTPS utility S# module for general use where simple secure web requests are necessary, but complex chains of requests involving cookies, and response parsing is not. This module was intended for simple functionality only, where a custom solution is not needed.

[![modules](https://img.shields.io/badge/S%23-Modules-brightgreen.svg)](https://sharptoothcode.com) [![release](https://img.shields.io/github/release/bitm0de/HttpsUtility.svg?style=flat)](https://github.com/bitm0de/HttpsUtility/releases) [![downloads](https://img.shields.io/github/downloads/bitm0de/HttpsUtility/total.svg?style=flat)](https://github.com/bitm0de/HttpsUtility/releases) [![issues](https://img.shields.io/github/issues/bitm0de/HttpsUtility.svg?style=flat)](https://github.com/bitm0de/HttpsUtility/issues) [![license](https://img.shields.io/github/license/bitm0de/HttpsUtility.svg?style=flat)](https://github.com/bitm0de/HttpsUtility/blob/master/LICENSE)

## Compatibility
| Controller  | Supported     |
| ----------- | ------------- |
| MC3         | Not Supported |

## Information
Multiple headers are supported, the formatting for sending multiple headers to the public functions requires each header to be separated by a `|` character.

Example:
```Accept: application/json|Content-Type: application/json```

## Generated API (Intended for use by SIMPL+)
```cs
namespace HttpsUtility.Symbols
{
  // note: type can be instantiated from SIMPL+
  class SimplHttpsClient
  {
    // class properties
    // RegisterDelegate(obj, SimplHttpsClientResponse, SimplHttpsClientResponseHandler);
    // CALLBACK FUNCTION SimplHttpsClientResponseHandler(INTEGER status, STRING responseUrl, STRING content, INTEGER length);
    DelegateProperty SimplHttpsClientResponseDelegate SimplHttpsClientResponse(INTEGER status, STRING responseUrl, STRING content, INTEGER length);

    // class methods
    INTEGER_FUNCTION SendGet(STRING url, STRING headers);
    INTEGER_FUNCTION SendPost(STRING url, STRING headers, STRING content);
    INTEGER_FUNCTION SendPut(STRING url, STRING headers, STRING content);
    INTEGER_FUNCTION SendDelete(STRING url, STRING headers, STRING content);
    STRING_FUNCTION ToString();
    SIGNED_LONG_INTEGER_FUNCTION GetHashCode();
  }
}
```

## Downloads

* Note: I have no direct affiliation with "SharptoothCode" anymore due to employment changes. I do still have full control over maintaining this repository however. (Current releases on Github will contain the relevant SIMPL Windows and SIMPL+ module files.)

Latest release is available on the releases section: https://github.com/bitm0de/HttpsUtility/releases
