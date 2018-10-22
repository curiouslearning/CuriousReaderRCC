using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System.IO;
using UnityEngine.UI;

/// <summary>
/// Script responsible for logging events to firebase.
/// </summary>
public class FirebaseHelper  : MonoBehaviour{

	JSONNode dataToLog;
	string dataAsJSON, path;
	string[] apps;
	public static int appID, secID;
	static int tabID = 5001;
	public Text text;

	void Start () {
		
		StartCoroutine(checkInternetConnection((isConnected)=>{
			// handle connection status here
			if(isConnected){
			    Debug.Log("internet connection found");
				//sending data directly to firebase using "72 hours rule"! (removed local data storage)
			    //LogEvent ();
			
			}
			else{
				Debug.Log("not connected");
			}
		}));

	}
		
	/// <summary>
	/// Checks the internet connection.
	/// </summary>
	/// <param name="action">Action delegate: Value for internet connectivity.</param>
	IEnumerator checkInternetConnection(System.Action<bool> action){
		WWW www = new WWW("http://google.com");
		yield return www;
		if (www.error != null) {
			action (false);
		} else {
			action (true);
		}
	} 

	/// <summary>
	/// Assigns the value of book id to appID.
	/// </summary>
	/// <param name="id">book id.</param>
	public static void AddBook(int id){
		appID = id;
	
	}
		
	/// <summary>
	/// Assigns the value of page number to secID.
	/// </summary>
	/// <param name="no">Page number.</param>
	public static void AddSection(int no){
		secID = no;
	}

	//sending data directly to firebase using "72 hours rule"! (removed local data storage)
	/// <summary>
	/// Logs the event stored in the Local JSON to firebase and deletes from JSON simultaneously.
	/// </summary>
	public void LogEvent(){

		string label, time, timeEnter;
		string answer, selection, foilList;
		double timeSpent;
		long count;

		path = DataCollection.GetPath ();
		if (File.Exists(path)) {
			dataAsJSON = File.ReadAllText (path);
			dataToLog = JSON.Parse (dataAsJSON);

			if (dataToLog.Tag == JSONNodeType.Object)
			{
				// take each node under tablet id as an app's/book's value.
				foreach (KeyValuePair<string, JSONNode> app in (JSONObject)dataToLog[tabID])
				{
					appID = int.Parse(app.Key);
					Debug.Log (app+"");

					//take each section/page under the app and take all the three events
					foreach (KeyValuePair<string, JSONNode> section in (JSONObject)app.Value) {
						secID =  int.Parse(section.Key);

						// for all IN_APP_SECTION event, take the value of parameters and log the event to firebase
						if (dataToLog [tabID] [app.Key] [section.Key] ["IN_APP_SECTION"] != null) {

							count = (dataToLog [tabID] [app.Key] [section.Key] ["IN_APP_SECTION"]).Count;  
							for (int i = 0; i < count; i++) {
								timeEnter = dataToLog [tabID] [app.Key] [section.Key] ["IN_APP_SECTION"] [0] ["inTime"];
								timeSpent = dataToLog [tabID] [app.Key] [section.Key] ["IN_APP_SECTION"] [0] ["timeSpent"];
								//log the section data to firebase
								LogInAppSection (timeEnter, timeSpent);
								dataToLog [tabID] [app.Key] [section.Key] ["IN_APP_SECTION"].Remove (0);
							}

						}

						// for all IN_APP_TOUCH event, take the value of parameters and log the event to firebase
						if (dataToLog [tabID] [app.Key] [section.Key] ["IN_APP_TOUCH"] != null) {
							count = (dataToLog [tabID] [app.Key] [section.Key] ["IN_APP_TOUCH"]).Count;
							for (int i = 0; i < count; i++) {
								label = dataToLog [tabID] [app.Key] [section.Key] ["IN_APP_TOUCH"] [0] ["label"];
								time = dataToLog [tabID] [app.Key] [section.Key] ["IN_APP_TOUCH"] [0] ["time"];
								LogInAppTouch ( label,time);
								dataToLog [tabID] [app.Key] [section.Key] ["IN_APP_TOUCH"].Remove (0);
							}
						}


						// for all IN_APP_RESPONSE event, take the value of parameters and log the event to firebase
						if (dataToLog [tabID] [app.Key] [section.Key] ["IN_APP_RESPONSE"] != null) {
							count = (dataToLog [tabID] [app.Key] [section.Key] ["IN_APP_RESPONSE"]).Count;
							for (int i = 0; i < count; i++) {
								answer = dataToLog [tabID] [app.Key] [section.Key] ["IN_APP_RESPONSE"] [0] ["answer"];
								selection = dataToLog [tabID] [app.Key] [section.Key] ["IN_APP_RESPONSE"] [0] ["selection"];
								time = dataToLog [tabID] [app.Key] [section.Key] ["IN_APP_RESPONSE"] [0] ["timeTaken"];
								foilList = dataToLog [tabID] [app.Key] [section.Key] ["IN_APP_RESPONSE"] [0] ["foil"].ToString();
								LogInAppResponse (selection,answer,foilList,time);
								dataToLog [tabID] [app.Key] [section.Key] ["IN_APP_RESPONSE"].Remove (0);
							}
						}


					}


				}
				    

			}
		}

		else {
			Debug.Log ("no file to log data!");		
 		}

		DataCollection.SaveLocalJSON (dataToLog);
	}

