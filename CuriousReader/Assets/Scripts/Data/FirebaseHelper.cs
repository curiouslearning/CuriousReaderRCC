using UnityEngine;
using Firebase.Analytics;
using System.Collections.Generic;

/// <summary>
/// Script responsible for logging events to firebase.
/// </summary>
public static class FirebaseHelper {

	static int m_appID;
	static int m_sectionID;
	static int m_tabletID = 5001;

	/*
		Note: In case of adding a new EventName it must be added to EventName enumeration e.g "InAppEvent" and then the
		string "IN_APP_EVENT" to the m_eventNames array in the same order
	*/
	enum EventName { InAppSection, InAppTouch, InAppResponse };
	static readonly string[] m_eventNames = { "IN_APP_SECTION", "IN_APP_TOUCH", "IN_APP_RESPONSE" };

	/* 
		Note: In case of adding a new EventParam it must be added to EventParam enumeration e.g "AnswerTime" and then 
		the string "ANSWER_TIME" to the m_parameterNames array in the same order
	*/
	enum EventParam { 
		AppID, TabletID, SectionID, TimeEnter, TimeSpent, Label, TimeOfTouch, Selection, CorrectAnswer, 
		Foil, ResponseTime };
	static readonly string[] m_parameterNames = { 
		"APP_ID", "TABLET_ID", "SECTION_ID", "TIME_ENTER", "TIME_SPENT", "LABEL", "TIME_OF_TOUCH", "SELECTION", 
		"CORRECT_ANSWER", "FOIL", "TIME_OF_RESPONSE" };

	/// <summary>
	/// Sets appID to passed bookID
	/// </summary>
	/// <param name="i_bookID">Book ID Number</param>
	public static void AddBook(int i_bookID) {
		m_appID = i_bookID;
	}
		
	/// <summary>
	/// Assigns the value of page number to secID
	/// </summary>
	/// <param name="i_pageNumber">Page Number</param>
	public static void AddSection(int i_pageNumber) {
		m_sectionID = i_pageNumber;
	}

	/// <summary>
	/// IN_APP_SECTION depicts time spent on a particular page
	/// </summary>
	/// <param name="i_timeEnter">Time of enter into the page</param>
	/// <param name="i_timeSpent">Time spent on the page</param>
	public static void LogInAppSection(string i_timeEnter, double i_timeSpent) {
		if (i_timeEnter == null ) {
			Debug.LogWarning("LogInAppSection: timeEnter is null, not sending a requested log.");
			return;
		}
		logEventToFirebase(EventName.InAppSection, new Dictionary<EventParam, object>() {
			{ EventParam.TabletID, m_tabletID },
			{ EventParam.AppID, m_appID },
			{ EventParam.SectionID, m_sectionID },
			{ EventParam.TimeEnter, i_timeEnter },
			{ EventParam.TimeSpent, i_timeSpent }
		});
	}

	/// <summary>
	/// Logs IN_APP_SECTION event for shelf to firebase(special functions because shelf doesnot have section id!!!)
	/// </summary>
	/// <param name="i_timeEnter">Time of enter into the page</param>
	/// <param name="i_timeSpent">Time spent on the page</param>
	public static void LogInShelfSection(string i_timeEnter, double i_timeSpent) {
		if (i_timeEnter == null) {
			Debug.LogWarning("LogInShelfSection: timeEnter is null, not sending a requested log.");
			return;
		}
		logEventToFirebase(EventName.InAppSection, new Dictionary<EventParam, object>(){
			{ EventParam.TabletID, m_tabletID },
			{ EventParam.AppID, 0 },
			{ EventParam.TimeEnter, i_timeEnter },
			{ EventParam.TimeSpent, i_timeSpent }
		});
	}

