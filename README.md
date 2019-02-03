# HttpsUtility
A basic HTTPS utility S# module for general use where simple secure web requests are necessary, but complex chains of requests involving cookies, and response parsing is not. This module was intended for simple functionality only, where a custom solution is not needed.

## Generated API (Intended for use by SIMPL+)
```cs
namespace HttpsUtility.Symbols
{
	// note: type can be instantiated from SIMPL+
	class SimplHttpsClient
	{
		// class properties
		// RegisterDelegate(obj, SimplHttpsClientResponse, SimplHttpsClientResponseHandler);
		// CALLBACK FUNCTION SimplHttpsClientResponseHandler(INTEGER status, STRING responseUrl, STRING content);
		DelegateProperty SimplHttpsClientResponseDelegate SimplHttpsClientResponse(INTEGER status, STRING responseUrl, STRING content);

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
If you do not want to build your own modules from the compiled S# archive, you can download our SIMPL user modules with the SIMPL# library in a demo program here: https://sharptoothcode.com/product/httpsutility/