	/// <summary>
	/// Logs IN_APP_SECTION event to firebase.
	/// IN_APP_SECTION depicts time spent on a particular page.
	/// </summary>
	/// <param name="timeEnter">Time of enter into the page.</param>
	/// <param name="timeSpent">Time spent on the page.</param>
	public static void LogInAppSection( string timeEnter, double timeSpent){

		if (timeEnter != null ) {
			Firebase.Analytics.FirebaseAnalytics.LogEvent (
				"IN_APP_SECTION",
				new Firebase.Analytics.Parameter[] {
					new Firebase.Analytics.Parameter (
						"TABLET_ID", tabID),
					new Firebase.Analytics.Parameter (
						"APP_ID", appID),
					new Firebase.Analytics.Parameter (
						"SECTION_ID", secID),
					new Firebase.Analytics.Parameter (
						"TIME_ENTER", timeEnter),
					new Firebase.Analytics.Parameter (
						"TIME_SPENT", timeSpent)
				}
			);
		}
	}

	/// <summary>
	/// Logs IN_APP_SECTION event for shelf to firebase(special functions because shelf doesnot have section id!!!).
	/// </summary>
	/// <param name="timeEnter">Time of enter into the page.</param>
	/// <param name="timeSpent">Time spent on the page.</param>
	public static void LogInShelfSection( string timeEnter, double timeSpent){

		if (timeEnter != null) {
			Firebase.Analytics.FirebaseAnalytics.LogEvent (
				"IN_APP_SECTION",
				new Firebase.Analytics.Parameter[] {
					new Firebase.Analytics.Parameter (
						"TABLET_ID", tabID),
					new Firebase.Analytics.Parameter (
						"APP_ID", 0),
					new Firebase.Analytics.Parameter (
						"TIME_ENTER", timeEnter),
					new Firebase.Analytics.Parameter (
						"TIME_SPENT", timeSpent)
				}
			);
		}
	}


	/// <summary>
	/// Logs the IN_APP_TOUCH event to firebase.
	/// IN_APP_TOUCH event depicts the touch on any graphic, text, button.
	/// </summary>
	/// <param name="label">Label of the element touched.</param>
	/// <param name="timestamp">Time of touch.</param>
	public static void LogInAppTouch( string label, string timestamp){

		if (label != null ) {
			Firebase.Analytics.FirebaseAnalytics.LogEvent (
				"IN_APP_TOUCH",
				new Firebase.Analytics.Parameter[] {
					new Firebase.Analytics.Parameter (
						"TABLET_ID", tabID),
					new Firebase.Analytics.Parameter (
						"APP_ID", appID),
					new Firebase.Analytics.Parameter (
						"SECTION_ID", secID),
					new Firebase.Analytics.Parameter (
						"LABEL", label),
					new Firebase.Analytics.Parameter (
						"TIME_OF_TOUCH", timestamp)
				}
			);
		}
	}

	/// <summary>
	/// Logs IN_APP_TOUCH event for shelf to firebase(special functions because shelf doesnot have section id!!!).
	/// IN_APP_TOUCH event depicts the touch on any graphic, text, button.
	/// </summary>
	/// <param name="label">Label of the element touched.</param>
	/// <param name="timestamp">Time of touch.</param>
	public static void LogInShelfTouch( string label, string timestamp){
		if (label != null ) {
			Firebase.Analytics.FirebaseAnalytics.LogEvent (
				"IN_APP_TOUCH",
				new Firebase.Analytics.Parameter[] {
					new Firebase.Analytics.Parameter (
						"TABLET_ID", tabID),
					new Firebase.Analytics.Parameter (
						"APP_ID", "0"),
					new Firebase.Analytics.Parameter (
						"LABEL", label),
					new Firebase.Analytics.Parameter (
						"TIME_OF_TOUCH", timestamp)
				}
			);
		}
	}

	/// <summary>
	/// Logs the IN_APP_RESPONSE event to firebase.
	/// IN_APP_RESPONSE event depicts response given to any question asked in the Tinkbook.
	/// </summary>
	/// <param name="selection">Selection by the user.</param>
	/// <param name="answer">Correct answer.</param>
	/// <param name="foilList">Foil list with all the wrong options.</param>
	/// <param name="timeElapsed">Time elapsed between when the question was asked and when selection occured (user chose answer from the options).</param>
	public static void LogInAppResponse(string selection, string answer,string foilList, string timeElapsed){

		if (answer != null ) {
			Firebase.Analytics.FirebaseAnalytics.LogEvent (
				"IN_APP_RESPONSE",
				new Firebase.Analytics.Parameter[] {
					new Firebase.Analytics.Parameter (
						"TABLET_ID", tabID),
					new Firebase.Analytics.Parameter (
						"APP_ID", appID),
					new Firebase.Analytics.Parameter (
						"SECTION_ID", secID),
					new Firebase.Analytics.Parameter (
						"SELECTION", selection),
					new Firebase.Analytics.Parameter (
						"CORRECT_ANSWER", answer),
					new Firebase.Analytics.Parameter (
						"FOIL", foilList),
					new Firebase.Analytics.Parameter (
						"TIME_OF_RESPONSE", timeElapsed)
				}
			);
		}
	}
}