	/// <summary>
	/// IN_APP_TOUCH event depicts the touch on any graphic, text, button
	/// </summary>
	/// <param name="i_label">Label of the element touched</param>
	/// <param name="i_timestamp">Time of touch</param>
	public static void LogInAppTouch(string i_label, string i_timestamp) {
		if (i_label == null ) {
			Debug.LogWarning("LogInAppTouch: label is null, not sending a requested log.");
			return;
		}
		logEventToFirebase(EventName.InAppTouch, new Dictionary<EventParam, object>(){
			{ EventParam.TabletID, m_tabletID },
			{ EventParam.AppID, m_appID },
			{ EventParam.SectionID, m_sectionID },
			{ EventParam.Label, i_label },
			{ EventParam.TimeOfTouch, i_timestamp }
		});
	}

	/// <summary>
	/// Logs IN_APP_TOUCH event for shelf to firebase(special functions because shelf doesnot have section id!!!).
	/// IN_APP_TOUCH event depicts the touch on any graphic, text, button
	/// </summary>
	/// <param name="label">Label of the element touched</param>
	/// <param name="timestamp">Time of touch</param>
	public static void LogInShelfTouch(string label, string timestamp) {
		if (label == null ) {
			Debug.Log("LogInShelfTouch: label is null, not sending a requested log.");
			return;
		}
		logEventToFirebase(EventName.InAppTouch, new Dictionary<EventParam, object>(){
			{ EventParam.TabletID, m_tabletID },
			{ EventParam.AppID, 0 },
			{ EventParam.Label, label },
			{ EventParam.TimeOfTouch, timestamp }
		});
	}

	/// <summary>
	/// IN_APP_RESPONSE event depicts response given to any question asked in the Tinkbook
	/// </summary>
	/// <param name="selection">Selection by the user</param>
	/// <param name="answer">Correct answer</param>
	/// <param name="failList">Fail list with all the wrong options</param>
	/// <param name="timeElapsed">Time elapsed between when the question was asked and when selection occured 
	/// (user chose answer from the options)</param>
	public static void LogInAppResponse(string selection, string answer, string failList, string timeElapsed) {
		if (answer == null) {
			Debug.LogWarning("LogInAppResponse: answer is null, not sending a requested log.");
			return;
		}
		logEventToFirebase(EventName.InAppResponse, new Dictionary<EventParam, object>(){
			{ EventParam.TabletID, m_tabletID },
			{ EventParam.AppID, m_appID },
			{ EventParam.SectionID, m_sectionID },
			{ EventParam.Selection, selection },
			{ EventParam.CorrectAnswer, answer },
			{ EventParam.Foil, failList },
			{ EventParam.ResponseTime, timeElapsed }
		});
	}

	///	<summary>
	/// Logs events to Firebase Analytics Database
	/// </summary
	/// <param name="i_eventName">Name of the event</param>
	/// <param name="i_eventParameters">
	/// Dictionary that consists of EventParam object value pairs e.g 
	/// { EventParam.AppID, 0 }</param>
	private static void logEventToFirebase(EventName i_eventName, Dictionary<EventParam, object> i_eventParameters) {
		List<Parameter> eventParameters = new List<Parameter>();
		foreach (var pair in i_eventParameters) {
			Parameter param = null;
			object parameterValue = pair.Value;
			string parameterName = m_parameterNames[(int)pair.Key];
			string valueType = pair.Value.GetType().ToString();
			switch(valueType) {
				case "System.Int16":	// short
				case "System.Int32":	// int
					param = new Parameter(parameterName, (long)(int)parameterValue);
					break;
				case "System.Int64":	// long
					param = new Parameter(parameterName, (long)parameterValue);
					break;
				case "System.Single":	// float
				case "System.Double":	// double
					param = new Parameter(parameterName, (double)parameterValue);
					break;
				case "System.String":
					param = new Parameter(parameterName, (string)parameterValue);
					break;
			}
			eventParameters.Add(param);
		}
		FirebaseAnalytics.LogEvent(m_eventNames[(int)i_eventName], eventParameters.ToArray());
	}

	#region Commented Old LogEvent

	// JSONNode dataToLog;
	// string dataAsJSON, path;

	//sending data directly to firebase using "72 hours rule"! (removed local data storage)
	/// <summary>
	/// Logs the event stored in the Local JSON to firebase and deletes from JSON simultaneously.
	/// </summary>
	// public void LogEvent(){

