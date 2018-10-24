#if UNITY_IOS
using System.Runtime.InteropServices;
using System;
#else
using UnityEngine;
#endif

public static class NativeShare {

    public static void Share(string body, string filePath = null, string mimeType = "audio/wav", string chooserText = "Select sharing app") {
		#if UNITY_ANDROID
		ShareAndroid(body, filePath, mimeType, chooserText);
		#elif UNITY_IOS
		ShareIOS(body, new string[] { filePath });
		#else
		Debug.Log("No sharing set up for this platform.");
		#endif
	}

#if UNITY_ANDROID
	public static void ShareAndroid(string body, string filePath, string mimeType, string chooserText) {
		using (AndroidJavaClass intentClass = new AndroidJavaClass ("android.content.Intent"))
		using (AndroidJavaObject intentObject = new AndroidJavaObject ("android.content.Intent")) {
			using (intentObject.Call<AndroidJavaObject> ("setAction", intentClass.GetStatic<string> ("ACTION_SEND"))) {}
			using (intentObject.Call<AndroidJavaObject> ("setType", mimeType)) {}

			using (AndroidJavaClass uriClass = new AndroidJavaClass ("android.net.Uri")) {
				using (AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject> ("parse", "file:///" + filePath)) {
					using (intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uriObject)) { }
				}
			}

			// finally start application
			using (AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			using (AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity")) {
            	AndroidJavaObject jChooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, chooserText);
                currentActivity.Call("startActivity", jChooser);

			}
		}
	}
#endif

#if UNITY_IOS
	public struct ConfigStruct {
		public string title;
		public string message;
	}

	[DllImport ("__Internal")] private static extern void showAlertMessage(ref ConfigStruct conf);

	public struct SocialSharingStruct {
		public string text;
		public string subject;
		public string filePaths;
	}

	[DllImport ("__Internal")] private static extern void showSocialSharing(ref SocialSharingStruct conf);

	public static void ShareIOS(string title, string message) {
		ConfigStruct conf = new ConfigStruct();
		conf.title  = title;
		conf.message = message;
		showAlertMessage(ref conf);
	}

	public static void ShareIOS(string body, string[] filePaths) {
		SocialSharingStruct conf = new SocialSharingStruct();
		conf.text = body;
		string paths = string.Join(";", filePaths);
		conf.filePaths = paths;
		showSocialSharing(ref conf);
	}
#endif
}