	// 	string label, time, timeEnter;
	// 	string answer, selection, foilList;
	// 	double timeSpent;
	// 	long count;

	// 	path = DataCollection.GetPath ();
	// 	if (File.Exists(path)) {
	// 		dataAsJSON = File.ReadAllText (path);
	// 		dataToLog = JSON.Parse (dataAsJSON);

	// 		if (dataToLog.Tag == JSONNodeType.Object)
	// 		{
	// 			// take each node under tablet id as an app's/book's value.
	// 			foreach (KeyValuePair<string, JSONNode> app in (JSONObject)dataToLog[tabID])
	// 			{
	// 				appID = int.Parse(app.Key);
	// 				Debug.Log (app+"");

	// 				//take each section/page under the app and take all the three events
	// 				foreach (KeyValuePair<string, JSONNode> section in (JSONObject)app.Value) {
	// 					secID =  int.Parse(section.Key);

	// 					// for all IN_APP_SECTION event, take the value of parameters and log the event to firebase
	// 					if (dataToLog [tabID] [app.Key] [section.Key] ["IN_APP_SECTION"] != null) {

	// 						count = (dataToLog [tabID] [app.Key] [section.Key] ["IN_APP_SECTION"]).Count;  
	// 						for (int i = 0; i < count; i++) {
	// 							timeEnter = dataToLog [tabID] [app.Key] [section.Key] ["IN_APP_SECTION"] [0] ["inTime"];
	// 							timeSpent = dataToLog [tabID] [app.Key] [section.Key] ["IN_APP_SECTION"] [0] ["timeSpent"];
	// 							//log the section data to firebase
	// 							LogInAppSection (timeEnter, timeSpent);
	// 							dataToLog [tabID] [app.Key] [section.Key] ["IN_APP_SECTION"].Remove (0);
	// 						}
	// 					}
	// 					// for all IN_APP_TOUCH event, take the value of parameters and log the event to firebase
	// 					if (dataToLog [tabID] [app.Key] [section.Key] ["IN_APP_TOUCH"] != null) {
	// 						count = (dataToLog [tabID] [app.Key] [section.Key] ["IN_APP_TOUCH"]).Count;
	// 						for (int i = 0; i < count; i++) {
	// 							label = dataToLog [tabID] [app.Key] [section.Key] ["IN_APP_TOUCH"] [0] ["label"];
	// 							time = dataToLog [tabID] [app.Key] [section.Key] ["IN_APP_TOUCH"] [0] ["time"];
	// 							LogInAppTouch ( label,time);
	// 							dataToLog [tabID] [app.Key] [section.Key] ["IN_APP_TOUCH"].Remove (0);
	// 						}
	// 					}
	// 					// for all IN_APP_RESPONSE event, take the value of parameters and log the event to firebase
	// 					if (dataToLog [tabID] [app.Key] [section.Key] ["IN_APP_RESPONSE"] != null) {
	// 						count = (dataToLog [tabID] [app.Key] [section.Key] ["IN_APP_RESPONSE"]).Count;
	// 						for (int i = 0; i < count; i++) {
	// 							answer = dataToLog [tabID] [app.Key] [section.Key] ["IN_APP_RESPONSE"] [0] ["answer"];
	// 							selection = dataToLog [tabID] [app.Key] [section.Key] ["IN_APP_RESPONSE"] [0] ["selection"];
	// 							time = dataToLog [tabID] [app.Key] [section.Key] ["IN_APP_RESPONSE"] [0] ["timeTaken"];
	// 							foilList = dataToLog [tabID] [app.Key] [section.Key] ["IN_APP_RESPONSE"] [0] ["foil"].ToString();
	// 							LogInAppResponse (selection,answer,foilList,time);
	// 							dataToLog [tabID] [app.Key] [section.Key] ["IN_APP_RESPONSE"].Remove (0);
	// 						}
	// 					}
	// 				}
	// 			}
	// 		}
	// 	}
	// 	else {
	// 		Debug.Log ("no file to log data!");		
 	// 	}
	// 	DataCollection.SaveLocalJSON (dataToLog);
	// }

	#endregion

}

